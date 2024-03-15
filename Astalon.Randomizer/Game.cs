using System.Collections.Generic;
using System.Text.RegularExpressions;
using Archipelago.MultiClient.Net.Enums;
using Astalon.Randomizer.Archipelago;
using BepInEx;
using Il2CppSystem;

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
    public static Queue<ItemInfo> IncomingItems { get; } = new();
    public static Queue<ItemInfo> IncomingMessages { get; } = new();
    public static string DeathSource { get; private set; }
    public static bool CanInitializeSave { get; set; }
    public static bool UnlockElevators { get; set; }
    public static bool TriggerDeath { get; set; }
    public static bool DumpRoom { get; set; }
    public static bool ReceivingItem { get; set; }
    private static int _deathCounter = -1;
    private static bool _saveInitialized;

    public static void InitializeSave()
    {
        if (_saveInitialized || !CanInitializeSave || !ArchipelagoClient.Connected)
        {
            return;
        }

        Plugin.Logger.LogDebug("Initializing Save");

        Player.PlayerDataLocal.collectedItems ??= new();
        Player.PlayerDataLocal.collectedStrengths ??= new();
        Player.PlayerDataLocal.collectedHearts ??= new();
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

    public static ItemBox FormatItemBox(ItemInfo itemInfo)
    {
        var message = itemInfo.Name;
        if (!itemInfo.IsLocal)
        {
            var playerName = itemInfo.PlayerName;
            if (playerName.IsNullOrWhiteSpace())
            {
                playerName = "Server";
            }

            message = itemInfo.Receiving ? $"{message} from {playerName}" : $"{playerName}'s {message}";
        }

        Data.IconMap.TryGetValue(itemInfo.Name, out var icon);

        if (itemInfo.Name.StartsWith("White Door"))
        {
            icon = "WhiteKey_1";
        }

        if (itemInfo.Name.StartsWith("Blue Door"))
        {
            icon = "BlueKey_1";
        }

        if (itemInfo.Name.StartsWith("Red Door"))
        {
            icon = "RedKey_1";
        }

        if (itemInfo.Name.StartsWith("Max HP"))
        {
            icon = "Item_HealthStone_1";
        }

        if (itemInfo.Name.EndsWith("Orbs"))
        {
            icon = "Deal_OrbReaper";
        }

        if (icon.IsNullOrWhiteSpace())
        {
            icon = "Item_AmuletOfSol";
        }

        var sound = itemInfo.Flags switch
        {
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
            Player.PlayerDataLocal.collectedStrengths.Add((int)itemInfo.LocationId);
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
                    Player.PlayerDataLocal.CollectItem(ItemProperties.ItemID.CyclopsIdol);
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
        if (!ArchipelagoClient.ServerData.SlotData.FreeApexElevator &&
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

        if (ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys && entityId == 0 &&
            Data.PotKeyMap.TryGetValue(Player.PlayerDataLocal.currentRoomID, out var potKeyLocation))
        {
            Plugin.ArchipelagoClient.SendLocation(potKeyLocation);
            return true;
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
        var data = SaveManager.CurrentSave.GetObjectData(ids.objectId);
        if (GetValue(data, "wasOpened").ToLower() == "true")
        {
            return false;
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
            Plugin.Logger.LogDebug($"Data for room={Player.PlayerDataLocal.currentRoomID}");
            foreach (var data in SaveManager.CurrentSave.objectsData)
            {
                if (data.RoomID == Player.PlayerDataLocal.currentRoomID)
                {
                    Plugin.Logger.LogDebug($"Id={data.ID} Data='{data.Data}'");
                }
            }

            DumpRoom = false;
        }
    }
}