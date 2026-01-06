using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json.Linq;

namespace Archipelago.Astalon;

public class ApItemInfo
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

public enum Goal
{
    Vanilla = 0,
    EyeHunt = 1,
}

public enum StartingLocation
{
    GorgonTomb = 0,
    Mechanism = 1,
    HallOfThePhantoms = 2,
    RuinsOfAsh = 3,
    Apex = 4,
    Catacombs = 5,
    TowerRoots = 6,
}

public enum ApexElevator
{
    Vanilla = 0,
    Included = 1,
    Removed = 2,
}

public enum FastBloodChalice
{
    Off = 0,
    Campfires = 1,
    NotBosses = 2,
    Always = 3,
}

public class SlotData
{
    public string Version { get; set; }

    public Goal Goal { get; set; } = Goal.Vanilla;
    public int AdditionalEyesRequired { get; set; }
    public StartingLocation StartingLocation { get; set; } = StartingLocation.GorgonTomb;
    public bool RandomizeCharacters { get; set; }
    public bool RandomizeKeyItems { get; set; } = true;
    public bool RandomizeAttackPickups { get; set; } = true;
    public bool RandomizeHealthPickups { get; set; } = true;
    public bool RandomizeWhiteKeys { get; set; }
    public bool RandomizeBlueKeys { get; set; }
    public bool RandomizeRedKeys { get; set; }
    public bool RandomizeShop { get; set; }
    public bool RandomizeElevator { get; set; }
    public bool RandomizeSwitches { get; set; }
    public bool RandomizeCandles { get; set; }
    public bool RandomizeOrbMultipliers { get; set; }
    public bool ShuffleVoidPortals { get; set; }
    public bool SkipCutscenes { get; set; } = true;
    public ApexElevator ApexElevator { get; set; } = ApexElevator.Vanilla;
    public int CostMultiplier { get; set; } = 100;
    public FastBloodChalice FastBloodChalice { get; set; } = FastBloodChalice.Campfires;
    public bool CampfireWarp { get; set; } = true;
    public bool AllowBlockWarping { get; set; }
    public bool CheapKyuliRay { get; set; }
    public bool AlwaysRestoreCandles { get; set; }
    public bool ScaleCharacterStats { get; set; } = true;
    public bool TagLink { get; set; }
    public bool DeathLink { get; set; }
    public string[] StartingCharacters { get; set; } = [];
    public Dictionary<string, float> CharacterStrengths { get; set; } = [];
    public string[][] PortalPairs { get; set; } = [];

    public SlotData() { }

    public SlotData(IReadOnlyDictionary<string, object> slotData)
    {
        var options = (JObject)slotData["options"];

        Version = options.GetValue("version")?.ToString();
        // Campaign = ParseEnum<Campaign>(options, "campaign");
        Goal = ParseEnum<Goal>(options, "goal");
        AdditionalEyesRequired = ParseInt(options, "additional_eyes_required");
        StartingLocation = ParseEnum<StartingLocation>(options, "starting_location");
        RandomizeCharacters = ParseInt(options, "randomize_characters") != 0;
        RandomizeKeyItems = ParseBool(options, "randomize_key_items", true);
        RandomizeAttackPickups = ParseBool(options, "randomize_attack_pickups", true);
        RandomizeHealthPickups = ParseBool(options, "randomize_health_pickups", true);
        RandomizeWhiteKeys = ParseBool(options, "randomize_white_keys");
        RandomizeBlueKeys = ParseBool(options, "randomize_blue_keys");
        RandomizeRedKeys = ParseBool(options, "randomize_red_keys");
        RandomizeShop = ParseBool(options, "randomize_shop");
        RandomizeElevator = ParseBool(options, "randomize_elevator");
        RandomizeSwitches = ParseBool(options, "randomize_switches");
        RandomizeCandles = ParseBool(options, "randomize_candles");
        RandomizeOrbMultipliers = ParseBool(options, "randomize_orb_multipliers");
        ShuffleVoidPortals = ParseInt(options, "shuffle_void_portals") != 0;
        SkipCutscenes = ParseBool(options, "skip_cutscenes", true);
        ApexElevator = ParseEnum<ApexElevator>(options, "apex_elevator");
        CostMultiplier = ParseInt(options, "cost_multiplier", 100);
        FastBloodChalice = ParseEnum<FastBloodChalice>(options, "fast_blood_chalice");
        CampfireWarp = ParseBool(options, "campfire_warp", true);
        AllowBlockWarping = ParseBool(options, "allow_block_warping");
        CheapKyuliRay = ParseBool(options, "cheap_kyuli_ray");
        AlwaysRestoreCandles = ParseBool(options, "always_restore_candles");
        ScaleCharacterStats = ParseBool(options, "scale_character_stats", true);
        TagLink = ParseBool(options, "tag_link");
        DeathLink = ParseBool(options, "death_link");

        try
        {
            var startingCharacters = (JArray)slotData["starting_characters"];
            StartingCharacters = startingCharacters.ToObject<string[]>();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Error parsing slot_data.starting_characters: {e.Message}");
            StartingCharacters = [];
        }

        try
        {
            var characterStrengths = (JObject)slotData["character_strengths"];
            CharacterStrengths = characterStrengths.ToObject<Dictionary<string, float>>();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Error parsing slot_data.character_strengths: {e.Message}");
            CharacterStrengths = [];
        }

        try
        {
            var portalPairs = (JArray)slotData["portal_pairs"];
            PortalPairs = portalPairs.ToObject<string[][]>();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Error parsing slot_data.portal_pairs: {e.Message}");
            PortalPairs = [];
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
                Plugin.Logger.LogError($"Error parsing slot_data.settings.{key}: {e.Message}");
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
                Plugin.Logger.LogError($"Error parsing slot_data.settings.{key}: {e.Message}");
                return defaultValue;
            }
        }

        return defaultValue;
    }

    public static T ParseEnum<T>(JObject settings, string key, T defaultValue = default)
        where T : Enum
    {
        if (settings.TryGetValue(key, out var value))
        {
            try
            {
                return (T)Enum.Parse(typeof(T), value.ToString());
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Error parsing slot_data.settings.{key}: {e.Message}");
                return defaultValue;
            }
        }

        return defaultValue;
    }
}

public class State
{
    public bool Valid;

    public string Uri;
    public string SlotName;
    public string Password;
    public string Seed;
    public SlotData SlotData;
    public Dictionary<long, ApItemInfo> LocationInfos = [];

    public int ItemIndex { get; set; }
    public bool ReceivedCyclopsKey { get; set; }
    public bool ReceivedCrown { get; set; }
    public bool CheckedCyclopsIdol { get; set; }
    public bool CheckedZeek { get; set; }
    public bool CheckedBram { get; set; }
    public int CollectedGoldEyes { get; set; }
    public List<long> CheckedLocations { get; set; } = [];
    public List<DealProperties.DealID> ReceivedDeals { get; set; } = [];
    public List<int> ReceivedElevators { get; set; } = [];
    public List<int> CheckedElevators { get; set; } = [];
    public List<int> VisitedCampfires { get; set; } = [];

    public State()
    {
        Uri = "localhost";
        SlotName = "Player1";
    }

    public State(string uri, string slotName, string password)
    {
        Uri = uri;
        SlotName = slotName;
        Password = password;
    }

    public void UpdateConnection(string uri, string slotName, string password)
    {
        Uri = uri;
        SlotName = slotName;
        Password = password;
    }

    public bool SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
    {
        SlotData = new(roomSlotData);
        Seed = roomSeed;
        return Game.ConnectSave();
    }

    public void ClearConnection()
    {
        Valid = false;

        Seed = "";
        SlotData = null;
        LocationInfos.Clear();
    }

    public void ClearSave()
    {
        Valid = false;

        ItemIndex = 0;
        ReceivedCyclopsKey = false;
        ReceivedCrown = false;
        CheckedCyclopsIdol = false;
        CheckedZeek = false;
        CheckedBram = false;
        CollectedGoldEyes = 0;
        CheckedLocations.Clear();
        ReceivedDeals.Clear();
        ReceivedElevators.Clear();
        CheckedElevators.Clear();
        VisitedCampfires.Clear();
    }

    public bool ShouldSkipCutscenes()
    {
        return Valid && SlotData.SkipCutscenes;
    }

    public bool CampfireWarpsEnabled()
    {
        return Valid && SlotData.CampfireWarp;
    }

    public bool IsEyeHunt()
    {
        return Valid && SlotData.Goal == Goal.EyeHunt;
    }

    public int GoldEyeRequirement()
    {
        return Valid ? SlotData.AdditionalEyesRequired : 0;
    }

    public int GoldEyesCollected()
    {
        return Valid ? CollectedGoldEyes : 0;
    }
}
