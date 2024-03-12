using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using BepInEx;

namespace Astalon.Randomizer.Archipelago;

public class ItemInfo
{
    public long Id { get; set; }
    public string Name { get; set; }
    public ItemFlags Flags { get; set; }
    public int Player { get; set; }
    public string PlayerName { get; set; }
    public bool IsLocal { get; set; }
    public long LocationId { get; set; }
    public bool Receiving { get; set; }
}

public class ArchipelagoClient
{
    public const string ArchipelagoVersion = "0.4.4";

    public static bool Connected { get; private set; }
    private bool _attemptingConnection;

    public static ArchipelagoData ServerData { get; } = new();
    private DeathLinkHandler _deathLinkHandler;
    private ArchipelagoSession _session;

    public ArchipelagoClient(string uri, string slotName, string password)
    {
        ServerData.Uri = uri;
        ServerData.SlotName = slotName;
        ServerData.Password = password;
    }

    public void Connect()
    {
        if (Connected || _attemptingConnection)
        {
            return;
        }

        if (ServerData.Uri.IsNullOrWhiteSpace())
        {
            return;
        }

        try
        {
            _session = ArchipelagoSessionFactory.CreateSession(ServerData.Uri);
            SetupSession();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError(e);
        }

        TryConnect();
    }

    private void SetupSession()
    {
        _session.Socket.ErrorReceived += Session_ErrorReceived;
        _session.Socket.SocketClosed += Session_SocketClosed;
        _session.Items.ItemReceived += Session_ItemReceived;
        _session.Locations.CheckedLocationsUpdated += Session_CheckedLocationsUpdated;
        _session.MessageLog.OnMessageReceived += Session_OnMessageReceived;
    }

    private void TryConnect()
    {
        LoginResult loginResult;
        _attemptingConnection = true;

        try
        {
            loginResult = _session.TryConnectAndLogin(
                "Astalon",
                ServerData.SlotName,
                ItemsHandlingFlags.AllItems,
                new Version(ArchipelagoVersion),
                password: ServerData.Password,
                requestSlotData: true);
        }
        catch (Exception e)
        {
            loginResult = new LoginFailure(e.GetBaseException().Message);
        }

        if (loginResult is LoginFailure loginFailure)
        {
            Connected = false;
            _attemptingConnection = false;
            Plugin.Logger.LogError("AP connection failed: " + string.Join("\n", loginFailure.Errors));
            _session = null;
            return;
        }

        var login = loginResult as LoginSuccessful;
        Connected = true;
        Plugin.Logger.LogInfo($"Successfully connected to {ServerData.Uri} as {ServerData.SlotName}");
        OnConnect(login);
    }

    public void OnConnect(LoginSuccessful login)
    {
        ServerData.SetupSession(login.SlotData, _session.RoomState.Seed);
        _deathLinkHandler = new(_session.CreateDeathLinkService(), ServerData.SlotName, ServerData.SlotData.DeathLink);
        _attemptingConnection = false;
        Game.InitializeSave();
    }

    public void Disconnect()
    {
        if (!Connected)
        {
            return;
        }

        Connected = false;
        _attemptingConnection = false;
        Task.Run(() => { _session.Socket.DisconnectAsync(); }).Wait();
        _session = null;
    }

    public void Session_SocketClosed(string reason)
    {
        Plugin.Logger.LogError("Connection to Archipelago lost: " + reason);
        Disconnect();
    }

    public void Session_ErrorReceived(Exception e, string message)
    {
        Plugin.Logger.LogError(message);
        if (e != null)
        {
            Plugin.Logger.LogError(e.ToString());
        }

        Disconnect();
    }

    public void Session_OnMessageReceived(LogMessage message)
    {
        Plugin.Logger.LogMessage(message);
        ArchipelagoConsole.LogMessage(message.ToString());
    }

    public void SendLocation(string location)
    {
        if (!Connected)
        {
            return;
        }

        var id = _session.Locations.GetLocationIdFromName("Astalon", location);
        _session.Locations.CompleteLocationChecksAsync(id);
    }

    public ItemInfo ScoutLocation(string location)
    {
        if (!Connected)
        {
            return null;
        }

        var id = _session.Locations.GetLocationIdFromName("Astalon", location);
        return ScoutLocation(id);
    }

    public ItemInfo ScoutLocation(long id)
    {
        if (!Connected)
        {
            return null;
        }

        var scout = _session.Locations.ScoutLocationsAsync(id);
        scout.Wait();
        var networkItem = scout.Result.Locations[0];
        var itemName = _session.Items.GetItemName(networkItem.Item);
        var name = GetPlayerName(networkItem.Player);
        return new()
        {
            Id = id,
            Name = itemName,
            Flags = networkItem.Flags,
            Player = networkItem.Player,
            PlayerName = name,
            IsLocal = networkItem.Player == GetCurrentPlayer(),
            LocationId = id,
        };
    }

    public void SendCompletion()
    {
        if (!Connected)
        {
            return;
        }

        var packet = new StatusUpdatePacket
        {
            Status = ArchipelagoClientState.ClientGoal,
        };
        _session.Socket.SendPacket(packet);
    }

    public void Session_ItemReceived(ReceivedItemsHelper helper)
    {
        var itemName = helper.PeekItemName();
        var item = helper.DequeueItem();
        Plugin.Logger.LogInfo($"Received item: {item.Item} - {itemName}");
        var player = item.Player;
        Game.IncomingItems.Enqueue(new()
        {
            Id = item.Item,
            Name = itemName,
            Flags = item.Flags,
            Player = player,
            PlayerName = GetPlayerName(player),
            IsLocal = player == GetCurrentPlayer(),
            LocationId = item.Location,
            Receiving = true,
        });
    }

    public void Session_CheckedLocationsUpdated(ReadOnlyCollection<long> newCheckedLocations)
    {
        foreach (var id in newCheckedLocations)
        {
            var itemInfo = ScoutLocation(id);
            Plugin.Logger.LogInfo($"Checked location: {id} - {itemInfo.Name} for {itemInfo.PlayerName}");
            if (itemInfo is { IsLocal: false })
            {
                Game.IncomingMessages.Enqueue(itemInfo);
            }
        }
    }

    public string GetPlayerName(int slot)
    {
        var name = _session.Players.GetPlayerName(slot);
        if (name.IsNullOrWhiteSpace())
        {
            name = "Server";
        }

        return name;
    }

    public int GetCurrentPlayer()
    {
        if (!Connected)
        {
            return -1;
        }

        return _session.ConnectionInfo.Slot;
    }

    public void SendDeath()
    {
        if (Connected)
        {
            _deathLinkHandler.SendDeathLink();
        }
    }

    public void ToggleDeathLink()
    {
        _deathLinkHandler.ToggleDeathLink();
    }

    public bool DeathLinkEnabled()
    {
        return _deathLinkHandler.IsEnabled();
    }

    public void CheckForDeath()
    {
        if (Game.CanBeKilled())
        {
            _deathLinkHandler.KillPlayer();
        }
    }

    public void SendMessage(string message)
    {
        if (Connected)
        {
            _session.Socket.SendPacketAsync(new SayPacket { Text = message });
        }
    }
}