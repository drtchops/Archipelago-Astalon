using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Archipelago.MultiClient.Net.Enums;
using Astalon.Randomizer.Archipelago;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Astalon.Randomizer;

public class ItemBox
{
    public string Message { get; set; }
    public string Icon { get; set; }
    public string Sound { get; set; }
    public float Duration { get; set; } = 2.5f;
    public bool DisableController { get; set; } = false;
}

public struct SaveData
{
    public string Seed { get; set; }
    public int ItemIndex { get; set; }
    public ArchipelagoSlotData SlotData { get; set; }
    public List<string> PendingLocations { get; set; }
    public List<DealProperties.DealID> ReceivedDeals { get; set; }
    public List<int> ReceivedElevators { get; set; }
}

[JsonObject(NamingStrategyType = typeof(SnakeCaseNamingStrategy))]
public struct RoomData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public int Area { get; set; }
    public int Floor { get; set; }
    public bool IsRocks { get; set; }
    public bool PreventBellUse { get; set; }
    public bool SavePoint { get; set; }
    public bool TitanStatue { get; set; }
    public bool VoidPortal { get; set; }
    public string InitialPosition { get; set; }
    public int[] Switches { get; set; }
    public int[] Switchables { get; set; }
}

public struct SwitchDump
{
    public string Id { get; set; }
    public int RoomId { get; set; }
    public int[] ObjectsToEnable { get; set; }
    public int[] ObjectsToDisable { get; set; }
    public string ItemName { get; set; }
    public string LocationName { get; set; }
}

public static class Game
{
    public const string Name = "Astalon";
    public const int SaveObjectId = 333000;
    public const int SaveRoomId = -1;

    public static Queue<ItemInfo> IncomingItems { get; } = new();
    public static Queue<ItemInfo> IncomingMessages { get; } = new();
    public static string DeathSource { get; private set; }
    public static bool ReceivingItem { get; set; }
    public static bool IsInShop { get; set; }

    public static bool UnlockElevators { get; set; }
    public static bool TriggerDeath { get; set; }
    public static bool DumpRoom { get; set; }
    public static bool ToggleSwitches { get; set; }
    public static bool ToggleObjects { get; set; }
    public static bool ResetDoors { get; set; }
    public static string WarpDestination { get; set; }
    public static string MoveDirection { get; set; }
    public static int RoomWarp { get; set; }

    private static readonly string DataDir = Path.GetFullPath("BepInEx/data/Archipelago");
    private static bool _saveNew;
    private static bool _saveLoaded;
    private static bool _saveValid;
    private static bool _saveDataFilled;
    private static bool _saveInitialized;
    private static SaveData _saveData;
    private static int _deathCounter = -1;
    private static tk2dBaseSprite _baseSprite;
    private static tk2dSpriteCollectionData _spriteCollectionData;
    private static tk2dSpriteAnimationClip _spriteAnimationClip;
    private static bool _injectedAnimation;
    private static int _warpCooldown;

    #region AnimationExperiments

    public static void Awake()
    {
        var apTexture = LoadImageAsTexture("ap-item.png");
        var gameObject = tk2dSprite.CreateFromTexture(
            apTexture,
            tk2dSpriteCollectionSize.ForTk2dCamera(),
            new(0, 0, 16, 16),
            new(8, 8));
        _baseSprite = gameObject.GetComponent<tk2dSprite>();

        _spriteCollectionData = tk2dSpriteCollectionData.CreateFromTexture(
            LoadImageAsTexture("multi-images.png"),
            tk2dSpriteCollectionSize.ForTk2dCamera(),
            new(new string[] { "AP_ITEM", "AP_ITEM_BRIGHT" }),
            new(new Rect[] { new(0, 0, 16, 16), new(16, 0, 16, 16) }),
            new(new Vector2[] { new(8, 8), new(8, 8) }));

        _spriteAnimationClip = new()
        {
            fps = 6,
            loopStart = 0,
            name = "AP_ITEM",
            wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop,
            frames = new(new tk2dSpriteAnimationFrame[]
            {
                new()
                {
                    eventFloat = 0,
                    eventInfo = null,
                    eventInt = 0,
                    spriteCollection = _spriteCollectionData,
                    spriteId = 0,
                    triggerEvent = false,
                },
                new()
                {
                    eventFloat = 0,
                    eventInfo = null,
                    eventInt = 0,
                    spriteCollection = _spriteCollectionData,
                    spriteId = 1,
                    triggerEvent = false,
                },
            }),
        };
    }

    public static Texture2D LoadImageAsTexture(string filename)
    {
        var path = $"{DataDir}/{filename}";
        var bytes = File.ReadAllBytes(path);
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(bytes, false);
        return texture;
    }

    public static void UpdateSprite(tk2dBaseSprite sprite)
    {
        sprite.SetSprite(_baseSprite.Collection, 0);
    }

    public static void UpdateAnimation(tk2dSpriteAnimator animator)
    {
        if (!_injectedAnimation)
        {
            var newClips = animator.library.clips.ToList();
            newClips.Add(_spriteAnimationClip);
            animator.library.clips = newClips.ToArray();
            _injectedAnimation = true;
        }

        var clipId = animator.GetClipIdByName("AP_ITEM");
        animator.defaultClipId = clipId;
        animator.Play("AP_ITEM");
    }

    #endregion

    #region Visuals

    public static void UpdateItem(Item item)
    {
        if (!_saveDataFilled)
        {
            return;
        }

        if (item.itemProperties?.itemID == null)
        {
            return;
        }

        if (Data.LocationMap.TryGetValue(item.itemProperties.itemID, out var location))
        {
            UpdateEntityAppearance(item.gameObject, location);
        }
    }

    public static void UpdateEntity(GameObject gameObject, int actorId)
    {
        if (!_saveDataFilled)
        {
            return;
        }

        string location = null;

        if (_saveData.SlotData.RandomizeHealthPickups &&
            Data.HealthMap.TryGetValue(actorId, out var healthLocation))
        {
            location = healthLocation;
        }

        if (_saveData.SlotData.RandomizeAttackPickups &&
            Data.AttackMap.TryGetValue(actorId, out var attackLocation))
        {
            location = attackLocation;
        }

        if (_saveData.SlotData.RandomizeWhiteKeys &&
            Data.WhiteKeyMap.TryGetValue(actorId, out var whiteLocation))
        {
            location = whiteLocation;
        }

        if (_saveData.SlotData.RandomizeBlueKeys &&
            Data.BlueKeyMap.TryGetValue(actorId, out var blueLocation))
        {
            location = blueLocation;
        }

        if (actorId == 0 &&
            Data.SpawnedKeyMap.TryGetValue(Player.PlayerDataLocal.currentRoomID, out var spawnedLocation))
        {
            if ((spawnedLocation.Contains("White Key") && _saveData.SlotData.RandomizeWhiteKeys) ||
                (spawnedLocation.Contains("Blue Key") && _saveData.SlotData.RandomizeBlueKeys) ||
                (spawnedLocation.Contains("Red Key") && _saveData.SlotData.RandomizeRedKeys))
            {
                location = spawnedLocation;
            }
        }

        if (_saveData.SlotData.RandomizeRedKeys &&
            Data.RedKeyMap.TryGetValue(actorId, out var redLocation))
        {
            location = redLocation;
        }

        if (location != null)
        {
            UpdateEntityAppearance(gameObject, location);
        }
    }

    public static void UpdateEntityAppearance(GameObject gameObject, string location)
    {
        var itemInfo = Plugin.ArchipelagoClient.ScoutLocation(location);
        var itemName = itemInfo != null && itemInfo.IsAstalon ? itemInfo.Name : "";
        var itemFlags = itemInfo?.Flags ?? ItemFlags.None;
        var icon = GetIcon(itemName, itemFlags);
        var animationName = GetClip(icon);

        var animator = gameObject.GetComponent<tk2dSpriteAnimator>();
        if (animator != null)
        {
            if (animationName != null)
            {
                var clipId = animator.GetClipIdByName(animationName);
                animator.defaultClipId = clipId;
                animator.Play(animationName);
            }
            else
            {
                animator.StopAndResetFrame();
                animator.playAutomatically = false;
                animator.defaultClipId = -1;
            }
        }

        var sprite = gameObject.GetComponent<tk2dBaseSprite>();
        if (sprite != null)
        {
            sprite.SetSprite(icon);
        }
    }

    #endregion

    #region SaveData

    public static void LoadSave()
    {
        if (_saveLoaded)
        {
            return;
        }

        _saveNew = false;
        _saveLoaded = true;
        _saveValid = false;
        _saveDataFilled = false;

        //List<SwitchDump> switchDumps = [];

        //foreach (var room in GameManager.Instance.gameRooms)
        //{
        //    Dictionary<string, List<int>> objectsToEnable = [];
        //    Dictionary<string, List<int>> objectsToDisable = [];
        //    List<string> switches = [];

        //    foreach (var entityData in room.roomEntitiesData)
        //    {
        //        var switchId = entityData.GetValue("switchID");
        //        if (!string.IsNullOrWhiteSpace(switchId) && switchId != "-1")
        //        {
        //            switches.Add(switchId);
        //            if (!objectsToEnable.ContainsKey(switchId))
        //            {
        //                objectsToEnable[switchId] = [];
        //                objectsToDisable[switchId] = [];
        //            }
        //        }

        //        var linkId = entityData.GetValue("linkID");
        //        if (!string.IsNullOrWhiteSpace(linkId))
        //        {
        //            if (!objectsToEnable.ContainsKey(linkId))
        //            {
        //                objectsToEnable[linkId] = [];
        //                objectsToDisable[linkId] = [];
        //            }

        //            var isOn = entityData.GetValue("objectOn");
        //            if (isOn == "True")
        //            {
        //                objectsToDisable[linkId].Add(entityData.ID);
        //            }
        //            else if (isOn == "False")
        //            {
        //                objectsToEnable[linkId].Add(entityData.ID);
        //            }
        //        }
        //    }

        //    var itemName = "Switch";
        //    var locationName = "Switch";
        //    switch (room.GetRoomArea())
        //    {
        //        case 1:
        //            itemName = "GT Switch";
        //            locationName = "Gorgon Tomb - Switch";
        //            break;
        //        case 2:
        //            itemName = "Mech Switch";
        //            locationName = "Mechanism - Switch";
        //            break;
        //        case 11:
        //            itemName = "CD Switch";
        //            locationName = "Cyclops Den - Switch";
        //            break;
        //        case 3:
        //            itemName = "HotP Switch";
        //            locationName = "Hall of the Phantoms - Switch";
        //            break;
        //        case 7:
        //            itemName = "Cath Switch";
        //            locationName = "Cathedral - Switch";
        //            break;
        //        case 5:
        //            itemName = "RoA Switch";
        //            locationName = "Ruins of Ash - Switch";
        //            break;
        //        case 8:
        //            itemName = "SP Switch";
        //            locationName = "Serpent Path - Switch";
        //            break;
        //        case 21:
        //            itemName = "Caves Switch";
        //            locationName = "Caves - Switch";
        //            break;
        //        case 4:
        //            itemName = "Cata Switch";
        //            locationName = "Catacombs - Switch";
        //            break;
        //        case 19:
        //            itemName = "TR Switch";
        //            locationName = "Tower Roots - Switch";
        //            break;
        //    }

        //    foreach (var switchId in switches)
        //    {
        //        switchDumps.Add(new()
        //        {
        //            Id = switchId,
        //            RoomId = room.roomID,
        //            ObjectsToEnable = objectsToEnable[switchId].ToArray(),
        //            ObjectsToDisable = objectsToDisable[switchId].ToArray(),
        //            ItemName = itemName,
        //            LocationName = locationName,
        //        });
        //    }
        //}

        //Plugin.Logger.LogMessage(JsonConvert.SerializeObject(switchDumps.OrderBy((s) => int.Parse(s.Id)).ToArray()));

        //List<RoomData> rooms = [];
        //foreach (var room in GameManager.Instance.gameRooms)
        //{
        //    List<int> switches = [];
        //    List<int> switchables = [];

        //    foreach (var entityData in room.roomEntitiesData)
        //    {
        //        if (entityData.GetValue("wasActivated") != "")
        //        {
        //            switches.Add(entityData.ID);
        //        }

        //        if (entityData.GetValue("objectOn") != "")
        //        {
        //            switchables.Add(entityData.ID);
        //        }
        //    }

        //    rooms.Add(new()
        //    {
        //        Id = room.roomID,
        //        Name = room.name,
        //        Type = room.roomType,
        //        Area = room.GetRoomArea(),
        //        Floor = room.GetCurrentRoomFloor(),
        //        IsRocks = room.isRocks,
        //        PreventBellUse = room.preventBellUse,
        //        SavePoint = room.savePoint,
        //        TitanStatue = room.titanStatue,
        //        VoidPortal = room.voidPortal,
        //        InitialPosition = room.roomInitialPosition.ToString(),
        //        Switches = switches.ToArray(),
        //        Switchables = switchables.ToArray(),
        //    });
        //}

        //Plugin.Logger.LogMessage(JsonConvert.SerializeObject(rooms));

        var serializedData = SaveManager.CurrentSave.GetObjectData(SaveObjectId);
        if (string.IsNullOrWhiteSpace(serializedData))
        {
            Plugin.Logger.LogError("Did not find AP save data. Did you load a casual save?");
            Plugin.ArchipelagoClient.Disconnect();
            return;
        }

        try
        {
            _saveData = JsonConvert.DeserializeObject<SaveData>(serializedData);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Invalid AP save data: {e.Message}");
            Plugin.Logger.LogError(e);
            Plugin.ArchipelagoClient.Disconnect();
            return;
        }

        _saveValid = true;
        _saveDataFilled = true;

        InitializeSave();
        ConnectSave();
    }

    public static void SetupNewSave()
    {
        _saveNew = true;
        _saveLoaded = true;
        _saveValid = true;
        _saveDataFilled = false;
        _saveData = new()
        {
            PendingLocations = [],
            ReceivedDeals = [],
            ReceivedElevators = [],
        };
    }

    public static bool ConnectSave()
    {
        if (!_saveLoaded)
        {
            return true;
        }

        if (!_saveValid)
        {
            Plugin.ArchipelagoClient.Disconnect();
            return false;
        }

        if (!Plugin.ArchipelagoClient.Connected)
        {
            return true;
        }

        var seed = ArchipelagoClient.ServerData.Seed;
        var slotData = ArchipelagoClient.ServerData.SlotData;

        if (_saveNew)
        {
            _saveNew = false;
            _saveData.Seed = seed;
            _saveData.SlotData = slotData;
            _saveDataFilled = true;

            if (slotData.RandomizeCharacters != RandomizeCharacters.Vanilla)
            {
                if (!slotData.StartingCharacters.Contains("Algus"))
                {
                    Player.PlayerDataLocal.LockCharacter(CharacterProperties.Character.Algus);
                    Player.PlayerDataLocal.MakeDealNonAvailable(DealProperties.DealID.Deal_SubMenu_Algus);
                }

                if (!slotData.StartingCharacters.Contains("Arias"))
                {
                    Player.PlayerDataLocal.LockCharacter(CharacterProperties.Character.Arias);
                    Player.PlayerDataLocal.MakeDealNonAvailable(DealProperties.DealID.Deal_SubMenu_Arias);
                }

                if (!slotData.StartingCharacters.Contains("Kyuli"))
                {
                    Player.PlayerDataLocal.LockCharacter(CharacterProperties.Character.Kyuli);
                    Player.PlayerDataLocal.MakeDealNonAvailable(DealProperties.DealID.Deal_SubMenu_Kyuli);
                }

                if (slotData.StartingCharacters.Contains("Zeek"))
                {
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Zeek);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Zeek, false);
                }

                if (slotData.StartingCharacters.Contains("Bram"))
                {
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Bram);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Bram, false);
                }

                if (!slotData.StartingCharacters.Contains(
                        Data.CharacterToItem[Player.PlayerDataLocal.currentCharacter]))
                {
                    Player.Instance.CycleCharacterTo(Data.ItemToCharacter[slotData.StartingCharacters[0]]);
                }
            }

            UpdateSaveData();
        }
        else if (seed != _saveData.Seed)
        {
            Plugin.Logger.LogError("Mismatched seed detected. Did you load the right save?");
            Plugin.ArchipelagoClient.Disconnect();
        }
        else
        {
            _saveData.SlotData = ArchipelagoClient.ServerData.SlotData;
        }

        SyncLocations();
        InitializeSave();

        return true;
    }

    public static void UpdateSaveData()
    {
        var data = JsonConvert.SerializeObject(_saveData);
        SaveManager.SaveObject(SaveObjectId, data, SaveRoomId);
    }

    public static void InitializeSave()
    {
        if (_saveInitialized)
        {
            return;
        }

        if (_saveData.SlotData.SkipCutscenes)
        {
            Player.PlayerDataLocal.cs_bkbossfinal1 = true;
            Player.PlayerDataLocal.cs_bkbossintro1 = true;
            Player.PlayerDataLocal.cs_bkFinalToMedusa = true;
            Player.PlayerDataLocal.cs_finalPlatformRide = true;
            if (Player.PlayerDataLocal.epimetheusSequence == 0)
            {
                Player.PlayerDataLocal.epimetheusSequence = 10;
                Player.PlayerDataLocal.deaths = 1;
            }

            if (!Player.PlayerDataLocal.firstElevatorLit)
            {
                Player.PlayerDataLocal.firstElevatorLit = true;

                Player.PlayerDataLocal.elevatorsFound ??= new();
                if (!Player.PlayerDataLocal.elevatorsFound.Contains(6629))
                {
                    Player.PlayerDataLocal.elevatorsFound.Add(6629);
                }

                // blocks around first elevator
                for (var i = 6641; i < 6647; i++)
                {
                    UpdateObjectData(i, 6629, "objectOn", "False");
                }

                // tutorial rooms
                UpdateObjectData(6729, 6670, "objectOn", "True");
                UpdateObjectData(6757, 6670, "unlockStep", "3");
                UpdateObjectData(458, 6671, "wasActivated", "True");
                UpdateObjectData(459, 6672, "wasActivated", "True");
                UpdateObjectData(460, 6672, "objectOn", "False");
                UpdateObjectData(6708, 6672, "wasActivated", "True");
                UpdateObjectData(6709, 6672, "objectOn", "False");
                UpdateObjectData(6722, 6672, "wasActivated", "True");
                UpdateObjectData(454, 6673, "wasActivated", "True");
            }

            // Tauros cutscenes
            UpdateObjectData(383, 64, "triggeredOnce", "True");
            UpdateObjectData(383, 64, "talkAlgus", "True");
            UpdateObjectData(383, 64, "talkArias", "True");
            UpdateObjectData(383, 64, "talkKyuli", "True");

            // Volantis cutscenes
            UpdateObjectData(1914, 802, "triggeredOnce", "True");
            UpdateObjectData(1914, 802, "talkAlgus", "True");
            UpdateObjectData(1914, 802, "talkArias", "True");
            UpdateObjectData(1914, 802, "talkKyuli", "True");
            UpdateObjectData(1914, 802, "talkBram", "True");
            UpdateObjectData(1914, 802, "talkZeek", "True");

            // Solaria cutscenes
            UpdateObjectData(10018, 10015, "triggeredOnce", "True");
            UpdateObjectData(10018, 10015, "talkAlgus", "True");
            UpdateObjectData(10018, 10015, "talkArias", "True");
            UpdateObjectData(10018, 10015, "talkKyuli", "True");
            UpdateObjectData(10018, 10015, "talkBram", "True");
            UpdateObjectData(10018, 10015, "talkZeek", "True");

            // Gemini cutscenes
            UpdateObjectData(9114, 985, "triggeredOnce", "True");
            UpdateObjectData(9114, 985, "talkAlgus", "True");
            UpdateObjectData(9114, 985, "talkArias", "True");
            UpdateObjectData(9114, 985, "talkKyuli", "True");
            UpdateObjectData(9114, 985, "talkBram", "True");
            UpdateObjectData(9114, 985, "talkZeek", "True");
        }

        if (_saveData.SlotData.CostMultiplier != 100 && GameManager.Instance.itemManager
                .GetDealProperties(DealProperties.DealID.Deal_Gift).DealPrice == 666)
        {
            var mul = _saveData.SlotData.CostMultiplier / 100f;
            foreach (var deal in GameManager.Instance.itemManager.gameDeals)
            {
                deal.dealPrice = (int)Math.Round(deal.dealPrice * mul);
            }
        }

        if (_saveData.SlotData.FastBloodChalice)
        {
            Player.Instance.regenInterval = 0.2f;
        }

        _saveInitialized = true;
    }

    public static void ExitSave()
    {
        _saveNew = false;
        _saveLoaded = false;
        _saveValid = false;
        _saveDataFilled = false;
        _saveInitialized = false;
        _saveData = new();
    }

    #endregion

    public static bool CanGetItem()
    {
        if (!_saveDataFilled)
        {
            return false;
        }

        if (GameManager.Instance?.player?.playerData == null)
        {
            return false;
        }

        if (Player.Instance == null || !Player.Instance.playerDataLoaded)
        {
            return false;
        }

        return true;
    }

    public static bool CanDisplayMessage()
    {
        if (!CanGetItem())
        {
            return false;
        }

        if (GameplayUIManager.Instance == null)
        {
            return false;
        }

        if (GameplayUIManager.Instance.itemBox == null || GameplayUIManager.Instance.itemBox.active)
        {
            return false;
        }

        return true;
    }

    public static bool CanBeKilled()
    {
        if (!CanGetItem())
        {
            return false;
        }

        if (Player.PlayerDataLocal.currentHealth <= 0)
        {
            return false;
        }

        if (Player.Instance.isInElevator)
        {
            return false;
        }

        if (GameplayUIManager.Instance.ESOpen || GameplayUIManager.Instance.inGameMenuOpen)
        {
            return false;
        }

        return true;
    }

    public static bool CanCycleCharacter()
    {
        if (!_saveValid)
        {
            return true;
        }

        if (!_saveDataFilled)
        {
            return false;
        }

        return Player.PlayerDataLocal.unlockedCharacters.Count > 1;
    }

    public static string GetIcon(string itemName, ItemFlags flags = ItemFlags.None)
    {
        if (itemName.Contains("White Door"))
        {
            return "WhiteKey_1";
        }

        if (itemName.Contains("Blue Door"))
        {
            return "BlueKey_1";
        }

        if (itemName.StartsWith("Red Door"))
        {
            return "RedKey_1";
        }

        if (itemName.StartsWith("Max HP"))
        {
            return "Item_HealthStone_1";
        }

        if (itemName.EndsWith("Orbs"))
        {
            return "SoulOrb_Big";
        }

        if (Data.IconMap.TryGetValue(itemName, out var icon))
        {
            return icon;
        }

        return flags == ItemFlags.Advancement ? "BlueOrb_1" : "Orb_Idle_1";
    }

    public static string GetClip(string icon)
    {
        if (icon.StartsWith("Deal_") || icon.StartsWith("ElevatorMenu_"))
        {
            return null;
        }

        return icon switch
        {
            "WhiteKey_1" => "WhiteKey",
            "BlueKey_1" => "BlueKey",
            "RedKey_1" => "RedKey",
            "RedOrb_1" => "DeathOrb",
            "BlueOrb_1" => "TrapOrb",
            "SoulOrb_Big" => "Orb_Big_UI",
            "Orb_Idle_1" => "SecretsOrb_Idle",
            _ => icon,
        };
    }

    public static ItemBox FormatItemBox(ItemInfo itemInfo)
    {
        var message = itemInfo.Name;
        if (!itemInfo.IsLocal)
        {
            var playerName = itemInfo.PlayerName;
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = "Server";
            }

            message = itemInfo.Receiving ? $"{message} from {playerName}" : $"{playerName}'s {message}";
        }

        var icon = GetIcon(itemInfo.Name, itemInfo.Flags);

        var sound = itemInfo.Flags switch
        {
            //ItemFlags.Advancement => "applause",
            ItemFlags.Advancement => "secret",
            //ItemFlags.NeverExclude => "secret",
            ItemFlags.Trap => "evil-laugh",
            _ => "pickup",
        };

        return new()
        {
            Message = message,
            Icon = icon,
            Sound = sound,
        };
    }

    public static bool DisplayItem(ItemInfo itemInfo)
    {
        if (!CanDisplayMessage())
        {
            return false;
        }

        var itemBox = FormatItemBox(itemInfo);
        GameplayUIManager.Instance.DisplayItemBox(itemBox.Icon, itemBox.Message, itemBox.Duration,
            itemBox.DisableController);
        AudioManager.Play(itemBox.Sound);
        return true;
    }

    public static bool GiveItem(ItemInfo itemInfo)
    {
        if (!CanGetItem())
        {
            return false;
        }

        var itemName = itemInfo.Name;
        Plugin.Logger.LogDebug($"Giving item: {itemName}");

        if (itemName == "Attack +1")
        {
            Player.PlayerDataLocal.strengthBonusShared += 1;
        }
        else if (itemName.StartsWith("Max HP"))
        {
            var bonus = itemName switch
            {
                "Max HP +1" => 1,
                "Max HP +2" => 2,
                "Max HP +3" => 3,
                "Max HP +4" => 4,
                "Max HP +5" => 5,
                _ => 0,
            };

            Player.PlayerDataLocal.healthItemBonus += bonus;
            Player.PlayerDataLocal.currentHealth += bonus;
            GameplayUIManager.Instance?.UpdateHealthBar(Player.Instance, true);
        }
        else if (itemName.EndsWith("Orbs"))
        {
            var amount = itemName switch
            {
                "50 Orbs" => 50,
                "100 Orbs" => 100,
                "200 Orbs" => 200,
                "500 Orbs" => 500,
                "1000 Orbs" => 1000,
                "2000 Orbs" => 2000,
                _ => 0,
            };

            Player.Instance.CollectOrbs(amount);
        }
        else if (Data.ItemMap.TryGetValue(itemName, out var itemId))
        {
            ReceivingItem = true;
            Player.PlayerDataLocal.CollectItem(itemId);
            Player.PlayerDataLocal.EnableItem(itemId);
            ReceivingItem = false;

            switch (itemId)
            {
                case ItemProperties.ItemID.AscendantKey:
                    Player.PlayerDataLocal.elevatorsOpened = true;
                    foreach (var roomId in Player.PlayerDataLocal.elevatorsFound)
                    {
                        Player.PlayerDataLocal.UnlockElevator(roomId);
                    }

                    Player.PlayerDataLocal.UnlockElevator(6629);
                    if (_saveData.SlotData.FreeApexElevator)
                    {
                        Player.PlayerDataLocal.UnlockElevator(4109);
                    }

                    break;
                case ItemProperties.ItemID.ZeekItem:
                    // TODO: figure out why this item doesn't work
                    Player.PlayerDataLocal.zeekItem = true;
                    Player.PlayerDataLocal.cyclopsDenKey = true;
                    Player.PlayerDataLocal.CollectItem(ItemProperties.ItemID.CyclopsIdol);
                    Player.PlayerDataLocal.EnableItem(ItemProperties.ItemID.CyclopsIdol);
                    Player.PlayerDataLocal.AddKey(Key.KeyType.Cyclops);
                    Player.PlayerDataLocal.zeekQuestStatus = Room_Zeek.ZeekQuestStatus.QuestStarted;
                    Player.PlayerDataLocal.zeekSeen = true;
                    break;
                case ItemProperties.ItemID.AthenasBell:
                    Player.Instance.SetCanChangeCharacterTo(true);
                    break;
            }
        }
        else if (itemName.EndsWith("Key"))
        {
            switch (itemName)
            {
                case "White Key":
                    Player.PlayerDataLocal.AddKey(Key.KeyType.White);
                    break;
                case "Blue Key":
                    Player.PlayerDataLocal.AddKey(Key.KeyType.Blue);
                    break;
                case "Red Key":
                    Player.PlayerDataLocal.AddKey(Key.KeyType.Red);
                    break;
            }
        }
        else if (Data.WhiteDoorMap.TryGetValue(itemName, out var whiteIds))
        {
            return OpenDoor(whiteIds);
        }
        else if (Data.BlueDoorMap.TryGetValue(itemName, out var blueIds))
        {
            return OpenDoor(blueIds);
        }
        else if (Data.RedDoorMap.TryGetValue(itemName, out var redIds))
        {
            return OpenDoor(redIds);
        }
        else if (Data.ItemToDeal.TryGetValue(itemName, out var dealId))
        {
            if (_saveData.SlotData.RandomizeShop)
            {
                _saveData.ReceivedDeals.Add(dealId);
            }
            else
            {
                Player.PlayerDataLocal.PurchaseDeal_Immediate(dealId);
            }

            if (dealId == DealProperties.DealID.Deal_Gift)
            {
                Player.PlayerDataLocal.CollectItem(ItemProperties.ItemID.MarkOfEpimetheus);
                Player.PlayerDataLocal.EnableItem(ItemProperties.ItemID.MarkOfEpimetheus);
            }
        }
        else if (Data.ItemToLink.TryGetValue(itemName, out var switchData))
        {
            ToggleSwitchLink(switchData);
        }
        else if (Data.ItemToElevator.TryGetValue(itemName, out var elevatorId))
        {
            if (_saveData.SlotData.RandomizeElevator)
            {
                _saveData.ReceivedElevators.Add(elevatorId);
            }
            else if (!Player.PlayerDataLocal.elevatorsFound.Contains(elevatorId))
            {
                Player.PlayerDataLocal.elevatorsFound.Add(elevatorId);
            }
        }
        else
        {
            switch (itemName)
            {
                case "Algus":
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Algus);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Algus, false);
                    break;
                case "Arias":
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Arias);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Arias, false);
                    break;
                case "Kyuli":
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Kyuli);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Kyuli, false);
                    break;
                case "Zeek":
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Zeek);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Zeek, false);
                    break;
                case "Bram":
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Bram);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Bram, false);
                    break;
                default:
                    Plugin.Logger.LogWarning($"Item {itemInfo.Id} - {itemName} not found");
                    return false;
            }
        }

        return true;
    }

    public static void ElevatorUnlocked(int roomId)
    {
        if (!_saveDataFilled || !_saveData.SlotData.RandomizeElevator)
        {
            return;
        }

        if (Data.ElevatorToLocation.TryGetValue(roomId, out var location))
        {
            SendLocation(location);
        }
    }

    public static void UpdateElevatorList()
    {
        if (!_saveDataFilled)
        {
            return;
        }

        if (_saveData.SlotData.RandomizeElevator)
        {
            foreach (var elevatorId in Player.PlayerDataLocal.elevatorsFound)
            {
                if (elevatorId != 6629 && (elevatorId != 4109 || !_saveData.SlotData.FreeApexElevator) &&
                    !_saveData.ReceivedElevators.Contains(elevatorId))
                {
                    Player.PlayerDataLocal.elevatorsFound.Remove(elevatorId);
                }
            }

            foreach (var elevatorId in _saveData.ReceivedElevators)
            {
                if (!Player.PlayerDataLocal.elevatorsFound.Contains(elevatorId))
                {
                    Player.PlayerDataLocal.elevatorsFound.Add(elevatorId);
                }
            }
        }
        else if (!_saveData.SlotData.FreeApexElevator && !Player.PlayerDataLocal.discoveredRooms.Contains(4109))
        {
            Player.PlayerDataLocal.elevatorsFound.Remove(4109);
        }
    }

    public static void HandleDeath()
    {
        if (_deathCounter <= 0)
        {
            Plugin.ArchipelagoClient.SendDeath();
        }
    }

    public static void ReceiveDeath(string source)
    {
        DeathSource = source;
    }

    public static bool TryGetItemLocation(ItemProperties.ItemID itemId, out string location)
    {
        if (!_saveValid)
        {
            location = null;
            return false;
        }

        return Data.LocationMap.TryGetValue(itemId, out location);
    }

    public static bool TryGetEntityLocation(int entityId, out string location)
    {
        if (!_saveDataFilled)
        {
            location = null;
            return false;
        }

        if (_saveData.SlotData.RandomizeHealthPickups &&
            Data.HealthMap.TryGetValue(entityId, out var healthLocation))
        {
            location = healthLocation;
            return true;
        }

        if (_saveData.SlotData.RandomizeAttackPickups &&
            Data.AttackMap.TryGetValue(entityId, out var attackLocation))
        {
            location = attackLocation;
            return true;
        }

        if (_saveData.SlotData.RandomizeWhiteKeys &&
            Data.WhiteKeyMap.TryGetValue(entityId, out var whiteKeyLocation))
        {
            location = whiteKeyLocation;
            return true;
        }

        if (_saveData.SlotData.RandomizeBlueKeys &&
            Data.BlueKeyMap.TryGetValue(entityId, out var blueKeyLocation))
        {
            location = blueKeyLocation;
            return true;
        }

        if (entityId == 0 &&
            Data.SpawnedKeyMap.TryGetValue(Player.PlayerDataLocal.currentRoomID, out var spawnedKeyLocation))
        {
            if ((spawnedKeyLocation.Contains("White Key") &&
                 _saveData.SlotData.RandomizeWhiteKeys) ||
                (spawnedKeyLocation.Contains("Blue Key") && _saveData.SlotData.RandomizeBlueKeys) ||
                (spawnedKeyLocation.Contains("Red Key") && _saveData.SlotData.RandomizeRedKeys))
            {
                location = spawnedKeyLocation;
                return true;
            }
        }

        if (_saveData.SlotData.RandomizeRedKeys &&
            Data.RedKeyMap.TryGetValue(entityId, out var redKeyLocation))
        {
            location = redKeyLocation;
            return true;
        }

        location = null;
        return false;
    }

    public static bool TryUpdateDeal(DealProperties.DealID dealId, out string sprite, out string name,
        out string playerName)
    {
        sprite = null;
        name = null;
        playerName = null;

        if (!_saveDataFilled)
        {
            return false;
        }

        if (_saveData.SlotData.RandomizeShop && Data.DealToLocation.TryGetValue(dealId, out var location) &&
            _saveData.SlotData.ShopItems.TryGetValue(location, out var shopItem))
        {
            name = shopItem.Name;
            playerName = shopItem.IsLocal ? null : shopItem.PlayerName;
            sprite = GetIcon(name, shopItem.Flags);
            return true;
        }

        return false;
    }

    public static void MakeCharacterDealsUnavailable()
    {
        if (!_saveValid)
        {
            return;
        }

        if (!Player.PlayerDataLocal.HasUnlockedCharacter(CharacterProperties.Character.Algus))
        {
            Player.PlayerDataLocal.MakeDealNonAvailable(DealProperties.DealID.Deal_SubMenu_Algus);
        }

        if (!Player.PlayerDataLocal.HasUnlockedCharacter(CharacterProperties.Character.Arias))
        {
            Player.PlayerDataLocal.MakeDealNonAvailable(DealProperties.DealID.Deal_SubMenu_Arias);
        }

        if (!Player.PlayerDataLocal.HasUnlockedCharacter(CharacterProperties.Character.Kyuli))
        {
            Player.PlayerDataLocal.MakeDealNonAvailable(DealProperties.DealID.Deal_SubMenu_Kyuli);
        }
    }

    public static void UpdateSaveCharacters(SavePoint savePoint)
    {
        if (!_saveValid)
        {
            return;
        }

        savePoint.algusSpriteAnimator.gameObject.SetActive(
            Player.PlayerDataLocal.HasUnlockedCharacter(CharacterProperties.Character.Algus));
        savePoint.ariasSpriteAnimator.gameObject.SetActive(
            Player.PlayerDataLocal.HasUnlockedCharacter(CharacterProperties.Character.Arias));
        savePoint.kyuliSpriteAnimator.gameObject.SetActive(
            Player.PlayerDataLocal.HasUnlockedCharacter(CharacterProperties.Character.Kyuli));
    }

    public static bool IsSwitchRandomized(int roomId, string linkId, out string location)
    {
        location = null;

        if (!_saveValid)
        {
            return false;
        }

        if (_saveData.SlotData.RandomizeSwitches &&
            Data.LinkToLocation.TryGetValue((roomId, linkId), out var switchLocation))
        {
            location = switchLocation;
            return true;
        }

        return false;
    }

    public static void PressSwitch(int roomId, string linkId)
    {
        if (IsSwitchRandomized(roomId, linkId, out var location))
        {
            SendLocation(location);
        }
    }

    public static void ExploreRoom(Room room)
    {
        if (!_saveValid || !_saveDataFilled || _saveData.SlotData.RandomizeCharacters == RandomizeCharacters.Vanilla)
        {
            return;
        }

        switch (room.roomID)
        {
            case 6672 when !_saveData.SlotData.StartingCharacters.Contains("Algus"):
                SendLocation("Gorgon Tomb - Algus");
                break;
            case 6673 when !_saveData.SlotData.StartingCharacters.Contains("Arias"):
                SendLocation("Gorgon Tomb - Arias");
                break;
            case 6671 when !_saveData.SlotData.StartingCharacters.Contains("Kyuli"):
                SendLocation("Gorgon Tomb - Kyuli");
                break;
        }
    }

    public static bool CollectItem(ItemProperties.ItemID itemId)
    {
        if (!_saveDataFilled)
        {
            return false;
        }

        if (TryGetItemLocation(itemId, out var location))
        {
            SendLocation(location);
            return true;
        }

        return false;
    }

    public static bool CollectEntity(int entityId)
    {
        if (!_saveDataFilled)
        {
            return false;
        }

        if (TryGetEntityLocation(entityId, out var location))
        {
            SendLocation(location);
            return true;
        }

        return false;
    }

    public static bool CanDoorOpen(Key.KeyType keyType)
    {
        if (!_saveDataFilled)
        {
            return true;
        }

        switch (keyType)
        {
            case Key.KeyType.White when _saveData.SlotData.RandomizeWhiteKeys:
            case Key.KeyType.Blue when _saveData.SlotData.RandomizeBlueKeys:
            case Key.KeyType.Red when _saveData.SlotData.RandomizeRedKeys:
                return false;
            default:
                return true;
        }
    }

    public static bool ShouldCheckDeal(DealProperties.DealID dealId)
    {
        return _saveDataFilled && _saveData.SlotData.RandomizeShop && !IsInShop &&
               Data.ItemToDeal.ContainsValue(dealId);
    }

    public static bool IsDealReceived(DealProperties.DealID dealId)
    {
        if (!_saveDataFilled)
        {
            return Player.PlayerDataLocal?.purchasedDeals?.Contains(dealId) ?? false;
        }

        return _saveData.ReceivedDeals != null && _saveData.ReceivedDeals.Contains(dealId);
    }

    public static bool PurchaseDeal(DealProperties.DealID dealId)
    {
        if (_saveDataFilled && _saveData.SlotData.RandomizeShop &&
            Data.DealToLocation.TryGetValue(dealId, out var location))
        {
            SendLocation(location);
            return true;
        }

        return false;
    }

    public static string GetValue(string data, string property)
    {
        var re = new Regex($"_{property}(.*){property}_");
        var match = re.Match(data);
        return match.Success ? match.Groups[1].Value : "";
    }

    public static string GetObjectValue(int objectId, string property)
    {
        var data = SaveManager.CurrentSave.GetObjectData(objectId);
        return GetValue(data, property);
    }

    public static string SetValue(string data, string property, string value)
    {
        var newValue = $"_{property}{value}{property}_";
        var re = new Regex($"_{property}(.*){property}_");
        var match = re.Match(data);
        if (match.Success)
        {
            return re.Replace(data, newValue);
        }

        return data + newValue;
    }

    public static void UpdateObjectData(int objectId, int roomId, string property, string value)
    {
        var room = GameManager.GetRoomFromID(roomId);
        var data = SaveManager.CurrentSave.GetObjectData(objectId);
        SaveManager.SaveObject(objectId, SetValue(data, property, value), roomId);
        room.UpdateObjectState(SaveManager.CurrentSave);
    }

    public static bool OpenDoor((int roomId, int objectId) ids)
    {
        if (GetObjectValue(ids.objectId, "wasOpened").ToLower() == "true")
        {
            return false;
        }

        if (ids.roomId == Player.PlayerDataLocal?.currentRoomID)
        {
            var room = GameManager.GetRoomFromID(ids.roomId);
            var actors = room.GetActorsWithID(ids.objectId);
            if (actors != null && actors.Length > 0)
            {
                var actor = actors[0];
                var door = actor.gameObject.GetComponent<Door>();
                if (door != null)
                {
                    door.OpenDoor();
                }
            }
        }

        UpdateObjectData(ids.objectId, ids.roomId, "wasOpened", "True");
        return true;
    }

    public static void ToggleSwitchLink(SwitchData data)
    {
        foreach (var objId in data.ObjectsToEnable)
        {
            UpdateObjectData(objId, data.RoomId, "objectOn", "True");
        }

        foreach (var objId in data.ObjectsToDisable)
        {
            UpdateObjectData(objId, data.RoomId, "objectOn", "False");
        }
    }

    public static void SendLocation(string location)
    {
        if (Plugin.ArchipelagoClient.Connected)
        {
            Plugin.ArchipelagoClient.SendLocation(location);
        }
        else if (!_saveLoaded)
        {
            Plugin.Logger.LogWarning($"Trying to send location {location} but save hasn't loaded?");
        }
        else
        {
            Plugin.Logger.LogWarning($"No connection, saving location {location} for later");
            _saveData.PendingLocations.Add(location);
        }
    }

    public static void SyncLocations()
    {
        if (Plugin.ArchipelagoClient.SyncLocations(_saveData.PendingLocations))
        {
            _saveData.PendingLocations.Clear();
        }
    }

    public static void Update()
    {
        if (DeathSource != null && _deathCounter == -1 && CanBeKilled())
        {
            _deathCounter = 60;
            IncomingMessages.Enqueue(new()
            {
                Name = "Death",
                PlayerName = DeathSource,
                Receiving = true,
                Flags = ItemFlags.Trap,
            });
            Player.Instance.Kill(false, false);
        }
        else if (_deathCounter == -1)
        {
            Plugin.ArchipelagoClient.CheckForDeath();
        }

        if (_deathCounter > 0)
        {
            _deathCounter--;
        }

        if (_deathCounter == 0)
        {
            _deathCounter = -1;
            DeathSource = null;
        }

        if (TriggerDeath)
        {
            GameManager.Instance?.player?.Kill();
            TriggerDeath = false;
        }

        if (CanGetItem() && IncomingItems.TryDequeue(out var item))
        {
            if (item.Index < _saveData.ItemIndex)
            {
                Plugin.Logger.LogDebug($"Ignoring previously obtained item {item.Id}");
            }
            else
            {
                _saveData.ItemIndex++;
                var display = GiveItem(item);
                if (display)
                {
                    IncomingMessages.Enqueue(item);
                }
            }
        }

        if (CanDisplayMessage() && IncomingMessages.TryDequeue(out var message))
        {
            DisplayItem(message);
        }

        if (UnlockElevators)
        {
            Player.PlayerDataLocal?.UnlockAllElevators();
            UnlockElevators = false;
        }

        if (DumpRoom && Player.PlayerDataLocal?.currentRoomID != null)
        {
            DumpRoom = false;

            var room = GameManager.GetRoomFromID(Player.PlayerDataLocal.currentRoomID);
            Plugin.Logger.LogDebug($"Data for roomId={room.roomID} name={room.name}");
            foreach (var data in SaveManager.CurrentSave.objectsData)
            {
                if (data.RoomID == Player.PlayerDataLocal.currentRoomID)
                {
                    Plugin.Logger.LogDebug($"Id={data.ID} Data='{data.Data}'");
                }
            }
        }

        if (_warpCooldown > 0)
        {
            _warpCooldown--;
        }

        CheckWarp();

        if (ToggleSwitches)
        {
            ToggleSwitches = false;

            if (Player.PlayerDataLocal != null)
            {
                var room = GameManager.GetRoomFromID(Player.PlayerDataLocal.currentRoomID);
                foreach (var obj in room.objectSwitches)
                {
                    if (obj.wasActivated)
                    {
                        obj.ResetObject();
                    }
                    else
                    {
                        obj.ActivateObject(Player.Instance.gameObject);
                    }
                }
            }
        }

        if (ToggleObjects)
        {
            ToggleObjects = false;

            if (Player.PlayerDataLocal != null)
            {
                var room = GameManager.GetRoomFromID(Player.PlayerDataLocal.currentRoomID);
                foreach (var obj in room.switchableObjects)
                {
                    obj.ToggleObject();
                }
            }
        }

        if (ResetDoors)
        {
            ResetDoors = false;

            if (Player.PlayerDataLocal != null)
            {
                var room = GameManager.GetRoomFromID(Player.PlayerDataLocal.currentRoomID);
                Plugin.Logger.LogDebug($"resetting doors for room {room.roomID}");
                foreach (var data in room.GetRoomEntitiesData())
                {
                    Plugin.Logger.LogDebug(
                        $"{data.ID} ({data.ObjectType}) - '{data.Data}'");
                    if (!string.IsNullOrWhiteSpace(data.GetValue("wasOpened")))
                    {
                        UpdateObjectData(data.ID, data.RoomID, "wasOpened", "False");
                    }

                    if (!string.IsNullOrWhiteSpace(data.GetValue("wasActivated")))
                    {
                        UpdateObjectData(data.ID, data.RoomID, "wasActivated", "False");
                    }

                    if (!string.IsNullOrWhiteSpace(data.GetValue("objectOn")))
                    {
                        UpdateObjectData(data.ID, data.RoomID, "objectOn", "True");
                    }
                }
            }
        }

        if (MoveDirection != null)
        {
            if (Player.PlayerDataLocal != null)
            {
                var pos = Player.Instance.transform.position;
                switch (MoveDirection)
                {
                    case "up":
                        pos.y += 20;
                        break;
                    case "down":
                        pos.y -= 20;
                        break;
                    case "left":
                        pos.x -= 20;
                        break;
                    case "right":
                        pos.x += 20;
                        break;
                }

                Player.Instance.transform.position = pos;
            }

            MoveDirection = null;
        }

        if (RoomWarp != -1)
        {
            var roomId = RoomWarp;
            RoomWarp = -1;

            if (Player.PlayerDataLocal != null)
            {
                var room = GameManager.GetRoomFromID(roomId);
                if (room != null)
                {
                    Player.Instance.transform.position = room.roomInitialPosition + new Vector2(50, 50);
                    CameraManager.MoveCameraTo(room.roomInitialPosition);
                }
            }
        }
    }

    public static bool CanWarp(string destination)
    {
#if DEBUG
        return true;
#endif

        if (destination == "Last Checkpoint")
        {
            return true;
        }

        if (Data.Checkpoints.TryGetValue(destination, out var checkpoint))
        {
            if (checkpoint.RoomId == Player.PlayerDataLocal.currentRoomID)
            {
                return false;
            }
            else if (Player.PlayerDataLocal.discoveredRooms.Contains(checkpoint.RoomId) &&
                     (checkpoint.Id != 2669 || GetObjectValue(4338, "wasActivated").ToLower() == "true"))
            {
                return true;
            }
        }

        return false;
    }

    private static void CheckWarp()
    {
        if (WarpDestination == null)
        {
            return;
        }

        var destination = WarpDestination;
        WarpDestination = null;

        if (Player.Instance == null || Player.PlayerDataLocal == null || Player.Instance.isInElevator ||
            _warpCooldown > 0)
        {
            return;
        }

        Vector3 playerPos;
        Vector2 cameraPos;

        if (destination == "Last Checkpoint")
        {
            var room = GameManager.GetRoomFromID(Player.PlayerDataLocal.lastCheckpointData.checkpointRoomID);
            playerPos = new(Player.PlayerDataLocal.lastCheckpointX, Player.PlayerDataLocal.lastCheckpointY, 0);
            cameraPos = room.roomInitialPosition;
        }
        else if (Data.Checkpoints.TryGetValue(destination, out var checkpoint))
        {
            if (CanWarp(destination))
            {
                playerPos = checkpoint.PlayerPos;
                cameraPos = checkpoint.CameraPos;
            }
            else
            {
                Plugin.Logger.LogWarning("You don't have that checkpoint unlocked...");
                return;
            }
        }
        else
        {
            Plugin.Logger.LogWarning($"Cannot find warp: {destination}");
            return;
        }

        Player.Instance.transform.position = playerPos;
        CameraManager.MoveCameraTo(cameraPos);
        AudioManager.Play("thunder");
        AudioManager.Play("wall-gem");
        _warpCooldown = 60;
    }
}