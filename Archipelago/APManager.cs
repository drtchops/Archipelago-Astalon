using System;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;

namespace Archipelago;

public class ItemInfo
{
    public long ID { get; set; }
    public string Name { get; set; }
    public ItemFlags Flags { get; set; }
    public int Player { get; set; }
    public string PlayerName { get; set; }
    public bool IsLocal { get; set; }
    public long LocationID { get; set; }
    public bool Receiving { get; set; }
}

public class APManager
{
    public int[] AP_VERSION = new []{ 0, 4, 4 };
    public bool Connected;
    public ArchipelagoSession Session;
    public DeathLinkService DeathLink;

    public bool Connect()
    {
        if (Connected)
        {
            return true;
        }

        if (Settings.Address == null || Settings.Address.Length == 0)
        {
            return false;
        }

        Session = ArchipelagoSessionFactory.CreateSession(Settings.Address, Settings.Port);
        Session.Socket.ErrorReceived += Session_ErrorReceived;
        Session.Socket.SocketClosed += Session_SocketClosed;
        Session.Items.ItemReceived += Session_ItemReceived;
        Session.Locations.CheckedLocationsUpdated += Session_CheckedLocationsUpdated;

        LoginResult loginResult;

        try
        {
            loginResult = Session.TryConnectAndLogin(
                "Astalon",
                Settings.SlotName,
                ItemsHandlingFlags.AllItems,
                new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
                null,
                null,
                Settings.Password,
                true);
        }
        catch (Exception e)
        {
            loginResult = new LoginFailure(e.GetBaseException().Message);
        }

        if (loginResult is LoginFailure loginFailure)
        {
            Connected = false;
            Main.Log.LogError("AP connection failed: " + string.Join("\n", loginFailure.Errors));
            Session = null;
            return false;
        }

        var login = loginResult as LoginSuccessful;
        Connected = true;
        Main.Log.LogInfo("AP connection successful");
        OnConnect(login);

        return login.Successful;
    }

    public void OnConnect(LoginSuccessful login)
    {
        var config = (JObject)login.SlotData["settings"];
        Settings.RandomizeAttackPickups = (bool)config["randomize_attack_pickups"];
        Settings.RandomizeHealthPickups = (bool)config["randomize_health_pickups"];
        //Settings.RandomizeWhiteKeys = (bool)config["randomize_white_keys"];
        //Settings.RandomizeBlueKeys = (bool)config["randomize_blue_keys"];
        Settings.RandomizeRedKeys = (bool)config["randomize_red_keys"];
        //Settings.RandomizeFamiliars = (bool)config["randomize_familiars"];
        Settings.SkipCutscenes = (bool)config["skip_cutscenes"];
        Settings.StartWithZeek = (bool)config["start_with_zeek"];
        Settings.StartWithBram = (bool)config["start_with_bram"];
        Settings.StartWithQOL = (bool)config["start_with_qol"];
        Settings.FreeApexElevator = (bool)config["free_apex_elevator"];
        Settings.CostMultiplier = (int)config["cost_multiplier"];
        Settings.DeathLink = (bool)config["death_link"];

        DeathLink = Session.CreateDeathLinkService();
        DeathLink.OnDeathLinkReceived += ReceiveDeath;
        if (Settings.DeathLink)
        {
            DeathLink.EnableDeathLink();
        }
        else
        {
            DeathLink.DisableDeathLink();
        }

        Game.InitializeSave();
    }

    public void Session_SocketClosed(string reason)
    {
        Main.Log.LogError("Connection to Archipelago lost: " + reason);
        Disconnect();
    }

    public void Session_ErrorReceived(Exception e, string message)
    {
        Main.Log.LogError(message);
        if (e != null)
        {
            Main.Log.LogError(e.ToString());
        }
        Disconnect();
    }

    public void Disconnect()
    {
        if (Connected)
        {
            Connected = false;
            Task.Run(() => { Session.Socket.DisconnectAsync(); }).Wait();
            Session = null;
        }
    }

    public void SendLocation(string location)
    {
        if (!Connected)
        {
            return;
        }

        var id = Session.Locations.GetLocationIdFromName("Astalon", location);
        Session.Locations.CompleteLocationChecksAsync(id);
        Main.Log.LogInfo($"Found item: {id}");
    }

    public ItemInfo ScoutLocation(string location)
    {
        if (!Connected)
        {
            return null;
        }

        var id = Session.Locations.GetLocationIdFromName("Astalon", location);
        return ScoutLocation(id);
    }

    public ItemInfo ScoutLocation(long id)
    {
        if (!Connected)
        {
            return null;
        }

        var scout = Session.Locations.ScoutLocationsAsync(id);
        scout.Wait();
        var networkItem = scout.Result.Locations[0];
        var itemName = Session.Items.GetItemName(networkItem.Item);
        var name = GetPlayerName(networkItem.Player);
        return new ItemInfo
        {
            ID = id,
            Name = itemName,
            Flags = networkItem.Flags,
            Player = networkItem.Player,
            PlayerName = name,
            IsLocal = networkItem.Player == GetCurrentPlayer(),
            LocationID = id,
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
            Status = ArchipelagoClientState.ClientGoal
        };
        Session.Socket.SendPacket(packet);
    }

    public void Session_ItemReceived(ReceivedItemsHelper helper)
    {
        var itemName = helper.PeekItemName();
        var item = helper.DequeueItem();
        Main.Log.LogInfo($"Received item: {item.Item} - {itemName}");
        var player = item.Player;
        var itemInfo = new ItemInfo
        {
            ID = item.Item,
            Name = itemName,
            Flags = item.Flags,
            Player = player,
            PlayerName = GetPlayerName(player),
            IsLocal = player == GetCurrentPlayer(),
            LocationID = item.Location,
            Receiving = true,
        };
        Game.IncomingItems.Enqueue(itemInfo);
    }

    public void Session_CheckedLocationsUpdated(ReadOnlyCollection<long> newCheckedLocations)
    {
        Main.Log.LogInfo($"new locations checked: {string.Join(", ", newCheckedLocations)}");
        foreach (var id in newCheckedLocations)
        {
            var itemInfo = ScoutLocation(id);
            if (itemInfo != null && !itemInfo.IsLocal)
            {
                Game.IncomingMessages.Enqueue(itemInfo);
            }
        }
    }

    public string GetPlayerName(int slot)
    {
        var name = Session.Players.GetPlayerName(slot);
        if (name == "" || name == null)
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
        return Session.ConnectionInfo.Slot;
    }

    public void SendDeath()
    {
        if (Connected && Settings.DeathLink)
        {
            var name = Session.Players.GetPlayerName(GetCurrentPlayer());
            DeathLink.SendDeathLink(new DeathLink(name));
        }
    }

    public void ReceiveDeath(DeathLink link)
    {
        Game.DeathSource = link.Source;
    }

    public void ToggleDeathLink()
    {
        if (Settings.DeathLink)
        {
            Main.Log.LogInfo("Disabling death link");
            DeathLink.DisableDeathLink();
            Settings.DeathLink = false;
        }
        else
        {
            Main.Log.LogInfo("Enabling death link");
            DeathLink.EnableDeathLink();
            Settings.DeathLink = true;
        }
    }
}
