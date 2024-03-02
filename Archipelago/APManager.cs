using System;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using Newtonsoft.Json.Linq;

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
}

public class APManager
{
    public int[] AP_VERSION = new int[] { 0, 4, 4 };
    public bool Connected;
    public ArchipelagoSession Session;
    public DeathLinkService DeathLink;
    public bool ReceivingItem;

    class Config
    {
        public bool RandomizeAttackPickups { get; set; }
        public bool RandomizeHealthPickups { get; set; }
    }

    public bool Connect()
    {
        if (Connected)
        {
            return true;
        }

        if (Main.Settings.Address is null || Main.Settings.Address.Length == 0)
        {
            return false;
        }

        Session = ArchipelagoSessionFactory.CreateSession(Main.Settings.Address, Main.Settings.Port);
        Session.Socket.ErrorReceived += Session_ErrorReceived;
        Session.Socket.SocketClosed += Session_SocketClosed;
        Session.Items.ItemReceived += Session_ReceiveItem;
        // add Session.Locations.CheckedLocationsUpdated

        LoginResult loginResult;

        try
        {
            loginResult = Session.TryConnectAndLogin(
                "Astalon",
                Main.Settings.SlotName,
                ItemsHandlingFlags.AllItems,
                new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
                null,
                null,
                Main.Settings.Password,
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
        var config = ((JObject)login.SlotData["settings"]).ToObject<Config>();
        Main.Settings.RandomizeAttackPickups = config.RandomizeAttackPickups;
        Main.Settings.RandomizeHealthPickups = config.RandomizeHealthPickups;

        DeathLink = Session.CreateDeathLinkService();
        DeathLink.OnDeathLinkReceived += ReceiveDeath;
        // if (deathLinkEnabled) {
        //     DeathLink.EnableDeathLink();
        // } else {
        //     DeathLink.DisableDeathLink();
        // }
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
        Session.Locations.CompleteLocationChecks(id);
        Main.Log.LogInfo($"Found item: {id}");
    }

    public ItemInfo ScoutLocation(string location)
    {
        if (!Connected)
        {
            return null;
        }

        var id = Session.Locations.GetLocationIdFromName("Astalon", location);
        var scout = Session.Locations.ScoutLocationsAsync(id);
        scout.Wait();
        var locationInfo = scout.Result.Locations[0];
        var itemName = Session.Items.GetItemName(locationInfo.Item);
        var name = GetPlayerName(locationInfo.Player);
        return new ItemInfo
        {
            ID = id,
            Name = itemName,
            Flags = locationInfo.Flags,
            Player = locationInfo.Player,
            PlayerName = name,
            IsLocal = locationInfo.Player == GetCurrentPlayer(),
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

    public void Session_ReceiveItem(ReceivedItemsHelper helper)
    {
        var item = helper.PeekItem();
        var itemName = helper.PeekItemName();
        Main.Log.LogInfo($"Received item: {itemName}");
        var player = helper.PeekItem().Player;
        var itemInfo = new ItemInfo
        {
            ID = item.Item,
            Name = itemName,
            Flags = item.Flags,
            Player = player,
            PlayerName = GetPlayerName(player),
            IsLocal = player == GetCurrentPlayer(),
            LocationID = item.Location,
        };
        ReceivingItem = true;
        if (Game.GiveItem(itemInfo, display: !itemInfo.IsLocal))
        {
            helper.DequeueItem();
        }
        ReceivingItem = false;
    }

    public string GetPlayerName(int slot)
    {
        var name = Session.Players.GetPlayerName(slot);
        if (name == "" || name is null)
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
        if (Connected)
        {
            var name = Session.Players.GetPlayerAliasAndName(Session.ConnectionInfo.Slot);
            DeathLink.SendDeathLink(new DeathLink(name));
        }
    }

    public void ReceiveDeath(DeathLink link)
    {
        // TODO
    }
}
