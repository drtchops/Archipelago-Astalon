using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Astalon.Randomizer.Archipelago;

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public struct ShopItem
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string PlayerName { get; set; }
    public string Game { get; set; }
    public ItemFlags Flags { get; set; }
    public bool IsLocal { get; set; }
}

public class ArchipelagoSlotData
{
    public bool RandomizeAttackPickups { get; set; }
    public bool RandomizeHealthPickups { get; set; }
    public bool RandomizeWhiteKeys { get; set; }
    public bool RandomizeBlueKeys { get; set; }
    public bool RandomizeRedKeys { get; set; }
    public bool RandomizeShop { get; set; }
    public bool RandomizeFamiliars { get; set; }
    public bool SkipCutscenes { get; set; }
    public bool FreeApexElevator { get; set; }
    public int CostMultiplier { get; set; }
    public bool FastBloodChalice { get; set; }
    public bool CampfireWarp { get; set; }
    public bool DeathLink { get; set; }
    public Dictionary<string, ShopItem> ShopItems { get; }

    public ArchipelagoSlotData()
    {
        ShopItems = new();
    }

    public ArchipelagoSlotData(IReadOnlyDictionary<string, object> slotData)
    {
        var settings = (JObject)slotData["settings"];

        RandomizeAttackPickups = ParseBool(settings, "randomize_attack_pickups", true);
        RandomizeHealthPickups = ParseBool(settings, "randomize_health_pickups", true);
        RandomizeWhiteKeys = ParseBool(settings, "randomize_white_keys");
        RandomizeBlueKeys = ParseBool(settings, "randomize_blue_keys");
        RandomizeRedKeys = ParseBool(settings, "randomize_red_keys");
        RandomizeShop = ParseBool(settings, "randomize_shop");
        RandomizeFamiliars = ParseBool(settings, "randomize_familiars");
        SkipCutscenes = ParseBool(settings, "skip_cutscenes", true);
        FreeApexElevator = ParseBool(settings, "free_apex_elevator", true);
        CostMultiplier = ParseInt(settings, "cost_multiplier", 100);
        FastBloodChalice = ParseBool(settings, "fast_blood_chalice", true);
        CampfireWarp = ParseBool(settings, "campfire_warp", true);
        DeathLink = ParseBool(settings, "death_link");

        var shopItems = (JObject)slotData["shop_items"];
        try
        {
            ShopItems = shopItems.ToObject<Dictionary<string, ShopItem>>();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Error parsing slot_data.shop_items: {e.Message}");
            ShopItems = new();
        }
    }

    public static bool ParseBool(JObject settings, string key, bool defaultValue = false)
    {
        if (settings.TryGetValue(key, out var value))
        {
            try
            {
                return int.Parse(value.ToString()) == 1;
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error parsing slot_data.{key}: {e.Message}");
                return defaultValue;
            }
        }

        return defaultValue;
    }

    public static int ParseInt(JObject settings, string key, int defaultValue = 0)
    {
        if (settings.TryGetValue(key, out var value))
        {
            try
            {
                return int.Parse(value.ToString());
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error parsing slot_data.{key}: {e.Message}");
                return defaultValue;
            }
        }

        return defaultValue;
    }
}

public class ArchipelagoData
{
    public string Uri;
    public string SlotName;
    public string Password;

    /// <summary>
    ///     seed for this archipelago data. Can be used when loading a file to verify the session the player is trying to
    ///     load is valid to the room it's connecting to.
    /// </summary>
    public string Seed;

    public ArchipelagoSlotData SlotData;

    public ArchipelagoData()
    {
        Uri = "localhost";
        SlotName = "Player1";
    }

    public ArchipelagoData(string uri, string slotName, string password)
    {
        Uri = uri;
        SlotName = slotName;
        Password = password;
    }

    /// <summary>
    ///     assigns the slot data and seed to our data handler. any necessary setup using this data can be done here.
    /// </summary>
    /// <param name="roomSlotData">slot data of your slot from the room</param>
    /// <param name="roomSeed">seed name of this session</param>
    public bool SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
    {
        SlotData = new(roomSlotData);
        Seed = roomSeed;
        return Game.ConnectSave();
    }

    public void Clear()
    {
        Seed = "";
        SlotData = null;
    }
}