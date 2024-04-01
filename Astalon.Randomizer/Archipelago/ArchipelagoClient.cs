using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;

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
    public int Index { get; set; }
    public bool IsAstalon { get; set; }
}

public class ArchipelagoClient
{
    public const string ArchipelagoVersion = "0.4.4";

    public static bool Connected { get; private set; }
    private bool _attemptingConnection;

    public static ArchipelagoData ServerData { get; } = new();
    private DeathLinkHandler _deathLinkHandler;
    private ArchipelagoSession _session;
    private bool _ignoreLocations;
    private readonly Dictionary<long, ItemInfo> _locationCache = new();

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

        if (string.IsNullOrWhiteSpace(ServerData.Uri))
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
        _ignoreLocations = true;

        try
        {
            loginResult = _session.TryConnectAndLogin(
                Game.Name,
                ServerData.SlotName,
                ItemsHandlingFlags.AllItems,
                new(ArchipelagoVersion),
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
        if (!ServerData.SetupSession(login.SlotData, _session.RoomState.Seed))
        {
            Disconnect();
            return;
        }

        _ignoreLocations = false;
        _deathLinkHandler = new(_session.CreateDeathLinkService(), ServerData.SlotName, ServerData.SlotData.DeathLink);
        _attemptingConnection = false;
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
        ServerData.Clear();
        _locationCache.Clear();
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
            Plugin.Logger.LogWarning($"Trying to send location {location} when there's no connection");
            return;
        }

        var id = _session.Locations.GetLocationIdFromName(Game.Name, location);
        _session.Locations.CompleteLocationChecksAsync(id);
    }

    public bool SyncLocations(List<string> locations)
    {
        if (!Connected || locations == null || locations.Count == 0)
        {
            return false;
        }

        List<long> ids = new();
        foreach (var location in locations)
        {
            ids.Add(_session.Locations.GetLocationIdFromName(Game.Name, location));
        }

        Plugin.Logger.LogInfo($"Sending location checks: {string.Join(", ", locations)}");
        _session.Locations.CompleteLocationChecksAsync(ids.ToArray());
        return true;
    }

    public ItemInfo ScoutLocation(string name)
    {
        if (!Connected)
        {
            return null;
        }

        var id = _session.Locations.GetLocationIdFromName(Game.Name, name);
        return ScoutLocation(id);
    }

    public ItemInfo ScoutLocation(long id)
    {
        if (_locationCache.TryGetValue(id, out var cachedInfo))
        {
            return cachedInfo;
        }

        if (!Connected)
        {
            return null;
        }

        var scout = _session.Locations.ScoutLocationsAsync(id);
        scout.Wait();
        var networkItem = scout.Result.Locations[0];
        var itemName = _session.Items.GetItemName(networkItem.Item);
        var name = GetPlayerName(networkItem.Player);
        var itemInfo = new ItemInfo
        {
            Id = id,
            Name = itemName,
            Flags = networkItem.Flags,
            Player = networkItem.Player,
            PlayerName = name,
            IsLocal = networkItem.Player == GetCurrentPlayer(),
            LocationId = id,
            IsAstalon = GetPlayerGame(networkItem.Player) == Game.Name,
        };
        _locationCache[id] = itemInfo;
        return itemInfo;
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
        if (!Connected)
        {
            return;
        }

        var index = helper.Index - 1;
        var itemName = helper.PeekItemName();
        var item = helper.DequeueItem();
        Plugin.Logger.LogInfo($"Received item #{index}: {item.Item} - {itemName}");
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
            Index = index,
            IsAstalon = true,
        });
    }

    public void Session_CheckedLocationsUpdated(ReadOnlyCollection<long> newCheckedLocations)
    {
        if (_ignoreLocations || !Connected)
        {
            return;
        }

        Plugin.Logger.LogDebug($"New locations checked: {string.Join(", ", newCheckedLocations)}");
        foreach (var id in newCheckedLocations)
        {
            var itemInfo = ScoutLocation(id);
            if (itemInfo == null)
            {
                Plugin.Logger.LogWarning($"Scouting failed for location {id}");
                continue;
            }

            Plugin.Logger.LogInfo($"Checked location: {id} - {itemInfo.Name} for {itemInfo.PlayerName}");
            if (!itemInfo.IsLocal)
            {
                Game.IncomingMessages.Enqueue(itemInfo);
            }
        }
    }

    public string GetPlayerName(int slot)
    {
        if (!Connected)
        {
            return "";
        }

        var name = _session.Players.GetPlayerName(slot);
        if (string.IsNullOrWhiteSpace(name))
        {
            name = "Server";
        }

        return name;
    }

    public string GetPlayerGame(int slot)
    {
        if (!Connected)
        {
            return "";
        }

        return _session.Players.Players[_session.ConnectionInfo.Team].FirstOrDefault((p) => p.Slot == slot)
            ?.Game;
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
        if (Connected)
        {
            _deathLinkHandler.ToggleDeathLink();
        }
    }

    public bool DeathLinkEnabled()
    {
        if (!Connected)
        {
            return false;
        }

        return _deathLinkHandler.IsEnabled();
    }

    public void CheckForDeath()
    {
        if (Connected && Game.CanBeKilled())
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