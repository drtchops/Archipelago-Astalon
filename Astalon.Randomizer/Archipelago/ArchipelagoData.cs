using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Astalon.Randomizer.Archipelago;

public class ArchipelagoSlotData
{
    public readonly bool RandomizeAttackPickups = true;
    public readonly bool RandomizeHealthPickups = true;
    public readonly bool RandomizeWhiteKeys;
    public readonly bool RandomizeBlueKeys;
    public readonly bool RandomizeRedKeys;
    public readonly bool RandomizeFamiliars;
    public readonly bool SkipCutscenes = true;
    public readonly bool StartWithZeek;
    public readonly bool StartWithBram;
    public readonly bool StartWithQoL = true;
    public readonly bool FreeApexElevator = true;
    public readonly int CostMultiplier = 100;
    public readonly bool DeathLink;

    public ArchipelagoSlotData(IReadOnlyDictionary<string, object> slotData)
    {
        var settings = (JObject)slotData["settings"];
        if (settings.TryGetValue("randomize_attack_pickups", out var randomizeAttackPickups))
        {
            RandomizeAttackPickups = int.Parse(randomizeAttackPickups.ToString()) == 1;
        }

        if (settings.TryGetValue("randomize_health_pickups", out var randomizeHealthPickups))
        {
            RandomizeHealthPickups = int.Parse(randomizeHealthPickups.ToString()) == 1;
        }

        if (settings.TryGetValue("randomize_white_keys", out var randomizeWhiteKeys))
        {
            RandomizeWhiteKeys = int.Parse(randomizeWhiteKeys.ToString()) == 1;
        }

        if (settings.TryGetValue("randomize_blue_keys", out var randomizeBlueKeys))
        {
            RandomizeBlueKeys = int.Parse(randomizeBlueKeys.ToString()) == 1;
        }

        if (settings.TryGetValue("randomize_red_keys", out var randomizeRedKeys))
        {
            RandomizeRedKeys = int.Parse(randomizeRedKeys.ToString()) == 1;
        }

        if (settings.TryGetValue("randomize_familiars", out var randomizeFamiliars))
        {
            RandomizeFamiliars = int.Parse(randomizeFamiliars.ToString()) == 1;
        }

        if (settings.TryGetValue("skip_cutscenes", out var skipCutscenes))
        {
            SkipCutscenes = int.Parse(skipCutscenes.ToString()) == 1;
        }

        if (settings.TryGetValue("start_with_zeek", out var startWithZeek))
        {
            StartWithZeek = int.Parse(startWithZeek.ToString()) == 1;
        }

        if (settings.TryGetValue("start_with_bram", out var startWithBram))
        {
            StartWithBram = int.Parse(startWithBram.ToString()) == 1;
        }

        if (settings.TryGetValue("start_with_qol", out var startWithQoL))
        {
            StartWithQoL = int.Parse(startWithQoL.ToString()) == 1;
        }

        if (settings.TryGetValue("free_apex_elevator", out var freeApexElevator))
        {
            FreeApexElevator = int.Parse(freeApexElevator.ToString()) == 1;
        }

        if (settings.TryGetValue("cost_multiplier", out var costMultiplier))
        {
            CostMultiplier = int.Parse(costMultiplier.ToString());
        }

        if (settings.TryGetValue("death_link", out var deathLink))
        {
            DeathLink = int.Parse(deathLink.ToString()) == 1;
        }
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