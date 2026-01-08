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
using Newtonsoft.Json.Linq;

namespace Archipelago.Astalon.Archipelago;

public readonly struct PlayerCoords
{
    public int Room { get; init; }
    public int Area { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
    public int Character { get; init; }
}

public class ArchipelagoClient
{
    private const string MinArchipelagoVersion = "0.6.0";
    private const string CampfiresDSKey = "campfires";

    public bool Connected => _session?.Socket.Connected ?? false;
    private bool _attemptingConnection;

    private DeathLinkHandler _deathLinkHandler;
    private TagLinkHandler _tagLinkHandler;
    private ArchipelagoSession _session;
    private bool _ignoreLocations;

    private readonly string _clientId = Guid.NewGuid().ToString();

    public void Connect()
    {
        if (Connected || _attemptingConnection)
        {
            return;
        }

        if (
            string.IsNullOrWhiteSpace(Plugin.State.Uri)
            || string.IsNullOrWhiteSpace(Plugin.State.SlotName)
        )
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
        _session.Socket.ErrorReceived += SessionErrorReceived;
        _session.Socket.SocketClosed += SessionSocketClosed;
        _session.Items.ItemReceived += SessionItemReceived;
        _session.Locations.CheckedLocationsUpdated += SessionCheckedLocationsUpdated;
        _session.MessageLog.OnMessageReceived += SessionOnMessageReceived;
        _session.DataStorage[Scope.Slot, CampfiresDSKey].OnValueChanged += OnUpdateVisitedCampfires;
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
                requestSlotData: true
            );
        }
        catch (Exception e)
        {
            loginResult = new LoginFailure(e.GetBaseException().Message);
        }

        if (loginResult is LoginFailure loginFailure)
        {
            _attemptingConnection = false;
            Plugin.Logger.LogError(
                "AP connection failed: " + string.Join("\n", loginFailure.Errors)
            );
            _session = null;
            return;
        }

        var login = loginResult as LoginSuccessful;
        Plugin.Logger.LogInfo(
            $"Successfully connected to {Plugin.State.Uri} as {Plugin.State.SlotName}"
        );
        OnConnect(login);
    }

    private void OnConnect(LoginSuccessful login)
    {
        if (!Plugin.State.SetupSession(login.SlotData, _session.RoomState.Seed))
        {
            return;
        }

        _ignoreLocations = false;
        _deathLinkHandler = new(
            _session.CreateDeathLinkService(),
            Plugin.State.SlotName,
            Plugin.State.SlotData.DeathLink
        );
        _tagLinkHandler = new(
            _session.CreateTagLinkService(),
            _clientId,
            Plugin.State.SlotData.TagLink
        );
        _attemptingConnection = false;
        _session.DataStorage[Scope.Slot, CampfiresDSKey].Initialize(Array.Empty<int>());
    }

    public void Disconnect()
    {
        if (!Connected)
        {
            return;
        }

        _attemptingConnection = false;
        Task.Run(() =>
            {
                _ = _session.Socket.DisconnectAsync();
            })
            .Wait();
        _deathLinkHandler = null;
        _tagLinkHandler = null;
        _session = null;
    }

    public void SessionSocketClosed(string reason)
    {
        Plugin.Logger.LogError("Connection to Archipelago lost: " + reason);
        Disconnect();
    }

    public void SessionErrorReceived(Exception e, string message)
    {
        Plugin.Logger.LogError(message);
        if (e != null)
        {
            Plugin.Logger.LogError(e.ToString());
        }

        Disconnect();
    }

    public static void SessionOnMessageReceived(LogMessage message)
    {
        Plugin.Logger.LogMessage(message);
        ArchipelagoConsole.LogMessage(message.ToString());
    }

    public void SendLocation(long location)
    {
        if (!Connected)
        {
            Plugin.Logger.LogWarning(
                $"Trying to send location {location} when there's no connection"
            );
            return;
        }

        _ = _session.Locations.CompleteLocationChecksAsync(location);
    }

    public bool IsLocationChecked(long location)
    {
        return Connected && _session.Locations.AllLocationsChecked.Contains(location);
    }

    public bool SyncLocations(List<long> locations)
    {
        if (!Connected || locations == null || locations.Count == 0)
        {
            return false;
        }

        Plugin.Logger.LogInfo($"Sending location checks: {string.Join(", ", locations)}");
        _ = _session.Locations.CompleteLocationChecksAsync([.. locations]);
        return true;
    }

    public Dictionary<long, ApItemInfo> ScoutAllLocations()
    {
        if (!Connected)
        {
            return null;
        }

        List<long> locations = [.. _session.Locations.AllLocations];
        var scouts = _session
            .Locations.ScoutLocationsAsync([.. locations])
            .ContinueWith(task =>
            {
                Dictionary<long, ApItemInfo> itemInfos = [];
                foreach (var entry in task.Result)
                {
                    var itemName = entry.Value.ItemDisplayName;
                    var isAstalon = entry.Value.ItemGame == Game.Name;
                    if (
                        isAstalon
                        && Data.ItemNames.TryGetValue((ApItemId)entry.Value.ItemId, out var name)
                    )
                    {
                        itemName = name;
                    }

                    var player = entry.Value.Player;
                    var playerName = player.Alias ?? player.Name ?? $"Player #{player.Slot}";

                    itemInfos[entry.Key] = new ApItemInfo
                    {
                        Id = entry.Value.ItemId,
                        Name = itemName,
                        Flags = entry.Value.Flags,
                        Player = player,
                        PlayerName = playerName,
                        IsLocal = player == GetCurrentPlayer(),
                        LocationId = entry.Key,
                        IsAstalon = isAstalon,
                    };
                }
                return itemInfos;
            });
        scouts.Wait();
        return scouts.Result;
    }

    public bool HintLocations(List<long> locations)
    {
        if (!Connected || locations == null || locations.Count == 0)
        {
            return false;
        }

        Plugin.Logger.LogInfo($"Creating hints for locations: {string.Join(", ", locations)}");
        _session.Hints.CreateHints(locationIds: [.. locations]);
        return true;
    }

    public void SendCompletion()
    {
        if (!Connected)
        {
            return;
        }

        _session.SetGoalAchieved();
    }

    public void SessionItemReceived(IReceivedItemsHelper helper)
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
        var playerName = player.Alias ?? player.Name ?? $"Player #{player.Slot}";

        Game.IncomingItems.Enqueue(
            new()
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
            }
        );
    }

    public void SessionCheckedLocationsUpdated(ReadOnlyCollection<long> newCheckedLocations)
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
                Plugin.Logger.LogInfo(
                    $"Checked location: {id} - {itemInfo.Name} for {itemInfo.PlayerName}"
                );
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
        return !Connected ? -1 : _session.ConnectionInfo.Slot;
    }

    public void SendDeath()
    {
        _deathLinkHandler?.SendDeathLink();
    }

    public void ToggleDeathLink()
    {
        _deathLinkHandler?.ToggleDeathLink();
    }

    public static bool DeathLinkEnabled()
    {
        return DeathLinkHandler.IsEnabled();
    }

    public void SendTag(int character)
    {
        _tagLinkHandler?.SendTagLink(character);
    }

    public void ToggleTagLink()
    {
        _tagLinkHandler?.ToggleTagLink();
    }

    public static bool TagLinkEnabled()
    {
        return TagLinkHandler.IsEnabled();
    }

    public void CheckForTag()
    {
        if (Game.CanCycleCharacter())
        {
            _tagLinkHandler?.TagPlayer();
        }
    }

    public void SendMessage(string message)
    {
        _session?.Say(message);
    }

    public void StorePosition(int area, int room, int x, int y, int character)
    {
        if (!Connected)
        {
            return;
        }

        _session.DataStorage[
            $"{_session.ConnectionInfo.Slot}_{_session.ConnectionInfo.Team}_astalon_room"
        ] = room;

        PlayerCoords coords = new()
        {
            Room = room,
            Area = area,
            X = x,
            Y = y,
            Character = character,
        };
        _session.DataStorage[
            $"{_session.ConnectionInfo.Slot}_{_session.ConnectionInfo.Team}_astalon_coords"
        ] = JObject.FromObject(coords);

        if (area is not 0 and not 22)
        {
            _session.DataStorage[
                $"{_session.ConnectionInfo.Slot}_{_session.ConnectionInfo.Team}_astalon_area"
            ] = area;
        }
    }

    public void SyncVisitedCampfires(List<int> ids)
    {
        if (!Connected)
        {
            return;
        }

        _session.DataStorage[Scope.Slot, CampfiresDSKey] = ids;
    }

    private static void OnUpdateVisitedCampfires(
        JToken originalValue,
        JToken newValue,
        Dictionary<string, JToken> additionalArguments
    )
    {
        var ids = newValue.ToObject<List<int>>();
        Game.AddVisitedCampfires(ids);
    }

    public void LoadCampfires()
    {
        try
        {
            var ids = _session.DataStorage[Scope.Slot, CampfiresDSKey].To<List<int>>();
            Game.AddVisitedCampfires(ids);
        }
        catch (Exception ex)
        {
            Plugin.Logger.LogError($"Unexpected issue unlocking campfires from server data: {ex}");
        }
    }

    public int CountItem(long id)
    {
        return !Connected ? 0 : _session.Items.AllItemsReceived.Count((item) => item.ItemId == id);
    }
}
