using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BepInEx.Logging;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Packets;
using Archipelago.MultiClient.Net.Helpers;

namespace Archipelago;

public static class APState
{
    public static ManualLogSource Log = Logger.CreateLogSource("APState");

    public static int[] AP_VERSION = new int[] { 0, 4, 4 };
    public static List<string> message_queue = new();
    public static bool Authenticated;
    public static bool Silent = false;
    public static ArchipelagoSession Session;

    public static Dictionary<string, int> itemIds;

    public static bool Connect()
    {
        if (Authenticated)
        {
            return true;
        }

        if (Plugin.config.address is null || Plugin.config.address.Length == 0)
        {
            return false;
        }

        Session = ArchipelagoSessionFactory.CreateSession(Plugin.config.address, Plugin.config.port);
        Session.MessageLog.OnMessageReceived += Session_MessageReceived;
        Session.Socket.ErrorReceived += Session_ErrorReceived;
        Session.Socket.SocketClosed += Session_SocketClosed;
        Session.Items.ItemReceived += Session_ReceiveItem;

        LoginResult loginResult;

        try
        {
            loginResult = Session.TryConnectAndLogin(
                "Astalon",
                Plugin.config.slotName,
                ItemsHandlingFlags.AllItems,
                new Version(AP_VERSION[0], AP_VERSION[1], AP_VERSION[2]),
                null,
                "",
                Plugin.config.password);
        }
        catch (Exception e)
        {
            loginResult = new LoginFailure(e.GetBaseException().Message);
        }

        if (loginResult is LoginSuccessful loginSuccess)
        {
            Authenticated = true;
            // slot data here
            Log.LogInfo("AP connection successful");
        }
        else if (loginResult is LoginFailure loginFailure)
        {
            Authenticated = false;
            Log.LogError(String.Join("\n", loginFailure.Errors));
            Session = null;
        }

        return loginResult.Successful;
    }

    static void Session_SocketClosed(string reason)
    {
        message_queue.Add("Connection to Archipelago lost: " + reason);
        Log.LogError("Connection to Archipelago lost: " + reason);
        Disconnect();
    }

    static void Session_MessageReceived(LogMessage message)
    {
        Log.LogInfo(message.ToString());
        if (!Silent)
        {
            message_queue.Add(message.ToString());
        }
    }

    static void Session_ErrorReceived(Exception e, string message)
    {
        Log.LogError(message);
        if (e != null)
        {
            Log.LogError(e.ToString());
        }
        Disconnect();
    }

    public static void Disconnect()
    {
        Authenticated = false;
        if (Session != null && Session.Socket != null && Session.Socket.Connected)
        {
            Task.Run(() => { Session.Socket.DisconnectAsync(); }).Wait();
        }
        Session = null;
    }

    public static void SendLocation(string location)
    {
        var id = Session.Locations.GetLocationIdFromName("Astalon", location);
        // var scout = Session.Locations.ScoutLocationsAsync(id);
        Session.Locations.CompleteLocationChecks(id);
        // scout.Wait();
        // var locationInfo = scout.Result.Locations[0];
        // var message = Session.Items.GetItemName(locationInfo.Item);
        // if (locationInfo.Player != Session.ConnectionInfo.Slot)
        // {
        //     var name = Session.Players.GetPlayerName(locationInfo.Player);
        //     message = $"{name}'s {message}";
        // }
        Log.LogInfo($"Found item: {id}");
        // Archipelago.DisplayItem(message);
    }

    public static void SendCompletion()
    {
        var packet = new StatusUpdatePacket();
        packet.Status = ArchipelagoClientState.ClientGoal;
        Session.Socket.SendPacket(packet);
    }

    public static void Session_ReceiveItem(ReceivedItemsHelper helper)
    {
        var itemName = helper.PeekItemName();
        Log.LogInfo($"Received item: {itemName}");
        var message = itemName;
        var player = helper.PeekItem().Player;
        var local = true;
        if (player != Session.ConnectionInfo.Slot)
        {
            local = false;
            var name = Session.Players.GetPlayerName(player);
            if (name == "" || name is null)
            {
                name = "Server";
            }
            message = $"{name}'s {message}";
        }
        helper.DequeueItem();
        Archipelago.GiveItem(itemName);
        Archipelago.DisplayItem(message, local);
    }
}
