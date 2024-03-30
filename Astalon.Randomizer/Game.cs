using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Archipelago.MultiClient.Net.Enums;
using Astalon.Randomizer.Archipelago;
using Il2CppSystem;
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

    public static Queue<ItemInfo> IncomingItems { get; } = new();
    public static Queue<ItemInfo> IncomingMessages { get; } = new();
    public static string DeathSource { get; private set; }
    public static bool CanInitializeSave { get; set; }
    public static bool UnlockElevators { get; set; }
    public static bool TriggerDeath { get; set; }
    public static bool DumpRoom { get; set; }
    public static bool ReceivingItem { get; set; }
    public static string WarpDestination { get; set; }

    private static readonly string DataDir = Path.GetFullPath("BepInEx/data/Archipelago");
    private static int _deathCounter = -1;
    private static bool _saveInitialized;
    private static tk2dBaseSprite _baseSprite;
    private static tk2dSpriteCollectionData _spriteCollectionData;
    private static tk2dSpriteAnimationClip _spriteAnimationClip;
    private static bool _injectedAnimation;
    private static int _warpCooldown;

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

    public static void UpdateItem(Item item)
    {
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
        string location = null;

        if (ArchipelagoClient.ServerData.SlotData.RandomizeHealthPickups &&
            Data.HealthMap.TryGetValue(actorId, out var healthLocation))
        {
            location = healthLocation;
        }

        if (ArchipelagoClient.ServerData.SlotData.RandomizeAttackPickups &&
            Data.AttackMap.TryGetValue(actorId, out var attackLocation))
        {
            location = attackLocation;
        }

        if (ArchipelagoClient.ServerData.SlotData.RandomizeWhiteKeys &&
            Data.WhiteKeyMap.TryGetValue(actorId, out var whiteLocation))
        {
            location = whiteLocation;
        }

        if (ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys &&
            Data.BlueKeyMap.TryGetValue(actorId, out var blueLocation))
        {
            location = blueLocation;
        }

        if (actorId == 0 &&
            Data.SpawnedKeyMap.TryGetValue(Player.PlayerDataLocal.currentRoomID, out var spawnedLocation))
        {
            if ((spawnedLocation.Contains("White Key") && ArchipelagoClient.ServerData.SlotData.RandomizeWhiteKeys) ||
                (spawnedLocation.Contains("Blue Key") && ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys) ||
                (spawnedLocation.Contains("Red Key") && ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys))
            {
                location = spawnedLocation;
            }
        }

        if (ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys &&
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
        var animationName = icon switch
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

        var animator = gameObject.GetComponent<tk2dSpriteAnimator>();
        if (animator != null)
        {
            var clipId = animator.GetClipIdByName(animationName);
            animator.defaultClipId = clipId;
            animator.Play(animationName);
        }

        var sprite = gameObject.GetComponent<tk2dBaseSprite>();
        if (sprite != null)
        {
            sprite.SetSprite(icon);
        }
    }

    public static void InitializeSave()
    {
        if (_saveInitialized || !CanInitializeSave || !ArchipelagoClient.Connected)
        {
            return;
        }

        Plugin.Logger.LogDebug("Initializing Save");

        Player.PlayerDataLocal.elevatorsFound ??= new();
        Player.PlayerDataLocal.purchasedDeals ??= new();

        if (ArchipelagoClient.ServerData.SlotData.SkipCutscenes)
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

        if (ArchipelagoClient.ServerData.SlotData.StartWithZeek)
        {
            Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Zeek);
            var deal = GameManager.Instance.itemManager.GetDealProperties(DealProperties.DealID.Deal_SubMenu_Zeek);
            deal.availableOnStart = true;
        }

        if (ArchipelagoClient.ServerData.SlotData.StartWithBram)
        {
            Player.PlayerDataLocal.UnlockCharacter(CharacterProperties.Character.Bram);
            // TODO: check if I need to do these
            Player.PlayerDataLocal.bramFreed = true;
            Player.PlayerDataLocal.bramSeen = true;
            var deal = GameManager.Instance.itemManager.GetDealProperties(DealProperties.DealID.Deal_SubMenu_Bram);
            deal.availableOnStart = true;
        }

        if (ArchipelagoClient.ServerData.SlotData.StartWithQoL)
        {
            var deals = new[]
            {
                DealProperties.DealID.Deal_Knowledge,
                DealProperties.DealID.Deal_OrbReaper,
                DealProperties.DealID.Deal_TitanEgo,
                DealProperties.DealID.Deal_MapReveal,
                DealProperties.DealID.Deal_Gift,
                DealProperties.DealID.Deal_LockedDoors,
            };
            foreach (var deal in deals)
            {
                if (!Player.PlayerDataLocal.purchasedDeals.Contains(deal))
                {
                    Player.PlayerDataLocal.purchasedDeals.Add(deal);
                }
            }

            Player.PlayerDataLocal.CollectItem(ItemProperties.ItemID.MarkOfEpimetheus);
        }

        if (ArchipelagoClient.ServerData.SlotData.CostMultiplier != 100 && GameManager.Instance.itemManager
                .GetDealProperties(DealProperties.DealID.Deal_Gift).DealPrice == 666)
        {
            var mul = ArchipelagoClient.ServerData.SlotData.CostMultiplier / 100f;
            foreach (var deal in GameManager.Instance.itemManager.gameDeals)
            {
                deal.dealPrice = (int)Math.Round(deal.dealPrice * mul);
            }
        }

        if (ArchipelagoClient.ServerData.SlotData.FastBloodChalice)
        {
            Player.Instance.regenInterval = 0.2f;
        }

        var validated = ArchipelagoClient.ServerData.ValidateSave();
        if (!validated)
        {
            Plugin.ArchipelagoClient.Disconnect();
        }

        Plugin.ArchipelagoClient.SyncLocations();
        _saveInitialized = true;
    }

    public static void ExitSave()
    {
        _saveInitialized = false;
    }

    public static bool CanGetItem()
    {
        if (!_saveInitialized)
        {
            return false;
        }

        if (GameManager.Instance?.player?.playerData == null)
        {
            //Main.Log.LogWarning("Cannot get item: PlayerData == null");
            return false;
        }

        if (Player.Instance == null || !Player.Instance.playerDataLoaded)
        {
            //Main.Log.LogWarning("Cannot get item: PlayerData is not loaded");
            return false;
        }

        //if (GameLoader.Instance.gameIsLoading)
        //{
        //    //Main.Log.LogWarning("Cannot get item: loading");
        //    return false;
        //}

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
            //Main.Log.LogWarning("Cannot display message: GameplayUIManager == null");
            return false;
        }

        if (GameplayUIManager.Instance.itemBox == null || GameplayUIManager.Instance.itemBox.active)
        {
            //Main.Log.LogWarning("Cannot display message: itemBox null or displayed");
            return false;
        }

        //if (GameLoader.Instance.gameIsLoading)
        //{
        //    //Main.Log.LogWarning("Cannot get item: loading");
        //    return false;
        //}

        //if (GameplayUIManager.Instance.dialogueRunning)
        //{
        //    //Main.Log.LogWarning("Cannot display message: dialogue running");
        //    return false;
        //}

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
                    if (ArchipelagoClient.ServerData.SlotData.FreeApexElevator)
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
        else
        {
            Plugin.Logger.LogWarning($"Item {itemInfo.Id} - {itemName} not found");
            return false;
        }

        return true;
    }

    public static void RemoveFreeElevator()
    {
        if (ArchipelagoClient.Connected && !ArchipelagoClient.ServerData.SlotData.FreeApexElevator &&
            Player.PlayerDataLocal.elevatorsFound.Contains(4109) &&
            !Player.PlayerDataLocal.discoveredRooms.Contains(4109))
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

    public static bool CollectItem(ItemProperties.ItemID itemId)
    {
        if (Data.LocationMap.TryGetValue(itemId, out var location))
        {
            Plugin.ArchipelagoClient.SendLocation(location);
            return true;
        }

        return false;
    }

    public static bool CollectEntity(int entityId)
    {
        if (ArchipelagoClient.ServerData.SlotData.RandomizeHealthPickups &&
            Data.HealthMap.TryGetValue(entityId, out var healthLocation))
        {
            Plugin.ArchipelagoClient.SendLocation(healthLocation);
            return true;
        }

        if (ArchipelagoClient.ServerData.SlotData.RandomizeAttackPickups &&
            Data.AttackMap.TryGetValue(entityId, out var attackLocation))
        {
            Plugin.ArchipelagoClient.SendLocation(attackLocation);
            return true;
        }

        if (ArchipelagoClient.ServerData.SlotData.RandomizeWhiteKeys &&
            Data.WhiteKeyMap.TryGetValue(entityId, out var whiteKeyLocation))
        {
            Plugin.ArchipelagoClient.SendLocation(whiteKeyLocation);
            return true;
        }

        if (ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys &&
            Data.BlueKeyMap.TryGetValue(entityId, out var blueKeyLocation))
        {
            Plugin.ArchipelagoClient.SendLocation(blueKeyLocation);
            return true;
        }

        if (entityId == 0 &&
            Data.SpawnedKeyMap.TryGetValue(Player.PlayerDataLocal.currentRoomID, out var spawnedKeyLocation))
        {
            if ((spawnedKeyLocation.Contains("White Key") &&
                 ArchipelagoClient.ServerData.SlotData.RandomizeWhiteKeys) ||
                (spawnedKeyLocation.Contains("Blue Key") && ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys) ||
                (spawnedKeyLocation.Contains("Red Key") && ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys))
            {
                Plugin.ArchipelagoClient.SendLocation(spawnedKeyLocation);
                return true;
            }
        }

        if (ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys &&
            Data.RedKeyMap.TryGetValue(entityId, out var redKeyLocation))
        {
            Plugin.ArchipelagoClient.SendLocation(redKeyLocation);
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
            if (item.Index < ArchipelagoClient.ServerData.ItemIndex)
            {
                Plugin.Logger.LogDebug($"Ignoring previously obtained item {item.Id}");
            }
            else
            {
                ArchipelagoClient.ServerData.ItemIndex++;
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

            Plugin.Logger.LogDebug($"Data for room={Player.PlayerDataLocal.currentRoomID}");
            foreach (var data in SaveManager.CurrentSave.objectsData)
            {
                if (data.RoomID == Player.PlayerDataLocal.currentRoomID)
                {
                    Plugin.Logger.LogDebug($"Id={data.ID} Data='{data.Data}'");
                }
            }

            var cp = Player.PlayerDataLocal.lastCheckpointData;
            Plugin.Logger.LogDebug(
                $"last cp id={cp.checkpointID} room={cp.checkpointRoomID} pos=({Player.PlayerDataLocal.lastCheckpointX}, {Player.PlayerDataLocal.lastCheckpointY})");
            var room = GameManager.GetRoomFromID(cp.checkpointRoomID);
            if (room)
            {
                Plugin.Logger.LogDebug($"room pos={room.roomInitialPosition}");
            }
        }

        if (_warpCooldown > 0)
        {
            _warpCooldown--;
        }

        CheckWarp();
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
        var skipCheck = false;
#if DEBUG
        skipCheck = true;
#endif

        if (destination == "Last Checkpoint")
        {
            var room = GameManager.GetRoomFromID(Player.PlayerDataLocal.lastCheckpointData.checkpointRoomID);
            playerPos = new(Player.PlayerDataLocal.lastCheckpointX, Player.PlayerDataLocal.lastCheckpointY, 0);
            cameraPos = room.roomInitialPosition;
        }
        else if (Data.Checkpoints.TryGetValue(destination, out var checkpoint))
        {
            if (checkpoint.RoomId == Player.PlayerDataLocal.currentRoomID)
            {
                return;
            }
            else if (skipCheck || Player.PlayerDataLocal.discoveredRooms.Contains(checkpoint.RoomId) &&
                     (checkpoint.Id != 2669 || GetObjectValue(4338, "wasActivated").ToLower() == "true"))
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

        Plugin.Logger.LogDebug($"Warping to: {destination}");
        Player.Instance.transform.position = playerPos;
        CameraManager.MoveCameraTo(cameraPos);
        AudioManager.Play("thunder");
        AudioManager.Play("wall-gem");
        _warpCooldown = 60;
    }
}