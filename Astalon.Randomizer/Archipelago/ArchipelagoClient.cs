using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;

namespace Astalon.Randomizer.Archipelago;

public class ArchipelagoClient
{
    private const string MinArchipelagoVersion = "0.4.6";

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

    public Dictionary<long, ApItemInfo> ScoutAllLocations()
    {
        if (!Connected)
        {
            return null;
        }

        List<long> locations = new(_session.Locations.AllLocations);
        var scouts = _session.Locations.ScoutLocationsAsync(locations.ToArray()).ContinueWith(task =>
        {
            Dictionary<long, ApItemInfo> itemInfos = [];
            foreach (var entry in task.Result)
            {
                var itemName = entry.Value.ItemDisplayName;
                var isAstalon = entry.Value.ItemGame == Game.Name;
                if (isAstalon && Data.ItemNames.TryGetValue((ApItemId)entry.Value.ItemId, out var name))
                {
                    itemName = name;
                }

                itemInfos[entry.Key] = new ApItemInfo
                {
                    Id = entry.Value.ItemId,
                    Name = itemName,
                    Flags = entry.Value.Flags,
                    Player = entry.Value.Player,
                    PlayerName = entry.Value.Player.Name,
                    IsLocal = entry.Value.Player == GetCurrentPlayer(),
                    LocationId = entry.Key,
                    IsAstalon = isAstalon,
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

        _session.SetGoalAchieved();
    }

    public void Session_ItemReceived(IReceivedItemsHelper helper)
    {
        var index = helper.Index - 1;
        var item = helper.DequeueItem();
        var itemName = item.ItemName;
        if (Data.ItemNames.TryGetValue((ApItemId)item.ItemId, out var name))
        {
            itemName = name;
        }
        itemName ??= item.ItemDisplayName;

        Plugin.Logger.LogInfo($"Received item #{index}: {item.ItemId} - {itemName}");
        var player = item.Player;
        var playerName = player.Name;
        if (player.Alias != null && player.Alias != playerName)
        {
            playerName = $"{player.Alias} ({playerName})";
        }

        Game.IncomingItems.Enqueue(new()
        {
            Id = item.ItemId,
            Name = itemName,
            Flags = item.Flags,
            Player = player,
            PlayerName = playerName,
            IsLocal = player == GetCurrentPlayer(),
            LocationId = item.LocationId,
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
        _session?.Say(message);
    }
}
