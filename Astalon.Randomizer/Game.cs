using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Archipelago.MultiClient.Net.Enums;
using BepInEx.Unity.IL2CPP.Utils;
using Newtonsoft.Json;
using PathologicalGames;
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

public static class Game
{
    public const string Name = "Astalon";
    public const int SaveObjectId = 333000;
    public const int SaveRoomId = -1;

    public static Queue<ApItemInfo> IncomingItems { get; } = new();
    public static Queue<ApItemInfo> IncomingMessages { get; } = new();
    public static string DeathSource { get; private set; }
    public static bool ReceivingItem { get; set; }
    public static bool IsInShop { get; set; }
    public static int QueuedCutscenes { get; set; }
    public static int QueuedRocks { get; set; }

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
    private static bool _saveInitialized;
    private static int _deathCounter = -1;
    private static int _warpCooldown;
    private static bool _isWarping;
    private static bool _activatingZeekRoom;
    private static bool _activatingBramRoom;
    private static int _activatingElevator = -1;
    private static bool _cutscenePlaying;

#if DEBUG
    private static tk2dBaseSprite _baseSprite;
    private static tk2dSpriteCollectionData _spriteCollectionData;
    private static tk2dSpriteAnimationClip _spriteAnimationClip;
    // private static bool _injectedSprite;
    // private static int _injectedSpriteId;
    private static bool _injectedAnimation;

    #region AnimationExperiments

    public static void Awake()
    {
        var apTexture = LoadImageAsTexture("ap-item.png");
        var gameObject = tk2dSprite.CreateFromTexture(
            apTexture,
            tk2dSpriteCollectionSize.ForTk2dCamera(),
            new(0, 0, apTexture.width, apTexture.height),
            new(0.5f, 0.5f));
        _baseSprite = gameObject.GetComponent<tk2dSprite>();
        _baseSprite.collection.spriteDefinitions[0].name = "AP_ITEM";

        _spriteCollectionData = tk2dSpriteCollectionData.CreateFromTexture(
            LoadImageAsTexture("multi-images.png"),
            tk2dSpriteCollectionSize.ForTk2dCamera(),
            new(["AP_ITEM", "AP_ITEM_BRIGHT"]),
            new([new(0, 0, 16, 16), new(16, 0, 16, 16)]),
            new([new(8, 8), new(8, 8)]));

        _spriteAnimationClip = new()
        {
            fps = 6,
            loopStart = 0,
            name = "AP_ITEM",
            wrapMode = tk2dSpriteAnimationClip.WrapMode.Loop,
            frames = new(
            [
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
            ]),
        };
    }

    public static Texture2D LoadImageAsTexture(string filename)
    {
        var path = $"{DataDir}/{filename}";
        var bytes = File.ReadAllBytes(path);
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.LoadImage(bytes, markNonReadable: true);
        texture.filterMode = FilterMode.Bilinear;
        return texture;
    }

    public static void UpdateSprite(tk2dBaseSprite sprite)
    {
        // if (!_injectedSprite)
        // {
        //     Plugin.Logger.LogDebug("Injecting");
        //     var newDefinitions = sprite.collection.spriteDefinitions.ToList();
        //     newDefinitions.Add(sprite.collection.spriteDefinitions[0]);
        //     sprite.collection.spriteDefinitions = newDefinitions.ToArray();
        //     _injectedSprite = true;
        //     _injectedSpriteId = sprite.collection.spriteDefinitions.Count - 1;
        // }

        var sr = sprite.GetComponent<SpriteRenderer>();
        Plugin.Logger.LogDebug($"has sr: {sr != null}");
        Plugin.Logger.LogDebug(sr?.sprite);

        // sprite.SetSprite(_injectedSpriteId);
        // sprite.SetSprite(_baseSprite.Collection, 1);
        tk2dSprite.AddComponent(sprite.gameObject, _baseSprite.Collection, 0);
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
#endif

    #region Visuals

    public static void UpdateItem(Item item)
    {
        if (!Plugin.State.Valid || !Plugin.State.SlotData.RandomizeKeyItems || item.itemProperties?.itemID == null)
        {
            return;
        }

        if (Data.ItemToApLocationId.TryGetValue(item.itemProperties.itemID, out var location))
        {
            UpdateEntityAppearance(item.gameObject, location);
        }
    }

    public static void UpdateEntity(GameObject gameObject, int actorId)
    {
        if (!Plugin.State.Valid)
        {
            return;
        }

        ApLocationId? location = null;

        if (Plugin.State.SlotData.RandomizeHealthPickups &&
            Data.HealthMap.TryGetValue(actorId, out var healthLocation))
        {
            location = healthLocation;
        }

        if (Plugin.State.SlotData.RandomizeAttackPickups &&
            Data.AttackMap.TryGetValue(actorId, out var attackLocation))
        {
            location = attackLocation;
        }

        if (Plugin.State.SlotData.RandomizeWhiteKeys &&
            Data.WhiteKeyMap.TryGetValue(actorId, out var whiteLocation))
        {
            location = whiteLocation;
        }

        if (Plugin.State.SlotData.RandomizeBlueKeys &&
            Data.BlueKeyMap.TryGetValue(actorId, out var blueLocation))
        {
            location = blueLocation;
        }

        if (actorId == 0 &&
            Data.SpawnedKeyMap.TryGetValue(Player.PlayerDataLocal.currentRoomID, out var spawnedLocation))
        {
            if ((spawnedLocation == ApLocationId.MechWhiteKeyArena && Plugin.State.SlotData.RandomizeWhiteKeys) ||
                (spawnedLocation != ApLocationId.MechWhiteKeyArena && Plugin.State.SlotData.RandomizeBlueKeys))
            {
                location = spawnedLocation;
            }
        }

        if (Plugin.State.SlotData.RandomizeRedKeys &&
            Data.RedKeyMap.TryGetValue(actorId, out var redLocation))
        {
            location = redLocation;
        }

        if (location != null)
        {
            UpdateEntityAppearance(gameObject, (ApLocationId)location);
        }
    }

    public static void UpdateEntityAppearance(GameObject gameObject, ApLocationId location)
    {
        var itemInfo = Plugin.State.LocationInfos.GetValueOrDefault((long)location);
        var icon = GetIcon(itemInfo);
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
        sprite?.SetSprite(icon);
        // if (icon == "AP_ITEM" && sprite != null)
        // {
        //     UpdateSprite(sprite);
        // }
        // else
        // {
        //     sprite?.SetSprite(icon);
        // }
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

        var serializedData = SaveManager.CurrentSave.GetObjectData(SaveObjectId);
        if (string.IsNullOrWhiteSpace(serializedData))
        {
            Plugin.Logger.LogError("Did not find AP save data. Did you load a casual save?");
            Plugin.ArchipelagoClient.Disconnect();
            return;
        }

        try
        {
            Plugin.State = JsonConvert.DeserializeObject<State>(serializedData);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"Invalid AP save data: {e.Message}");
            Plugin.Logger.LogError(e);
            Plugin.ArchipelagoClient.Disconnect();
            return;
        }

        Plugin.State.CheckedLocations ??= [];
        Plugin.State.ReceivedDeals ??= [];
        Plugin.State.ReceivedElevators ??= [];
        Plugin.State.CheckedElevators ??= [];
        Plugin.State.VisitedCampfires ??= [];

        _saveValid = true;
        Plugin.State.Valid = true;

        if (!Plugin.ArchipelagoClient.Connected)
        {
            Plugin.UpdateConnectionInfo(Plugin.State.Uri, Plugin.State.SlotName, Plugin.State.Password);
            Plugin.ArchipelagoClient.Connect();
        }

        InitializeSave(false);
        ConnectSave();
    }

    public static void SetupNewSave()
    {
        _saveNew = true;
        _saveLoaded = true;
        _saveValid = true;
        Plugin.State.ClearSave();
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

        var seed = Plugin.State.Seed;
        var slotData = Plugin.State.SlotData;
        var isNew = _saveNew;

        if (_saveNew)
        {
            _saveNew = false;
            Plugin.State.Valid = true;

            Plugin.State.LocationInfos = Plugin.ArchipelagoClient.ScoutAllLocations();

            if (slotData.RandomizeCharacters)
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

                if (!slotData.StartingCharacters.Contains(Data.CharacterToName[Player.PlayerDataLocal.currentCharacter]))
                {
                    Player.Instance.CycleCharacterTo(Data.NameToCharacter[slotData.StartingCharacters[0]]);
                }
            }

            UpdateSaveData();
        }
        else if (seed != Plugin.State.Seed)
        {
            Plugin.Logger.LogError("Mismatched seed detected. Did you load the right save?");
            Plugin.ArchipelagoClient.Disconnect();
        }
        else
        {
            Plugin.UpdateConnectionInfo();
        }

        Plugin.ArchipelagoClient.SyncLocations(Plugin.State.CheckedLocations);
        InitializeSave(isNew);

        return true;
    }

    public static void UpdateSaveData()
    {
        if (!Plugin.State.Valid)
        {
            return;
        }

        var data = JsonConvert.SerializeObject(Plugin.State);
        SaveManager.SaveObject(SaveObjectId, data, SaveRoomId);
    }

    public static void InitializeSave(bool isNew)
    {
        if (_saveInitialized)
        {
            return;
        }

        if (Plugin.State.SlotData.SkipCutscenes || Plugin.State.SlotData.RandomizeCharacters)
        {
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

        if (Plugin.State.SlotData.SkipCutscenes)
        {
            Player.PlayerDataLocal.cs_bkbossfinal1 = true;
            Player.PlayerDataLocal.cs_bkbossintro1 = true;
            Player.PlayerDataLocal.cs_bkFinalToMedusa = true;
            Player.PlayerDataLocal.cs_finalPlatformRide = true;
            Player.PlayerDataLocal.cs_ending1 = true;
            if (Player.PlayerDataLocal.epimetheusSequence == 0)
            {
                Player.PlayerDataLocal.epimetheusSequence = 10;
                Player.PlayerDataLocal.deaths = 1;
            }

            if (!Player.PlayerDataLocal.firstElevatorLit)
            {
                Player.PlayerDataLocal.firstElevatorLit = true;
                UpdateElevatorList();

                // blocks around first elevator
                for (var i = 6641; i < 6647; i++)
                {
                    UpdateObjectData(i, 6629, "objectOn", "False");
                }
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

        if (Plugin.State.SlotData.CostMultiplier != 100 && GameManager.Instance.itemManager
                .GetDealProperties(DealProperties.DealID.Deal_Gift).DealPrice == 666)
        {
            var mul = Plugin.State.SlotData.CostMultiplier / 100f;
            foreach (var deal in GameManager.Instance.itemManager.gameDeals)
            {
                deal.dealPrice = (int)Math.Round(deal.dealPrice * mul);
            }
        }

        if (Plugin.State.SlotData.FastBloodChalice)
        {
            Player.Instance.regenInterval = 0.2f;
        }

        if (Plugin.State.SlotData.CheapKyuliRay)
        {
            Player.Instance.shiningRayCost = 50;
        }

        if (isNew && Plugin.State.SlotData.ScaleCharacterStats)
        {
            foreach (var entry in Plugin.State.SlotData.CharacterStrengths)
            {
                var character = Data.NameToCharacter[entry.Key];
                var scaling = entry.Value;
                var strDeal = Data.CharacterToStrDeal[character];
                var defDeal = Data.CharacterToDefDeal[character];
                var strProps = GameManager.Instance.itemManager.GetDealProperties(strDeal);
                var defProps = GameManager.Instance.itemManager.GetDealProperties(defDeal);
                var str = (int)Math.Round(20.0 * scaling);
                var def = (int)Math.Round(10.0 * scaling);
                str = Math.Min(Math.Max(0, str), strProps.dealMaxLevel);
                def = Math.Min(Math.Max(0, def), defProps.dealMaxLevel);

                if (str >= strProps.dealMaxLevel)
                {
                    Player.PlayerDataLocal.purchasedDeals.Add(strDeal);
                    Player.PlayerDataLocal.MakeDealNonAvailable(strDeal);
                }
                if (def >= defProps.dealMaxLevel)
                {
                    Player.PlayerDataLocal.purchasedDeals.Add(defDeal);
                    Player.PlayerDataLocal.MakeDealNonAvailable(defDeal);
                }

                switch (character)
                {
                    case CharacterProperties.Character.Algus:
                        Player.PlayerDataLocal.strengthBonusAlgus = str;
                        Player.PlayerDataLocal.defenseBonusAlgus = def;
                        break;
                    case CharacterProperties.Character.Arias:
                        Player.PlayerDataLocal.strengthBonusArias = str;
                        Player.PlayerDataLocal.defenseBonusArias = Math.Max(def, 2);
                        break;
                    case CharacterProperties.Character.Kyuli:
                        Player.PlayerDataLocal.strengthBonusKyuli = Math.Max(str, 2);
                        Player.PlayerDataLocal.defenseBonusKyuli = def;
                        break;
                    case CharacterProperties.Character.Bram:
                        Player.PlayerDataLocal.strengthBonusBram = Math.Max(str, 2);
                        Player.PlayerDataLocal.defenseBonusBram = Math.Max(def, 1);
                        break;
                    case CharacterProperties.Character.Zeek:
                        Player.PlayerDataLocal.strengthBonusZeek = str;
                        Player.PlayerDataLocal.defenseBonusZeek = def;
                        break;
                }
            }
        }

        // maybe for future version
        // Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.BlackKnight);
        // Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Gargoyle);
        // Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.OldMan);

        _saveInitialized = true;
    }

    public static void ExitSave()
    {
        _saveNew = false;
        _saveLoaded = false;
        _saveValid = false;
        _saveInitialized = false;
        Plugin.State.ClearConnection();
        Plugin.State.ClearSave();
        SaveManager.SaveObject(SaveObjectId, "", SaveRoomId);
    }

    #endregion

    public static bool CanGetItem()
    {
        if (!Plugin.State.Valid || !_saveInitialized)
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
        if (!Plugin.State.Valid)
        {
            return true;
        }

        return Player.PlayerDataLocal.unlockedCharacters.Count > 1;
    }

    public static string GetDefaultIcon(ItemFlags flags = ItemFlags.None)
    {
        if (flags.HasFlag(ItemFlags.Advancement) || flags.HasFlag(ItemFlags.Trap))
        {
            return "BlueOrb_1";
        }

        return "Orb_Idle_1";
    }

    public static string GetIcon(ApItemInfo itemInfo)
    {
        if (itemInfo == null)
        {
            return "Orb_Idle_1";
        }

        if (!itemInfo.IsAstalon)
        {
            return GetDefaultIcon(itemInfo.Flags);
        }

        var itemId = (ApItemId)itemInfo.Id;

        if (Data.ApItemIdToIcon.TryGetValue(itemId, out var icon))
        {
            return icon;
        }

        var itemName = itemId.ToString();

        if (itemName.StartsWith("WhiteDoor"))
        {
            return "WhiteKey_1";
        }

        if (itemName.StartsWith("BlueDoor"))
        {
            return "BlueKey_1";
        }

        if (itemName.StartsWith("RedDoor"))
        {
            return "RedKey_1";
        }

        if (itemName.StartsWith("Orbs"))
        {
            return "SoulOrb_Big";
        }

        if (itemName.StartsWith("Elevator"))
        {
            return "ElevatorMenu_Icon_OldMan";
        }

        if (itemName.StartsWith("Switch") || itemName.StartsWith("Crystal") || itemName.StartsWith("Face"))
        {
            return "Frog";
        }

        return GetDefaultIcon(itemInfo.Flags);
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
            "Candle_1_Lit_1_01" => "Candle1_Lit",
            "Frog" => null,
            "AP_ITEM" => null,
            _ => icon,
        };
    }

    public static ItemBox FormatItemBox(ApItemInfo itemInfo)
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

        var icon = GetIcon(itemInfo);
        var sound = "pickup";
        if (itemInfo.Flags.HasFlag(ItemFlags.Advancement))
        {
            sound = "secret";
        }
        else if (itemInfo.Flags.HasFlag(ItemFlags.Trap))
        {
            sound = "evil-laugh";
        }

        return new()
        {
            Message = message,
            Icon = icon,
            Sound = sound,
        };
    }

    public static bool DisplayItem(ApItemInfo itemInfo)
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

    public static bool GiveItem(ApItemInfo itemInfo)
    {
        if (!CanGetItem())
        {
            return false;
        }

        ReceivingItem = true;

        var apItemId = (ApItemId)itemInfo.Id;
        var itemName = apItemId.ToString();
        Plugin.Logger.LogDebug($"Giving item: {itemName}");

        if (itemName.StartsWith("UpgradeAttack"))
        {
            var bonus = int.Parse(itemName[13..]);
            Player.PlayerDataLocal.IncreaseStrengthBonusBy(bonus);
        }
        else if (itemName.StartsWith("UpgradeMaxHp"))
        {
            var bonus = int.Parse(itemName[12..]);
            Player.PlayerDataLocal.healthItemBonus += bonus;
            Player.PlayerDataLocal.currentHealth += bonus;
            GameplayUIManager.Instance?.UpdateHealthBar(Player.Instance, true);
        }
        else if (itemName.StartsWith("Orbs"))
        {
            var amount = int.Parse(itemName[4..]);
            Player.Instance.CollectOrbs(amount);
        }
        else if (Data.ApItemIdToItem.TryGetValue(apItemId, out var itemId))
        {
            Player.PlayerDataLocal.CollectItem(itemId);
            Player.PlayerDataLocal.EnableItem(itemId);

            switch (itemId)
            {
                case ItemProperties.ItemID.AscendantKey:
                    Player.PlayerDataLocal.elevatorsOpened = true;
                    UpdateElevatorList();
                    if (Player.PlayerDataLocal.elevatorsFound != null)
                    {
                        foreach (var roomId in Player.PlayerDataLocal.elevatorsFound)
                        {
                            Player.PlayerDataLocal.UnlockElevator(roomId);
                        }
                    }
                    if (Plugin.State.SlotData.ApexElevator == ApexElevator.Vanilla)
                    {
                        Player.PlayerDataLocal.UnlockElevator((int)ElevatorId.Apex);
                    }
                    Player.PlayerDataLocal.UnlockElevator((int)ElevatorId.Gt1);

                    break;
                case ItemProperties.ItemID.ZeekItem:
                    Player.PlayerDataLocal.cyclopsDenKey = true;
                    Plugin.State.ReceivedCyclopsKey = true;
                    break;
                case ItemProperties.ItemID.PrincesCrown:
                    Plugin.State.ReceivedCrown = true;
                    break;
                case ItemProperties.ItemID.AthenasBell:
                    Player.Instance.SetCanChangeCharacterTo(true);
                    Player.Instance.UpdateItems();
                    break;
                case ItemProperties.ItemID.BloodChalice:
                    Player.Instance.UpdateItems();
                    break;
                case ItemProperties.ItemID.LunarianBow:
                    Player.Instance.UpdateWeapon();
                    break;
            }
        }
        else if (Data.WhiteDoorMap.TryGetValue(apItemId, out var whiteIds))
        {
            var result = OpenDoor(whiteIds);
            ReceivingItem = false;
            return result;
        }
        else if (Data.BlueDoorMap.TryGetValue(apItemId, out var blueIds))
        {
            var result = OpenDoor(blueIds);
            ReceivingItem = false;
            return result;
        }
        else if (Data.RedDoorMap.TryGetValue(apItemId, out var redIds))
        {
            var result = OpenDoor(redIds);
            ReceivingItem = false;
            return result;
        }
        else if (Data.ItemToDeal.TryGetValue(apItemId, out var dealId))
        {
            if (Plugin.State.SlotData.RandomizeShop)
            {
                Plugin.State.ReceivedDeals.Add(dealId);
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
        else if (Data.ItemToLink.TryGetValue(apItemId, out var switchData))
        {
            ToggleSwitchLink(switchData);
        }
        else if (Data.ItemToElevator.TryGetValue(apItemId, out var elevatorId))
        {
            if (Plugin.State.SlotData.RandomizeElevator)
            {
                Plugin.State.ReceivedElevators.Add(elevatorId);
                UpdateElevatorList();
            }
            else
            {
                Player.PlayerDataLocal.UnlockElevator(elevatorId);
            }
        }
        else
        {
            switch (apItemId)
            {
                case ApItemId.EyeGold:
                    Plugin.State.CollectedGoldEyes += 1;
                    break;
                case ApItemId.CharacterAlgus:
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Algus);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Algus, false);
                    break;
                case ApItemId.CharacterArias:
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Arias);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Arias, false);
                    break;
                case ApItemId.CharacterKyuli:
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Kyuli);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Kyuli, false);
                    break;
                case ApItemId.CharacterZeek:
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Zeek);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Zeek, false);
                    break;
                case ApItemId.CharacterBram:
                    Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Bram);
                    Player.PlayerDataLocal.MakeDealAvailable(DealProperties.DealID.Deal_SubMenu_Bram, false);
                    break;
                case ApItemId.KeyWhite:
                    Player.PlayerDataLocal.AddKey(Key.KeyType.White);
                    break;
                case ApItemId.KeyBlue:
                    Player.PlayerDataLocal.AddKey(Key.KeyType.Blue);
                    break;
                case ApItemId.KeyRed:
                    Player.PlayerDataLocal.AddKey(Key.KeyType.Red);
                    break;
                case ApItemId.Heal5:
                    Player.Instance.Heal(5);
                    break;
                case ApItemId.TrapCutscene:
                    QueuedCutscenes++;
                    break;
                case ApItemId.TrapRocks:
                    QueuedRocks++;
                    break;
                default:
                    Plugin.Logger.LogWarning($"Item {itemInfo.Id} - {itemName} not found");
                    ReceivingItem = false;
                    return false;
            }
        }

        ReceivingItem = false;
        return true;
    }

    public static void ElevatorUnlocked(int roomId)
    {
        if (!Plugin.State.Valid || !Plugin.State.SlotData.RandomizeElevator)
        {
            return;
        }

        if ((roomId != ((int)ElevatorId.Apex) || Plugin.State.SlotData.ApexElevator == ApexElevator.Included) && Data.ElevatorToLocation.TryGetValue(roomId, out var location))
        {
            SendLocation(location);
            Plugin.State.CheckedElevators.Add(roomId);
        }
    }

    public static void UpdateElevatorList()
    {
        if (!Plugin.State.Valid)
        {
            return;
        }

        if (Plugin.State.SlotData.RandomizeElevator)
        {
            var existingElevators = Player.PlayerDataLocal.elevatorsFound ?? new();
            var elevators = existingElevators.Clone();

            foreach (var elevatorId in existingElevators)
            {
                if (elevatorId != ((int)ElevatorId.Gt1) && (elevatorId != ((int)ElevatorId.Apex) || Plugin.State.SlotData.ApexElevator != ApexElevator.Vanilla) && !Plugin.State.ReceivedElevators.Contains(elevatorId))
                {
                    elevators.Remove(elevatorId);
                }
            }

            foreach (var elevatorId in Plugin.State.ReceivedElevators)
            {
                if (!existingElevators.Contains(elevatorId))
                {
                    elevators.Add(elevatorId);
                }
            }

            if (!existingElevators.Contains((int)ElevatorId.Gt1))
            {
                elevators.Add((int)ElevatorId.Gt1);
            }

            if (Plugin.State.SlotData.ApexElevator == ApexElevator.Vanilla && !existingElevators.Contains((int)ElevatorId.Apex))
            {
                elevators.Add((int)ElevatorId.Apex);
            }

            Player.PlayerDataLocal.elevatorsFound = elevators;
        }
        else if (Player.PlayerDataLocal.elevatorsFound != null && Plugin.State.SlotData.ApexElevator != ApexElevator.Vanilla && !Player.PlayerDataLocal.discoveredRooms.Contains((int)ElevatorId.Apex))
        {
            Player.PlayerDataLocal.elevatorsFound.Remove((int)ElevatorId.Apex);
        }
    }

    public static void CampfireVisited(int id)
    {
        if (!Plugin.State.Valid || Plugin.State.VisitedCampfires.Contains(id))
        {
            return;
        }

        Plugin.State.VisitedCampfires.Add(id);
    }

    public static bool CharacterUnlocked(CharacterProperties.Character character)
    {
        if (!Plugin.State.Valid || !_saveInitialized || ReceivingItem || !Plugin.State.SlotData.RandomizeCharacters)
        {
            return false;
        }

        if (character == CharacterProperties.Character.Zeek && !Plugin.State.SlotData.StartingCharacters.Contains("Zeek"))
        {
            SendLocation(ApLocationId.MechZeek);
            Plugin.State.CheckedZeek = true;
            return true;
        }

        if (character == CharacterProperties.Character.Bram && !Plugin.State.SlotData.StartingCharacters.Contains("Bram"))
        {
            SendLocation(ApLocationId.TrBram);
            Plugin.State.CheckedBram = true;
            return true;
        }

        return false;
    }

    public static void HandleDeath()
    {
        if (!Plugin.State.Valid)
        {
            return;
        }

        if (_deathCounter <= 0)
        {
            Plugin.ArchipelagoClient.SendDeath();
        }

        if (Plugin.State.SlotData.AlwaysRestoreCandles)
        {
            PlayerData.ResetCandles();
        }
    }

    public static void ReceiveDeath(string source)
    {
        DeathSource = source;
    }

    public static bool TryGetItemLocation(ItemProperties.ItemID itemId, out ApLocationId? location)
    {
        location = null;
        if (!Plugin.State.Valid || !Plugin.State.SlotData.RandomizeKeyItems)
        {
            return false;
        }

        if (Data.ItemToApLocationId.TryGetValue(itemId, out var apLocation))
        {
            location = apLocation;
            return true;
        }

        return false;
    }

    public static bool TryGetEntityLocation(int entityId, out ApLocationId? location)
    {
        location = null;
        if (!Plugin.State.Valid)
        {
            return false;
        }

        if (Plugin.State.SlotData.RandomizeHealthPickups &&
            Data.HealthMap.TryGetValue(entityId, out var healthLocation))
        {
            location = healthLocation;
            return true;
        }

        if (Plugin.State.SlotData.RandomizeAttackPickups &&
            Data.AttackMap.TryGetValue(entityId, out var attackLocation))
        {
            location = attackLocation;
            return true;
        }

        if (Plugin.State.SlotData.RandomizeWhiteKeys &&
            Data.WhiteKeyMap.TryGetValue(entityId, out var whiteKeyLocation))
        {
            location = whiteKeyLocation;
            return true;
        }

        if (Plugin.State.SlotData.RandomizeBlueKeys &&
            Data.BlueKeyMap.TryGetValue(entityId, out var blueKeyLocation))
        {
            location = blueKeyLocation;
            return true;
        }

        if (entityId == 0 &&
            Data.SpawnedKeyMap.TryGetValue(Player.PlayerDataLocal.currentRoomID, out var spawnedKeyLocation))
        {
            if ((spawnedKeyLocation == ApLocationId.MechWhiteKeyArena && Plugin.State.SlotData.RandomizeWhiteKeys) ||
                (spawnedKeyLocation != ApLocationId.MechWhiteKeyArena && Plugin.State.SlotData.RandomizeBlueKeys))
            {
                location = spawnedKeyLocation;
                return true;
            }
        }

        if (Plugin.State.SlotData.RandomizeRedKeys &&
            Data.RedKeyMap.TryGetValue(entityId, out var redKeyLocation))
        {
            location = redKeyLocation;
            return true;
        }

        location = null;
        return false;
    }

    public static bool TryUpdateDeal(DealProperties.DealID dealId, out string sprite, out string name, out string playerName)
    {
        sprite = null;
        name = null;
        playerName = null;

        if (!Plugin.State.Valid)
        {
            return false;
        }

        if (Plugin.State.SlotData.RandomizeShop && Data.DealToLocation.TryGetValue(dealId, out var location) &&
            Plugin.State.LocationInfos.TryGetValue((long)location, out var shopItem))
        {
            name = shopItem.Name;
            playerName = shopItem.IsLocal ? null : shopItem.PlayerName;
            sprite = GetIcon(shopItem);

            if (dealId == DealProperties.DealID.Deal_DeathOrb || dealId == DealProperties.DealID.Deal_Gift)
            {
                name = $"*{name}";
            }

            return true;
        }

        return false;
    }

    public static bool TryHasItem(ItemProperties.ItemID itemId, out bool result)
    {
        result = false;

        if (!Plugin.State.Valid)
        {
            return false;
        }

        if (Player.PlayerDataLocal.currentRoomID == 4112 && itemId == ItemProperties.ItemID.GorgonEyeGreen &&
            Plugin.State.IsEyeHunt() && (Player.PlayerDataLocal.collectedItems?.Contains(ItemProperties.ItemID.GorgonEyeGreen) ?? false))
        {
            result = Plugin.State.CollectedGoldEyes >= Plugin.State.SlotData.AdditionalEyesRequired;
            return true;
        }

        if (Plugin.State.SlotData.RandomizeKeyItems && _activatingZeekRoom)
        {
            if (itemId == ItemProperties.ItemID.PrincesCrown)
            {
                if (!Plugin.State.CheckedCyclopsIdol)
                {
                    result = false;
                }
                else
                {
                    result = Plugin.State.ReceivedCrown;
                }

                return true;
            }

            if (itemId == ItemProperties.ItemID.ZeekItem)
            {
                result = Plugin.State.CheckedCyclopsIdol;
                return true;
            }
        }

        return false;
    }

    public static bool TryHasUnlockedCharacter(CharacterProperties.Character character, out bool result)
    {
        result = false;

        if (!Plugin.State.Valid)
        {
            return false;
        }

        if (_activatingZeekRoom && character == CharacterProperties.Character.Zeek)
        {
            if (!Plugin.State.SlotData.RandomizeCharacters)
            {
                if (Plugin.State.CheckedCyclopsIdol || !Plugin.State.SlotData.RandomizeKeyItems)
                {
                    return false;
                }
                else
                {
                    result = false;
                    return true;
                }
            }

            result = Plugin.State.CheckedZeek;
            return true;
        }

        if (_activatingBramRoom && character == CharacterProperties.Character.Bram && Plugin.State.SlotData.RandomizeCharacters)
        {
            result = Plugin.State.CheckedBram;
            return true;
        }

        return false;
    }

    public static bool TryElevatorFound(int roomId, out bool result)
    {
        result = false;

        if (!Plugin.State.Valid || !Plugin.State.SlotData.RandomizeElevator || _activatingElevator != roomId)
        {
            return false;
        }

        if ((roomId != ((int)ElevatorId.Apex) || Plugin.State.SlotData.ApexElevator == ApexElevator.Included) && Data.ElevatorToLocation.ContainsKey(roomId))
        {
            result = Plugin.State.CheckedElevators.Contains(roomId);
            return true;
        }

        return false;
    }

    public static void MakeCharacterDealsUnavailable()
    {
        if (!Plugin.State.Valid)
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
        if (!Plugin.State.Valid)
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

    public static bool IsSwitchRandomized(int roomId, string linkId, out ApLocationId? location)
    {
        location = null;

        if (!Plugin.State.Valid)
        {
            return false;
        }

        if (Plugin.State.SlotData.RandomizeSwitches &&
            Data.LinkToLocation.TryGetValue((roomId, linkId), out var switchLocation))
        {
            location = switchLocation;
            return true;
        }

        return false;
    }

    public static void PressSwitch(int roomId, string linkId)
    {
        if (IsSwitchRandomized(roomId, linkId, out var location) && location != null)
        {
            SendLocation((ApLocationId)location);
        }
    }

    public static void ExploreRoom(Room room)
    {
        if (!Plugin.State.Valid)
        {
            return;
        }

        var area = room.GetRoomArea();
        switch (room.roomID)
        {
            case 2704:
            case 2705:
                area = 19;
                break;
            case 8771:
                area = 5;
                break;
            case 1080:
            case 10535:
                area = 3;
                break;
            case 803:
            case 3947:
                area = 2;
                break;
            case 248:
                area = 1;
                break;
        }
        if (area != 0 && area != 22)
        {
            Plugin.ArchipelagoClient.StoreArea(area);
        }

        float newRegen;
        if (Plugin.State.SlotData.FastBloodChalice && room.roomType != "boss")
        {
            newRegen = 0.2f;
        }
        else
        {
            newRegen = 1f;
        }
        if (newRegen != Player.Instance.regenInterval)
        {
            Player.Instance.regenInterval = newRegen;
            if (Player.PlayerDataLocal.HasEnabledItem(ItemProperties.ItemID.BloodChalice))
            {
                Player.Instance.RemoveRegen();
                Player.Instance.UpdateItems();
            }
        }

        if (room.roomID == 3728 && Plugin.State.SlotData.RandomizeKeyItems)
        {
            Player.PlayerDataLocal.cyclopsDenKey = Plugin.State.ReceivedCyclopsKey;
        }

        if (!Plugin.State.SlotData.RandomizeCharacters)
        {
            return;
        }

        switch (room.roomID)
        {
            case 6672 when !Plugin.State.SlotData.StartingCharacters.Contains("Algus"):
                SendLocation(ApLocationId.GtAlgus);
                break;
            case 6673 when !Plugin.State.SlotData.StartingCharacters.Contains("Arias"):
                SendLocation(ApLocationId.GtArias);
                break;
            case 6671 when !Plugin.State.SlotData.StartingCharacters.Contains("Kyuli"):
                SendLocation(ApLocationId.GtKyuli);
                break;
        }
    }

    public static void ActivateZeekRoom()
    {
        if (!Plugin.State.Valid)
        {
            return;
        }

        _activatingZeekRoom = true;
    }

    public static void DeactivateZeekRoom()
    {
        _activatingZeekRoom = false;
    }

    public static void ActivateBramRoom()
    {
        if (!Plugin.State.Valid)
        {
            return;
        }

        _activatingBramRoom = true;
    }

    public static void DeactivateBramRoom()
    {
        _activatingBramRoom = false;
    }

    public static void ActivateElevator(int roomId)
    {
        if (!Plugin.State.Valid)
        {
            return;
        }

        _activatingElevator = roomId;
    }

    public static void DeactivateElevator()
    {
        _activatingElevator = -1;
    }

    public static bool CollectItem(ItemProperties.ItemID itemId)
    {
        if (!Plugin.State.Valid || !Plugin.State.SlotData.RandomizeKeyItems)
        {
            return false;
        }

        if (TryGetItemLocation(itemId, out var location))
        {
            SendLocation((ApLocationId)location);

            if (itemId == ItemProperties.ItemID.ZeekItem)
            {
                Plugin.State.CheckedCyclopsIdol = true;
            }

            return true;
        }

        return false;
    }

    public static bool CollectEntity(int entityId)
    {
        if (!Plugin.State.Valid)
        {
            return false;
        }

        if (TryGetEntityLocation(entityId, out var location))
        {
            SendLocation((ApLocationId)location);
            return true;
        }

        return false;
    }

    public static bool CanDoorOpen(Key.KeyType keyType)
    {
        if (!Plugin.State.Valid)
        {
            return true;
        }

        switch (keyType)
        {
            case Key.KeyType.White when Plugin.State.SlotData.RandomizeWhiteKeys:
            case Key.KeyType.Blue when Plugin.State.SlotData.RandomizeBlueKeys:
            case Key.KeyType.Red when Plugin.State.SlotData.RandomizeRedKeys:
                return false;
            default:
                return true;
        }
    }

    public static bool ShouldCheckDeal(DealProperties.DealID dealId)
    {
        return Plugin.State.Valid && Plugin.State.SlotData.RandomizeShop && !IsInShop && Data.ItemToDeal.ContainsValue(dealId);
    }

    public static bool ShouldUnlockDeal(DealProperties.DealID dealId)
    {
        if (!Plugin.State.Valid || !Plugin.State.SlotData.RandomizeCharacters)
        {
            return true;
        }

        if (dealId != DealProperties.DealID.Deal_SubMenu_Zeek && dealId != DealProperties.DealID.Deal_SubMenu_Bram)
        {
            return true;
        }

        return ReceivingItem;
    }

    public static bool IsDealReceived(DealProperties.DealID dealId)
    {
        if (!Plugin.State.Valid)
        {
            return Player.PlayerDataLocal?.purchasedDeals?.Contains(dealId) ?? false;
        }

        return Plugin.State.ReceivedDeals != null && Plugin.State.ReceivedDeals.Contains(dealId);
    }

    public static bool PurchaseDeal(DealProperties.DealID dealId)
    {
        if (Plugin.State.Valid && Plugin.State.SlotData.RandomizeShop &&
            Data.DealToLocation.TryGetValue(dealId, out var location))
        {
            SendLocation(location);
            return true;
        }

        return false;
    }

    public static bool SpawnParticle(Transform particleParent)
    {
        if (!Plugin.State.Valid)
        {
            return true;
        }

        var actor = particleParent.GetComponent<Actor>();
        if (actor == null)
        {
            return true;
        }

        if (Plugin.State.SlotData.RandomizeCandles && Data.CandleToLocation.TryGetValue(actor.actorID, out var candleLocation) && !IsLocationChecked(candleLocation))
        {
            SendLocation(candleLocation);
            return false;
        }

        return true;
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

    public static bool OpenDoor(ActorIds ids)
    {
        if (GetObjectValue(ids.actorId, "wasOpened").ToLower() == "true")
        {
            return false;
        }

        if (ids.roomId == Player.PlayerDataLocal?.currentRoomID)
        {
            var room = GameManager.GetRoomFromID(ids.roomId);
            var actors = room.GetActorsWithID(ids.actorId);
            if (actors != null && actors.Length > 0)
            {
                var actor = actors[0];
                var door = actor.gameObject.GetComponent<Door>();
                door?.OpenDoor();
            }
        }

        UpdateObjectData(ids.actorId, ids.roomId, "wasOpened", "True");
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

    public static void SendLocation(ApLocationId location)
    {
        if (Plugin.ArchipelagoClient.Connected)
        {
            Plugin.ArchipelagoClient.SendLocation((long)location);
        }
        else
        {
            Plugin.Logger.LogWarning($"No connection, saving location {location} for later");
        }

        if (_saveLoaded)
        {
            Plugin.State.CheckedLocations.Add((long)location);
        }
    }

    public static bool IsLocationChecked(ApLocationId location)
    {
        if (Plugin.ArchipelagoClient.Connected)
        {
            return Plugin.ArchipelagoClient.IsLocationChecked((long)location);
        }

        return Plugin.State.CheckedLocations.Contains((long)location);
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
            if (item.Index < Plugin.State.ItemIndex)
            {
                Plugin.Logger.LogDebug($"Ignoring previously obtained item {item.Id}");
            }
            else
            {
                Plugin.State.ItemIndex++;
                var display = GiveItem(item);
                // don't show pre-collected items
                if (display && item.LocationId != -2)
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

        if (QueuedCutscenes > 0 && IsGameStateNormal())
        {
            PlayCutsceneTrap();
            QueuedCutscenes--;
        }

        if (QueuedRocks > 0 && CanEnableRocks())
        {
            StartRocksTrap();
            QueuedRocks--;
        }
    }

#if DEBUG
    public static bool CanWarp(string destination)
    {
        return Player.PlayerDataLocal != null;
    }
#else
    public static bool CanWarp(string destination)
    {
        if (Player.PlayerDataLocal == null || !Plugin.State.Valid)
        {
            return false;
        }

        if (destination == "Last Checkpoint")
        {
            return Player.PlayerDataLocal.lastCheckpointData != null && Player.PlayerDataLocal.lastCheckpointData.checkpointRoomID != Player.PlayerDataLocal.currentRoomID;
        }

        if (destination == "Entrance")
        {
            return Player.PlayerDataLocal.currentRoomID != 5;
        }

        if (Data.Checkpoints.TryGetValue(destination, out var checkpoint))
        {
            if (checkpoint.RoomId == Player.PlayerDataLocal.currentRoomID)
            {
                return false;
            }
            else if (Plugin.State.VisitedCampfires.Contains(checkpoint.Id))
            {
                return true;
            }
        }

        return false;
    }
#endif

    private static bool IsGameStateNormal()
    {
        return (
            !_cutscenePlaying &&
            !_isWarping &&
            Plugin.State.Valid &&
            GameplayUIManager.Instance != null &&
            !GameplayUIManager.Instance.InGameMenuOpen &&
            !GameplayUIManager.Instance.FullMapOpen &&
            !(GameplayUIManager.Instance.elevatorMenuHolder?.activeInHierarchy ?? false) &&
            Player.Instance != null &&
            Player.Instance.PlayerPhysics != null &&
            Player.Instance.PlayerPhysics.isIdle &&
            Player.Instance.gameObject.active &&
            !Player.Instance.isInElevator &&
            Player.Instance.allowRoomTransition &&
            !Player.Instance.isDead &&
            !Player.Instance.meteorInProgress &&
            !(Player.Instance.currentSubWeaponClass?.isShooting ?? false) &&
            !(Player.Instance.currentSubWeaponClass?.isAttacking ?? false) &&
            Player.Instance.ControllerEnabled()
        );
    }

    private static void CheckWarp()
    {
        if (WarpDestination == null)
        {
            return;
        }

        var destination = WarpDestination;
        WarpDestination = null;

        if (!IsGameStateNormal())
        {
            return;
        }

        Vector2 targetDestination;
        Room targetRoom;
        int checkpointId;

        if (destination == "Last Checkpoint")
        {
            targetRoom = GameManager.GetRoomFromID(Player.PlayerDataLocal.lastCheckpointData.checkpointRoomID);
            targetDestination = new(Player.PlayerDataLocal.lastCheckpointX, Player.PlayerDataLocal.lastCheckpointY);
            checkpointId = Player.PlayerDataLocal.lastCheckpointData.checkpointID;
        }
        else if (Data.Checkpoints.TryGetValue(destination, out var checkpoint))
        {
            if (CanWarp(destination))
            {
                targetRoom = GameManager.GetRoomFromID(checkpoint.RoomId);
                targetDestination = checkpoint.PlayerPos;
                checkpointId = checkpoint.Id;
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

        _isWarping = true;
        GameManager.Instance.StartCoroutine(Warp_Routine(targetDestination, targetRoom, checkpointId));
    }

    private static IEnumerator Warp_Routine(Vector2 targetDestination, Room targetRoom, int checkpointId)
    {
        var currentRoom = GameManager.GetRoomFromID(Player.PlayerDataLocal.currentRoomID);
        if (!Plugin.State.SlotData.AllowBlockWarping)
        {
            Player.Instance.liftableObject?.Object_Drop();
        }
        Player.Instance.SetOffPlatform();
        Player.Instance.SetIsInLadder(false, null);
        Player.Instance.HidePlayer();
        CameraManager.Flash(NESPalette.White, 0.02f, 0f, false);
        var lightning = PoolManager.Pools["Particles"].Spawn("Lightning");
        lightning.position = new(
            Player.Instance.selfTransform.position.x,
            Player.Instance.selfTransform.position.y + lightning.GetComponent<BoxCollider2D>().size.y / 2f,
            Player.Instance.selfTransform.position.z
        );
        AudioManager.Play("wall-gem");
        yield return new WaitForSeconds(0.2f);

        targetRoom.ActivateInisde();
        targetRoom.ActivateRoomSpecificOptions();
        currentRoom.StopRocksFalling();
        currentRoom.PreDeactivateInisde();
        currentRoom.DeactivateInisde();
        CameraManager.MoveCameraTo(targetRoom);
        yield return new WaitForSeconds(0.5f);

        var emptyAutoKill = PoolManager.Pools["Particles"].Spawn("EmptyAutoKill");
        emptyAutoKill.position = targetDestination;
        GameManager.ReparentToWorld(emptyAutoKill);
        var component = emptyAutoKill.GetComponent<KillSelfAfter>();
        component.clipName = "Teleport_Charge2";
        component.Activate();
        yield return new WaitForSeconds(1f);

        Player.Instance.AllowRoomTransition(false);
        Player.Instance.selfTransform.position = targetDestination;
        CameraManager.Flash(NESPalette.White, 0.02f, 0f, false);
        lightning = PoolManager.Pools["Particles"].Spawn("Lightning");
        lightning.position = new(
            Player.Instance.selfTransform.position.x,
            Player.Instance.selfTransform.position.y + lightning.GetComponent<BoxCollider2D>().size.y / 2f,
            Player.Instance.selfTransform.position.z
        );
        AudioManager.Play("wall-gem");
        Player.Instance.ResetMaterial();
        Player.Instance.ShowPlayer(true, true);
        targetRoom.PlayerEntered();
        yield return new WaitForSeconds(0.1f);

        Player.Instance.AllowRoomTransition(true);
        _isWarping = false;
        foreach (var actor in targetRoom.GetActorsWithID(checkpointId))
        {
            var checkpoint = actor.GetComponent<SavePoint>();
            checkpoint?.SaveGame();
        }
        yield break;
    }

    public static void ToggleInfiniteJumps()
    {
        var enabled = !Settings.InfiniteJumps;

        if (Player.Instance?.PlayerPhysics != null)
        {
            if (enabled)
            {
                Player.Instance.PlayerPhysics.infiniteJump = true;
            }
            else
            {
                Player.Instance.PlayerPhysics.infiniteJump = Player.Instance.CharacterProperty.infiniteJump;
            }
        }

        Settings.InfiniteJumps = enabled;
    }

    public static void PlayCutsceneTrap()
    {
        if (!IsGameStateNormal())
        {
            return;
        }

        _cutscenePlaying = true;
        var rand = new System.Random();
        var cutscene = Data.FakeCutscenes[rand.Next(0, Data.FakeCutscenes.Length)];
        GameManager.Instance.StartCoroutine(CutsceneTrap_Routine(cutscene));
    }

    private static IEnumerator CutsceneTrap_Routine(DialogueLine[] cutscene)
    {
        GameplayUIManager.HideNotification();
        Player.Instance.DisableController(true);
        Player.Instance.SetPhysicsActive(false);
        Player.Instance.godMode = true;
        // Player.Instance.HidePlayerSprite();
        // Player.Instance.Room.DeactivateEnemies();
        foreach (var (line, pos) in cutscene)
        {
            var lines = new Dialogue[] { new(line) };
            yield return GameManager.Instance.StartCoroutine(GameplayUIManager.Instance.TriggerDialogueSequence_Routine(lines, null, true, true, pos));
        }
        // Player.Instance.ShowPlayerSprite();
        Player.Instance.godMode = false;
        Player.Instance.SetPhysicsActive(true);
        Player.Instance.EnableController(true, true);
        // Player.Instance.Room.ActivateEnemies();
        _cutscenePlaying = false;
        yield break;
    }

    public static bool CanEnableRocks()
    {
        return (
            IsGameStateNormal() &&
            Player.Instance.Room != null &&
            !Player.Instance.Room.isRocks &&
            !Player.Instance.Room.savePoint &&
            !Player.Instance.Room.voidPortal &&
            !Player.Instance.Room.titanStatue &&
            Player.Instance.Room.roomType != "elevator" &&
            Player.Instance.Room.roomType != "boss" &&
            Player.Instance.Room.roomID != 5
        );
    }

    public static void StartRocksTrap()
    {
        if (!CanEnableRocks())
        {
            return;
        }

        Player.Instance.Room.isRocks = true;
        Room.previousRoomWasRocks = false;
        Player.Instance.Room.ActivatePostTransitionRoomOptions();
    }
}
