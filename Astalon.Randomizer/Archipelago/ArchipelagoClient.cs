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

public class ArchipelagoClient
{
    private const string MinArchipelagoVersion = "0.4.4";

    public bool Connected => _session?.Socket.Connected ?? false;
    private bool _attemptingConnection;

    private DeathLinkHandler _deathLinkHandler;
    private ArchipelagoSession _session;
    private bool _ignoreLocations;

    public void Connect()
    {
        if (Connected || _attemptingConnection)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Plugin.State.Uri) || string.IsNullOrWhiteSpace(Plugin.State.SlotName))
        {
            return;
        }

        try
        {
            _session = ArchipelagoSessionFactory.CreateSession(Plugin.State.Uri);
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
                Plugin.State.SlotName,
                ItemsHandlingFlags.AllItems,
                new(MinArchipelagoVersion),
                password: Plugin.State.Password,
                requestSlotData: true);
        }
        catch (Exception e)
        {
            loginResult = new LoginFailure(e.GetBaseException().Message);
        }

        if (loginResult is LoginFailure loginFailure)
        {
            _attemptingConnection = false;
            Plugin.Logger.LogError("AP connection failed: " + string.Join("\n", loginFailure.Errors));
            _session = null;
            return;
        }

        var login = loginResult as LoginSuccessful;
        Plugin.Logger.LogInfo($"Successfully connected to {Plugin.State.Uri} as {Plugin.State.SlotName}");
        OnConnect(login);
    }

    private void OnConnect(LoginSuccessful login)
    {
        if (!Plugin.State.SetupSession(login.SlotData, _session.RoomState.Seed))
        {
            return;
        }

        _ignoreLocations = false;
        _deathLinkHandler = new(_session.CreateDeathLinkService(), Plugin.State.SlotName, Plugin.State.SlotData.DeathLink);
        _attemptingConnection = false;
    }

    public void Disconnect()
    {
        if (!Connected)
        {
            return;
        }

        _attemptingConnection = false;
        Task.Run(() => { _session.Socket.DisconnectAsync(); }).Wait();
        _deathLinkHandler = null;
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

    public void SendLocation(long location)
    {
        if (!Connected)
        {
            Plugin.Logger.LogWarning($"Trying to send location {location} when there's no connection");
            return;
        }

        _session.Locations.CompleteLocationChecksAsync(location);
    }

    public bool SyncLocations(List<long> locations)
    {
        if (!Connected || locations == null || locations.Count == 0)
        {
            return false;
        }

        Plugin.Logger.LogInfo($"Sending location checks: {string.Join(", ", locations)}");
        _session.Locations.CompleteLocationChecksAsync(locations.ToArray());
        return true;
    }

    public Dictionary<long, ItemInfo> ScoutAllLocations()
    {
        if (!Connected)
        {
            return null;
        }

        List<long> locations = new(_session.Locations.AllLocations);
        var scouts = _session.Locations.ScoutLocationsAsync(locations.ToArray()).ContinueWith(task =>
        {
            Dictionary<long, ItemInfo> itemInfos = [];
            foreach (var networkItem in task.Result.Locations)
            {
                var itemName = _session.Items.GetItemName(networkItem.Item);
                itemInfos[networkItem.Item] = new ItemInfo
                {
                    Id = networkItem.Item,
                    Name = itemName,
                    Flags = networkItem.Flags,
                    Player = networkItem.Player,
                    PlayerName = GetPlayerName(networkItem.Player),
                    IsLocal = networkItem.Player == GetCurrentPlayer(),
                    LocationId = networkItem.Location,
                    IsAstalon = GetPlayerGame(networkItem.Player) == Game.Name,
                };
            }
            return itemInfos;
        });
        scouts.Wait();
        return scouts.Result;
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
        if (_ignoreLocations)
        {
            return;
        }

        Plugin.Logger.LogDebug($"New locations checked: {string.Join(", ", newCheckedLocations)}");
        foreach (var id in newCheckedLocations)
        {
            if (Plugin.State.LocationInfos.TryGetValue(id, out var itemInfo))
            {
                Plugin.Logger.LogInfo($"Checked location: {id} - {itemInfo.Name} for {itemInfo.PlayerName}");
                if (!itemInfo.IsLocal)
                {
                    Game.IncomingMessages.Enqueue(itemInfo);
                }
            }
            else
            {
                Plugin.Logger.LogWarning($"Scouting failed for location {id}");
                continue;
            }

        }
    }

    public string GetPlayerName(int slot)
    {
        if (!Connected)
        {
            return "";
        }

        var name = _session.Players.GetPlayerAliasAndName(slot);
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

        return _session.Players.Players[_session.ConnectionInfo.Team].FirstOrDefault((p) => p.Slot == slot)?.Game;
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
        _deathLinkHandler?.SendDeathLink();
    }

    public void ToggleDeathLink()
    {
        _deathLinkHandler?.ToggleDeathLink();
    }

    public bool DeathLinkEnabled()
    {
        return _deathLinkHandler?.IsEnabled() ?? false;
    }

    public void CheckForDeath()
    {
        if (Game.CanBeKilled())
        {
            _deathLinkHandler?.KillPlayer();
        }
    }

    public void SendMessage(string message)
    {
        _session?.Socket.SendPacketAsync(new SayPacket { Text = message });
    }
}
