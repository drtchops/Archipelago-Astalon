using System.Collections.Generic;
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
        Player.PlayerDataLocal.unlockedCharacters ??= new();
        Player.PlayerDataLocal.purchasedDeals ??= new();

        if (ArchipelagoClient.ServerData.SlotData.SkipCutscenes)
        {
            Player.PlayerDataLocal.cs_bkbossfinal1 = true;
            Player.PlayerDataLocal.cs_bkbossintro1 = true;
            Player.PlayerDataLocal.cs_bkFinalToMedusa = true;
            Player.PlayerDataLocal.cs_finalPlatformRide = true;
            if (Player.PlayerDataLocal.epimetheusSequence == 0)
            {
                Player.PlayerDataLocal.epimetheusSequence = 1;
            }

            if (!Player.PlayerDataLocal.firstElevatorLit)
            {
                Player.PlayerDataLocal.firstElevatorLit = true;
                if (!Player.PlayerDataLocal.elevatorsFound.Contains(6629))
                {
                    Player.PlayerDataLocal.elevatorsFound.Add(6629);
                }

                // blocks around first elevator
                var room = GameManager.GetRoomFromID(6629);
                for (var i = 6641; i < 6647; i++)
                {
                    SaveManager.CurrentSave.SetObjectData(i, "_objectOnFalseobjectOn__linkID189linkID_", 6629);
                }

                room.UpdateObjectState(SaveManager.CurrentSave);
            }
        }

        if (ArchipelagoClient.ServerData.SlotData.StartWithZeek)
        {
            if (!Player.PlayerDataLocal.unlockedCharacters.Contains(CharacterProperties.Character.Zeek))
            {
                Player.PlayerDataLocal.unlockedCharacters.Add(CharacterProperties.Character.Zeek);
            }

            var deal = GameManager.Instance.itemManager.GetDealProperties(DealProperties.DealID.Deal_SubMenu_Zeek);
            deal.availableOnStart = true;
        }

        if (ArchipelagoClient.ServerData.SlotData.StartWithBram)
        {
            Player.PlayerDataLocal.bramFreed = true;
            Player.PlayerDataLocal.bramSeen = true;
            if (!Player.PlayerDataLocal.unlockedCharacters.Contains(CharacterProperties.Character.Bram))
            {
                Player.PlayerDataLocal.unlockedCharacters.Add(CharacterProperties.Character.Bram);
            }

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
        // if (GameplayUIManager.Instance == null)
        // {
        //     Main.Log.LogWarning("Cannot get item: GameplayUIManager == null");
        //     return false;
        // }
        // if (GameplayUIManager.Instance.isOnMainMenu)
        // {
        //     Main.Log.LogWarning("Cannot get item: on main menu");
        //     return false;
        // }
        // if (GameLoader.Instance.gameIsLoading)
        // {
        //     Main.Log.LogWarning("Cannot get item: loading");
        //     return false;
        // }

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
        // if (GameplayUIManager.Instance.isOnMainMenu)
        // {
        //     Main.Log.LogWarning("Cannot get item: on main menu");
        //     return false;
        // }
        // if (GameLoader.Instance.gameIsLoading)
        // {
        //     Main.Log.LogWarning("Cannot get item: loading");
        //     return false;
        // }
        // if (GameplayUIManager.Instance.dialogueRunning)
        // {
        //     Main.Log.LogWarning("Cannot display message: dialogue running");
        //     return false;
        // }
        //if (!ItemBoxDisplayed)
        //{
        //    Main.Log.LogWarning("Cannot display message: item box not yet displayed");
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
        if (icon.IsNullOrWhiteSpace())
        {
            icon = "Item_AmuletOfSol";
        }

        var sound = "pickup";
        if (itemInfo.Flags == ItemFlags.Advancement)
        {
            sound = "secret";
        }
        else if (itemInfo.Flags == ItemFlags.Trap)
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
            if (Player.PlayerDataLocal.collectedStrengths.Contains((int)itemInfo.LocationId))
            {
                return false;
            }

            Player.PlayerDataLocal.strengthBonusShared += 1;
            Player.PlayerDataLocal.collectedStrengths.Add((int)itemInfo.LocationId);
        }
        else if (itemName.StartsWith("Max HP"))
        {
            if (Player.PlayerDataLocal.collectedHearts.Contains((int)itemInfo.LocationId))
            {
                return false;
            }

            var bonus = 0;
            switch (itemName)
            {
                case "Max HP +1":
                    bonus = 1;
                    break;
                case "Max HP +2":
                    bonus = 2;
                    break;
                case "Max HP +3":
                    bonus = 3;
                    break;
                case "Max HP +4":
                    bonus = 4;
                    break;
                case "Max HP +5":
                    bonus = 5;
                    break;
            }

            Player.PlayerDataLocal.healthItemBonus += bonus;
            Player.PlayerDataLocal.currentHealth += bonus;
            // TODO: replace this with a proper int based on slot data
            Player.PlayerDataLocal.collectedHearts.Add((int)itemInfo.LocationId);
            GameplayUIManager.Instance?.UpdateHealthBar(Player.Instance, true);
        }
        else if (itemName.EndsWith("Orbs"))
        {
            // TODO: check if already received

            var amount = 0;
            switch (itemName)
            {
                case "50 Orbs":
                    amount = 50;
                    break;
                case "100 Orbs":
                    amount = 100;
                    break;
                case "200 Orbs":
                    amount = 200;
                    break;
            }

            Player.Instance.CollectOrbs(amount);
        }
        else if (Data.ItemMap.TryGetValue(itemName, out var itemId))
        {
            if (Player.PlayerDataLocal.collectedItems.Contains(itemId))
            {
                return false;
            }

            Player.PlayerDataLocal.CollectItem(itemId);
            if (itemId == ItemProperties.ItemID.AscendantKey)
            {
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
            }
        }
        else if (itemName.EndsWith("Key"))
        {
            switch (itemName)
            {
                // TODO: check if already received
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
        else if (Data.RedDoorMap.TryGetValue(itemName, out (int roomID, int objectID) ids))
        {
            if (SaveManager.CurrentSave.GetObjectData(ids.objectID) == "_wasOpenedTruewasOpened_")
            {
                return false;
            }

            var room = GameManager.GetRoomFromID(ids.roomID);
            SaveManager.CurrentSave.SetObjectData(ids.objectID, "_wasOpenedTruewasOpened_", ids.roomID);
            room.UpdateObjectState(SaveManager.CurrentSave);
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

        if (ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys &&
            Data.RedKeyMap.TryGetValue(entityId, out var redKeyLocation))
        {
            Plugin.ArchipelagoClient.SendLocation(redKeyLocation);
            return true;
        }

        return false;
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
            var display = GiveItem(item);
            if (display)
            {
                IncomingMessages.Enqueue(item);
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
    }
}