using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Astalon.Randomizer.Archipelago;

public class ArchipelagoSlotData
{
    public readonly bool RandomizeAttackPickups;
    public readonly bool RandomizeHealthPickups;
    public readonly bool RandomizeWhiteKeys;
    public readonly bool RandomizeBlueKeys;
    public readonly bool RandomizeRedKeys;
    public readonly bool RandomizeFamiliars;
    public readonly bool SkipCutscenes;
    public readonly bool StartWithZeek;
    public readonly bool StartWithBram;
    public readonly bool StartWithQoL;
    public readonly bool FreeApexElevator;
    public readonly int CostMultiplier;
    public readonly bool FastBloodChalice;
    public readonly bool CampfireWarp;
    public readonly bool DeathLink;

    public ArchipelagoSlotData(IReadOnlyDictionary<string, object> slotData)
    {
        var settings = (JObject)slotData["settings"];

        RandomizeAttackPickups = ParseBool(settings, "randomize_attack_pickups", true);
        RandomizeHealthPickups = ParseBool(settings, "randomize_health_pickups", true);
        RandomizeWhiteKeys = ParseBool(settings, "randomize_white_keys");
        RandomizeBlueKeys = ParseBool(settings, "randomize_blue_keys");
        RandomizeRedKeys = ParseBool(settings, "randomize_red_keys");
        RandomizeFamiliars = ParseBool(settings, "randomize_familiars");
        SkipCutscenes = ParseBool(settings, "skip_cutscenes", true);
        StartWithZeek = ParseBool(settings, "start_with_zeek");
        StartWithBram = ParseBool(settings, "start_with_bram");
        StartWithQoL = ParseBool(settings, "start_with_qol", true);
        FreeApexElevator = ParseBool(settings, "free_apex_elevator", true);
        CostMultiplier = ParseInt(settings, "cost_multiplier", 100);
        FastBloodChalice = ParseBool(settings, "fast_blood_chalice", true);
        CampfireWarp = ParseBool(settings, "campfire_warp", true);
        DeathLink = ParseBool(settings, "death_link");
    }

    public static bool ParseBool(JObject settings, string key, bool defaultValue = false)
    {
        if (settings.TryGetValue(key, out var value))
        {
            try
            {
                return int.Parse(value.ToString()) == 1;
            }
            catch
            {
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
            catch
            {
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
    public int ItemIndex;
    public List<string> PendingLocations = new();

    /// <summary>
    ///     seed for this archipelago data. Can be used when loading a file to verify the session the player is trying to
    ///     load is valid to the room it's connecting to.
    /// </summary>
    private string _seed;

    private const int BaseObjectId = 333000;
    private const int BaseRoomId = -1;

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
    public void SetupSession(Dictionary<string, object> roomSlotData, string roomSeed)
    {
        SlotData = new(roomSlotData);
        _seed = roomSeed;
    }

    public bool ValidateSave()
    {
        if (!string.IsNullOrWhiteSpace(_seed))
        {
            var seed = SaveManager.CurrentSave.GetObjectData(BaseObjectId);
            Plugin.Logger.LogDebug($"seed={seed}");
            if (!string.IsNullOrWhiteSpace(seed) && seed != _seed)
            {
                Plugin.Logger.LogError($"Expected seed {_seed} but found {seed}. Did you load the right save?");
                return false;
            }
        }

        var index = SaveManager.CurrentSave.GetObjectData(BaseObjectId + 1);
        Plugin.Logger.LogDebug($"index={index}");
        ItemIndex = string.IsNullOrWhiteSpace(index) ? 0 : int.Parse(index);

        var pendingLocations = SaveManager.CurrentSave.GetObjectData(BaseObjectId + 2);
        Plugin.Logger.LogDebug($"pendingLocations={pendingLocations}");
        if (!string.IsNullOrWhiteSpace(pendingLocations))
        {
            PendingLocations = JsonConvert.DeserializeObject<List<string>>(pendingLocations);
        }

        return true;
    }

    public void UpdateSave()
    {
        SaveManager.SaveObject(BaseObjectId, _seed ?? "", BaseRoomId);
        SaveManager.SaveObject(BaseObjectId + 1, ItemIndex.ToString(), BaseRoomId);
        SaveManager.SaveObject(BaseObjectId + 2, JsonConvert.SerializeObject(PendingLocations), BaseRoomId);
    }

    public void Clear()
    {
        _seed = "";
        SlotData = null;
        ItemIndex = 0;
        PendingLocations.Clear();
    }
}