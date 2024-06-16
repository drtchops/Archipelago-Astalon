using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Newtonsoft.Json.Linq;

namespace Astalon.Randomizer.Archipelago;

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

public enum Campaign
{
    TearsOfTheEarth = 0,
    NewGamePlus = 1,
    BlackKnight = 2,
    MonsterMode = 3,
}

public enum Goal
{
    Vanilla = 0,
    EyeHunt = 1,
}

public enum ApexElevator
{
    Vanilla = 0,
    Included = 1,
    Removed = 2,
}

public class SlotData
{
    public Campaign Campaign { get; set; } = Campaign.TearsOfTheEarth;
    public Goal Goal { get; set; } = Goal.Vanilla;
    public int AdditionalEyesRequired { get; set; } = 0;
    public bool RandomizeCharacters { get; set; } = false;
    public bool RandomizeKeyItems { get; set; } = true;
    public bool RandomizeAttackPickups { get; set; } = true;
    public bool RandomizeHealthPickups { get; set; } = true;
    public bool RandomizeWhiteKeys { get; set; } = false;
    public bool RandomizeBlueKeys { get; set; } = false;
    public bool RandomizeRedKeys { get; set; } = false;
    public bool RandomizeShop { get; set; } = false;
    public bool RandomizeElevator { get; set; } = false;
    public bool RandomizeSwitches { get; set; } = false;
    public bool RandomizeFamiliars { get; set; } = false;
    public bool RandomizeOrbCrates { get; set; } = false;
    public bool RandomizeBossOrbRewards { get; set; } = false;
    public bool RandomizeMinibossOrbRewards { get; set; } = false;
    public bool SkipCutscenes { get; set; } = true;
    public ApexElevator ApexElevator { get; set; } = ApexElevator.Vanilla;
    public int CostMultiplier { get; set; } = 100;
    public bool FastBloodChalice { get; set; } = true;
    public bool CampfireWarp { get; set; } = true;
    public bool AllowBlockWarping { get; set; } = false;
    public bool CheapKyuliRay { get; set; } = false;
    public bool DeathLink { get; set; } = false;
    public string[] StartingCharacters { get; set; } = [];

    public SlotData() { }

    public SlotData(IReadOnlyDictionary<string, object> slotData)
    {
        var settings = (JObject)slotData["settings"];

        Campaign = ParseEnum<Campaign>(settings, "campaign");
        Goal = ParseEnum<Goal>(settings, "goal");
        AdditionalEyesRequired = ParseInt(settings, "additional_eyes_required");
        RandomizeCharacters = ParseInt(settings, "randomize_characters") != 0;
        RandomizeKeyItems = ParseBool(settings, "randomize_key_items", true);
        RandomizeAttackPickups = ParseBool(settings, "randomize_attack_pickups", true);
        RandomizeHealthPickups = ParseBool(settings, "randomize_health_pickups", true);
        RandomizeWhiteKeys = ParseBool(settings, "randomize_white_keys");
        RandomizeBlueKeys = ParseBool(settings, "randomize_blue_keys");
        RandomizeRedKeys = ParseBool(settings, "randomize_red_keys");
        RandomizeShop = ParseBool(settings, "randomize_shop");
        RandomizeElevator = ParseBool(settings, "randomize_elevator");
        RandomizeSwitches = ParseBool(settings, "randomize_switches");
        RandomizeFamiliars = ParseBool(settings, "randomize_familiars");
        RandomizeOrbCrates = ParseBool(settings, "randomize_orb_crates");
        RandomizeBossOrbRewards = ParseBool(settings, "randomize_boss_orb_rewards");
        RandomizeMinibossOrbRewards = ParseBool(settings, "randomize_miniboss_orb_rewards");
        SkipCutscenes = ParseBool(settings, "skip_cutscenes", true);
        ApexElevator = ParseEnum<ApexElevator>(settings, "apex_elevator");
        CostMultiplier = ParseInt(settings, "cost_multiplier", 100);
        FastBloodChalice = ParseBool(settings, "fast_blood_chalice", true);
        CampfireWarp = ParseBool(settings, "campfire_warp", true);
        AllowBlockWarping = ParseBool(settings, "allow_block_warping", false);
        CheapKyuliRay = ParseBool(settings, "cheap_kyuli_ray");
        DeathLink = ParseBool(settings, "death_link");

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

    public static T ParseEnum<T>(JObject settings, string key, T defaultValue = default) where T : Enum
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

    public bool ShouldSkipCutscenes() => Valid && SlotData.SkipCutscenes;

    public bool CampfireWarpsEnabled() => Valid && SlotData.CampfireWarp;

    public bool IsEyeHunt() => Valid && SlotData.Goal == Goal.EyeHunt;

    public int GoldEyeRequirement() => Valid ? SlotData.AdditionalEyesRequired : 0;

    public int GoldEyesCollected() => Valid ? CollectedGoldEyes : 0;
}
