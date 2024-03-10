using HarmonyLib;
using I2.Loc;
using Archipelago.MultiClient.Net.Enums;
using BepInEx.Unity.IL2CPP.UnityEngine;
using System.Collections.Generic;
using Il2CppSystem;

namespace Archipelago;

public static class Game
{
    public class ItemBox
    {
        public string Message { get; set; }
        public string Icon { get; set; }
        public string Sound { get; set; }
        public float Duration { get; set; } = 2.5f;
        public bool DisableController { get; set; } = false;
    }

    public static Queue<ItemInfo> IncomingItems = new();
    public static Queue<ItemInfo> IncomingMessages = new();
    public static string DeathSource;
    public static int DeathCounter = -1;
    public static bool ShouldToggleDeathLink = true;

    [HarmonyPatch(typeof(Item), nameof(Item.Collect))]
    class Item_Collect_Patch
    {
        public static void Prefix(Item __instance)
        {
            Main.Log.LogDebug($"Item.Collect({__instance}, {__instance.actorID})");
            if (Data.LocationMap.ContainsKey(__instance.itemProperties.itemID))
            {
                __instance.useItemBox = false;
                __instance.collectedSound = null;
                __instance.collectedText = null;
            }
        }
    }

    [HarmonyPatch(typeof(Item_PlayerHeart), nameof(Item_PlayerHeart.Collect))]
    class Item_PlayerHeart_Collect_Patch
    {
        public static void Prefix(Item_PlayerHeart __instance)
        {
            Main.Log.LogDebug($"Item_PlayerHeart.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
            if (Settings.RandomizeHealthPickups && Data.HPMap.ContainsKey(__instance.actorID))
            {
                __instance.heartGain = 0;
                __instance.useItemBox = false;
                __instance.collectedSound = null;
                __instance.collectedText = null;
            }
        }
    }

    [HarmonyPatch(typeof(Item_PlayerStrength), nameof(Item_PlayerStrength.Collect))]
    class Item_PlayerStrength_Collect_Patch
    {
        public static void Prefix(Item_PlayerStrength __instance)
        {
            Main.Log.LogDebug($"Item_PlayerStrength.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
            if (Settings.RandomizeAttackPickups && Data.AttackMap.ContainsKey(__instance.actorID))
            {
                __instance.strengthGain = 0;
                __instance.useItemBox = false;
                __instance.collectedSound = null;
                __instance.collectedText = null;
            }
        }
    }

    [HarmonyPatch(typeof(Key), nameof(Key.Collect))]
    class Key_Collect_Patch
    {
        public static void Prefix(Key __instance)
        {
            Main.Log.LogDebug($"Key.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
            if (__instance.keyType == Key.KeyType.Red && Settings.RandomizeRedKeys && Data.RedKeyMap.ContainsKey(__instance.actorID))
            {
                __instance.useItemBox = false;
                __instance.collectedSound = null;
                __instance.collectedText = null;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerData))]
    class PlayerData_Patch
    {
        [HarmonyPatch(nameof(PlayerData.CollectItem), typeof(ItemProperties.ItemID))]
        [HarmonyPrefix]
        public static void CollectItemID(ItemProperties.ItemID itemID)
        {
            Main.Log.LogDebug($"PlayerData.CollectItemID({itemID})");
        }

        [HarmonyPatch(nameof(PlayerData.CollectItem), typeof(ItemProperties))]
        [HarmonyPrefix]
        public static bool CollectItem(ItemProperties itemProp, PlayerData __instance)
        {
            Main.Log.LogDebug($"PlayerData.CollectItemProps({itemProp.itemID})");
            if (Data.LocationMap.TryGetValue(itemProp.itemID, out var location))
            {
                Main.APManager.SendLocation(location);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(PlayerData.CollectHeart))]
        [HarmonyPrefix]
        public static bool CollectHeart(int _id)
        {
            Main.Log.LogDebug($"PlayerData.CollectHeart({_id})");
            if (Settings.RandomizeHealthPickups && Data.HPMap.TryGetValue(_id, out var location))
            {
                Main.APManager.SendLocation(location);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(PlayerData.CollectKey))]
        [HarmonyPrefix]
        public static bool CollectKey(int _id)
        {
            Main.Log.LogDebug($"PlayerData.CollectKey({_id})");
            if (Settings.RandomizeRedKeys && Data.RedKeyMap.TryGetValue(_id, out var location))
            {
                Main.APManager.SendLocation(location);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(PlayerData.CollectStrength))]
        [HarmonyPrefix]
        public static bool CollectStrength(int _id)
        {
            Main.Log.LogDebug($"PlayerData.CollectStrength({_id})");
            if (Settings.RandomizeAttackPickups && Data.AttackMap.TryGetValue(_id, out var location))
            {
                Main.APManager.SendLocation(location);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(PlayerData.AddKey))]
        [HarmonyPrefix]
        public static bool AddKey(Key.KeyType keyType)
        {
            Main.Log.LogDebug($"PlayerData.AddKey({keyType})");

            if (keyType == Key.KeyType.White && Settings.RandomizeWhiteKeys)
            {
                return false;
            }
            if (keyType == Key.KeyType.Blue && Settings.RandomizeBlueKeys)
            {
                return false;
            }
            if (keyType == Key.KeyType.Red && Settings.RandomizeRedKeys)
            {
                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(PlayerData.AddKeys))]
        [HarmonyPrefix]
        public static bool AddKeys(Key.KeyType keyType, int amount)
        {
            Main.Log.LogDebug($"PlayerData.AddKeys({keyType}, {amount})");
            return true;
        }

        [HarmonyPatch(nameof(PlayerData.UseKey))]
        [HarmonyPostfix]
        public static void UseKey(Key.KeyType keyType)
        {
            if (!Settings.FreeKeys)
            {
                return;
            }

            switch (keyType)
            {
                case Key.KeyType.White:
                    Player.PlayerDataLocal.whiteKeys += 1;
                    break;
                case Key.KeyType.Blue:
                    Player.PlayerDataLocal.blueKeys += 1;
                    break;
                case Key.KeyType.Red:
                    Player.PlayerDataLocal.redKeys += 1;
                    break;
            }
        }

        [HarmonyPatch(nameof(PlayerData.RemoveOrbs))]
        [HarmonyPrefix]
        public static bool RemoveOrbs()
        {
            if (Settings.FreePurchases)
            {
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(PlayerData.UnlockElevator))]
        [HarmonyPostfix]
        public static void UnlockElevatorPostfix()
        {
            Main.Log.LogDebug("PlayerData.UnlockElevatorPostfix()");
            if (!Settings.FreeApexElevator && Player.PlayerDataLocal.elevatorsFound.Contains(4109) && !Player.PlayerDataLocal.discoveredRooms.Contains(4109))
            {
                Player.PlayerDataLocal.elevatorsFound.Remove(4109);
            }
        }
    }

    [HarmonyPatch(typeof(Player))]
    class Player_Patch
    {
        [HarmonyPatch(nameof(Player.Damage))]
        [HarmonyPrefix]
        public static bool Damage()
        {
            return !Settings.Invincibility;
        }

        [HarmonyPatch(nameof(Player.Activate))]
        [HarmonyPrefix]
        public static void Activate()
        {
            Main.Log.LogDebug("Player.Activate()");
            Main.APManager.Connect();
        }

        [HarmonyPatch(nameof(Player.Deactivate))]
        [HarmonyPrefix]
        public static void Deactivate()
        {
            Main.Log.LogDebug("Player.Deactivate()");
        }

        [HarmonyPatch(nameof(Player.DeathSequence), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        public static void DeathSequence()
        {
            Main.Log.LogDebug("Player.DeathSequence()");
            if (DeathCounter <= 0)
            {
                Main.APManager.SendDeath();
            }
        }

        [HarmonyPatch(nameof(Player.AssignRoom))]
        [HarmonyPrefix]
        public static void AssignRoom(Room _room)
        {
            Main.Log.LogDebug($"Player.AssignRoom({_room.roomID})");
        }
    }

    [HarmonyPatch(typeof(EnemyEntity), nameof(EnemyEntity.Damage))]
    class EnemyEntity_Damage_Patch
    {
        public static void Prefix(ref int damageAmount)
        {
            if (Settings.MaxDamage)
            {
                damageAmount = 999;
            }
        }
    }

    [HarmonyPatch(typeof(Boss_Worm))]
    class Boss_Worm_DamageBody_Patch
    {
        [HarmonyPatch(nameof(Boss_Worm.DamageBody))]
        [HarmonyPrefix]
        public static void DamageBody(ref int damageAmount)
        {
            if (Settings.MaxDamage)
            {
                damageAmount = 999;
            }
        }

        [HarmonyPatch(nameof(Boss_Worm.DamageEffectBody))]
        [HarmonyPrefix]
        public static void DamageEffectBody(ref int damageAmount)
        {
            if (Settings.MaxDamage)
            {
                damageAmount = 999;
            }
        }

        [HarmonyPatch(nameof(Boss_Worm.DamageEntity))]
        [HarmonyPrefix]
        public static void DamageEntity(ref int damageAmount)
        {
            if (Settings.MaxDamage)
            {
                damageAmount = 999;
            }
        }

        [HarmonyPatch(nameof(Boss_Worm.DamageTail))]
        [HarmonyPrefix]
        public static void DamageTail(ref int damageAmount)
        {
            if (Settings.MaxDamage)
            {
                damageAmount = 999;
            }
        }
    }

    [HarmonyPatch(typeof(GameplayUIManager))]
    class GameplayUIManager_Patch
    {
        [HarmonyPatch(nameof(GameplayUIManager.DisplayItemBox))]
        [HarmonyPrefix]
        public static bool DisplayItemBox(string _itemIcon, ref string _itemText, ref bool disableController)
        {
            Main.Log.LogDebug($"GameplayUIManager.DisplayItemBox({_itemIcon}, {_itemText})");
            if (_itemText == null)
            {
                return false;
            }
            if (_itemText.StartsWith("ARCHIPELAGO:"))
            {
                _itemText = _itemText.Substring(12);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Cutscene), nameof(Cutscene.PlayCutscene))]
    class Cutscene_PlayCutscene_Patch
    {
        public static void Prefix(Cutscene __instance)
        {
            Main.Log.LogDebug($"Cutscene.PlayCutscene({__instance.cutsceneID})");
        }
    }

    [HarmonyPatch(typeof(CutsceneManager))]
    class CutsceneManager_Patch
    {
        [HarmonyPatch(nameof(CutsceneManager.PlayCutscene), typeof(string), typeof(Room), typeof(bool), typeof(UnityEngine.Vector2))]
        [HarmonyPrefix]
        public static void PlayCutscene1(string ID)
        {
            Main.Log.LogDebug($"CutsceneManager.PlayCutscene1({ID})");
        }

        [HarmonyPatch(nameof(CutsceneManager.PlayCutscene), typeof(string), typeof(Room), typeof(bool), typeof(int), typeof(int))]
        [HarmonyPostfix]
        public static void PlayCutscene2(string ID)
        {
            Main.Log.LogDebug($"CutsceneManager.PlayCutscene2({ID})");
        }
    }

    [HarmonyPatch(typeof(CutsceneController))]
    class CutsceneController_Patch
    {
        [HarmonyPatch(nameof(CutsceneController.Play))]
        [HarmonyPrefix]
        public static void Play()
        {
            Main.Log.LogDebug("CutsceneController.Play()");
        }

        [HarmonyPatch(nameof(CutsceneController.Stop))]
        [HarmonyPrefix]
        public static void Stop()
        {
            Main.Log.LogDebug("CutsceneController.Stop()");
        }
    }

    [HarmonyPatch(typeof(CutsceneObject), nameof(CutsceneObject.LoadData))]
    class CutsceneObject_LoadData_Patch
    {
        public static void Postfix(CutsceneObject __instance)
        {
            Main.Log.LogDebug($"CutsceneObject.LoadData({__instance.sceneID})");
        }
    }

    [HarmonyPatch(typeof(CS_Scene3), nameof(CS_Scene3.PlayScene))]
    [HarmonyPatch(typeof(CS_Scene4), nameof(CS_Scene4.PlayScene))]
    [HarmonyPatch(typeof(CS_Scene9), nameof(CS_Scene9.PlayScene))]
    class CS_Scene_Patch
    {
        public static void Prefix()
        {
            Main.Log.LogDebug("CS_Scene3.PlayScene");
        }
    }

    [HarmonyPatch(typeof(CreditsManager), nameof(CreditsManager.Open))]
    class CreditsManager_Open_Patch
    {
        public static void Prefix()
        {
            Main.Log.LogDebug($"CreditsManager.Open()");
        }
    }

    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.TryGetTranslation))]
    class LocalizationManager_TryGetTranslation_Patch
    {
        public static void Postfix(string Term, ref string Translation, bool __result)
        {
            // Main.Log.LogDebug($"LocalizationManager.TryGetTranslation({Term})");
            if (Term.StartsWith("ARCHIPELAGO:"))
            {
                Translation = Term.Substring(12);
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(GameLoader))]
    class GameLoader_Patch
    {
        [HarmonyPatch(nameof(GameLoader.LoadGame))]
        [HarmonyPostfix]
        public static void LoadGame(int slot)
        {
            Main.Log.LogDebug($"GameLoader.LoadGame({slot})");
        }

        [HarmonyPatch(nameof(GameLoader.LoadNewGame))]
        [HarmonyPostfix]
        public static void LoadNewGame(int slot)
        {
            Main.Log.LogDebug($"GameLoader.LoadNewGame({slot})");
        }

        [HarmonyPatch(nameof(GameLoader.LoadMainMenu))]
        [HarmonyPostfix]
        public static void LoadMainMenu()
        {
            Main.Log.LogDebug("GameLoader.LoadMainMenu()");
            Main.APManager.Disconnect();
        }
    }

    [HarmonyPatch(typeof(SaveManager))]
    class SaveManager_Patch
    {
        [HarmonyPatch(nameof(SaveManager.ApplyCurrentSave))]
        [HarmonyPrefix]
        public static void ApplyCurrentSave(bool showIcon)
        {
            Main.Log.LogDebug($"SaveManager.UpdateSave({showIcon})");
            //foreach (var data in SaveManager.CurrentSave.objectsData)
            //{
            //    if (data.RoomID == 248)
            //    {
            //        Main.Log.LogInfo($"ID={data.ID} Room={data.RoomID} Data='{data.Data}'");
            //    }
            //}
        }

        [HarmonyPatch(nameof(SaveManager.InitializeFirstSave))]
        [HarmonyPostfix]
        public static void InitializeFirstSave()
        {
            Main.Log.LogDebug("SaveManager.InitializeFirstSave");
        }
    }

    [HarmonyPatch(typeof(Boss_BlackKnightFinal))]
    class Boss_BlackKnightFinal_Patch
    {
        [HarmonyPatch(nameof(Boss_BlackKnightFinal.MedusaDied))]
        [HarmonyPrefix]
        public static void MedusaDied()
        {
            Main.Log.LogDebug("Boss_BlackKnightFinal.MedusaDied()");
            Main.APManager.SendCompletion();
        }
    }

    [HarmonyPatch(typeof(AstalonDebug))]
    class Debug_Patch
    {
        [HarmonyPatch(nameof(AstalonDebug.Update))]
        public static void Prefix()
        {
            var holdingMods = Input.GetKeyInt(KeyCode.LeftControl) && Input.GetKeyInt(KeyCode.LeftShift);

            if (DeathSource != null && DeathCounter == -1 && CanBeKilled())
            {
                DeathCounter = 60;
                IncomingMessages.Enqueue(new ItemInfo
                {
                    Name = "Death",
                    PlayerName = DeathSource,
                    Receiving = true,
                    Flags = ItemFlags.Trap,
                });
                Player.Instance.Kill(false, false);
            }

            if (DeathCounter > 0)
            {
                DeathCounter--;
            }
            if (DeathCounter == 0)
            {
                DeathCounter = -1;
                DeathSource = null;
            }

            if (GameManager.Instance?.player != null && holdingMods && Input.GetKeyInt(KeyCode.K))
            {
                GameManager.Instance.player.Kill();
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

            if (holdingMods && Input.GetKeyInt(KeyCode.L))
            {
                if (ShouldToggleDeathLink)
                {
                    Main.APManager.ToggleDeathLink();
                    ShouldToggleDeathLink = false;
                }
            }
            else
            {
                ShouldToggleDeathLink = true;
            }

            if (holdingMods && Input.GetKeyInt(KeyCode.V))
            {
                Player.PlayerDataLocal?.UnlockAllElevators();
            }
        }
    }

    public static void InitializeSave()
    {
        Main.Log.LogDebug("Initializing Save");

        if (Settings.SkipCutscenes)
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
                if (Player.PlayerDataLocal.elevatorsFound == null)
                {
                    Player.PlayerDataLocal.elevatorsFound = new Il2CppSystem.Collections.Generic.List<int>();
                }
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

        if (Player.PlayerDataLocal.unlockedCharacters == null)
        {
            Player.PlayerDataLocal.unlockedCharacters = new Il2CppSystem.Collections.Generic.List<CharacterProperties.Character>();
        }

        if (Settings.StartWithZeek)
        {
            if (!Player.PlayerDataLocal.unlockedCharacters.Contains(CharacterProperties.Character.Zeek))
            {
                Player.PlayerDataLocal.unlockedCharacters.Add(CharacterProperties.Character.Zeek);
            }

            var deal = GameManager.Instance.itemManager.GetDealProperties(DealProperties.DealID.Deal_SubMenu_Zeek);
            deal.availableOnStart = true;
        }

        if (Settings.StartWithBram)
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

        if (Settings.StartWithQOL)
        {
            if (Player.PlayerDataLocal.purchasedDeals == null)
            {
                Player.PlayerDataLocal.purchasedDeals = new Il2CppSystem.Collections.Generic.List<DealProperties.DealID>();
            }
            var deals = new DealProperties.DealID[]
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

        if (Settings.CostMultiplier != 100 && GameManager.Instance.itemManager.GetDealProperties(DealProperties.DealID.Deal_Gift).DealPrice == 666)
        {
            var mul = Settings.CostMultiplier / 100f;
            foreach (var deal in GameManager.Instance.itemManager.gameDeals)
            {
                deal.dealPrice = (int)Math.Round(deal.dealPrice * mul);
            }
        }
    }

    public static bool CanGetItem()
    {
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
            if (playerName == "" || playerName == null)
            {
                playerName = "Server";
            }
            if (itemInfo.Receiving)
            {
                message = $"{message} from {playerName}";
            }
            else
            {
                message = $"{playerName}'s {message}";
            }
        }

        Data.IconMap.TryGetValue(itemInfo.Name, out var icon);
        if (icon == "" || icon == null)
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

        return new ItemBox
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
        GameplayUIManager.Instance.DisplayItemBox(itemBox.Icon, itemBox.Message, itemBox.Duration, itemBox.DisableController);
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
        Main.Log.LogDebug($"Giving item: {itemName}");

        if (itemName == "Attack +1")
        {
            if (Player.PlayerDataLocal.collectedStrengths.Contains((int)itemInfo.LocationID))
            {
                return false;
            }

            Player.PlayerDataLocal.strengthBonusShared += 1;
            Player.PlayerDataLocal.collectedStrengths.Add((int)itemInfo.LocationID);
        }
        else if (itemName.StartsWith("Max HP"))
        {
            if (Player.PlayerDataLocal.collectedHearts.Contains((int)itemInfo.LocationID))
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
            Player.PlayerDataLocal.collectedHearts.Add((int)itemInfo.LocationID);
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
        else if (Data.ItemMap.TryGetValue(itemName, out var itemID))
        {
            if (Player.PlayerDataLocal.collectedItems.Contains(itemID))
            {
                return false;
            }

            Player.PlayerDataLocal.CollectItem(itemID);
            if (itemID == ItemProperties.ItemID.AscendantKey)
            {
                Player.PlayerDataLocal.elevatorsOpened = true;
            }
            foreach (var roomID in Player.PlayerDataLocal.elevatorsFound)
            {
                Player.PlayerDataLocal.UnlockElevator(roomID);
            }
            Player.PlayerDataLocal.UnlockElevator(6629);
            if (Settings.FreeApexElevator)
            {
                Player.PlayerDataLocal.UnlockElevator(4109);
            }
        }
        else if (itemName.EndsWith("Key"))
        {
            // TODO: check if already received
            if (itemName == "White Key")
            {
                Player.PlayerDataLocal.AddKey(Key.KeyType.White);
            }
            else if (itemName == "Blue Key")
            {
                Player.PlayerDataLocal.AddKey(Key.KeyType.Blue);
            }
            else if (itemName == "Red Key")
            {
                Player.PlayerDataLocal.AddKey(Key.KeyType.Red);
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
            Main.Log.LogWarning($"Item {itemInfo.ID} - {itemName} not found");
            return false;
        }

        return true;
    }
}
