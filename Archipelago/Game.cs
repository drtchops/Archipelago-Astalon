using HarmonyLib;
using I2.Loc;
using UnityEngine;
using Archipelago.MultiClient.Net.Enums;
using BepInEx.Unity.IL2CPP.UnityEngine;

namespace Archipelago;

public class Game
{
    public static bool ItemBoxDisplayed = false;

    public class ItemBox
    {
        public string Message { get; set; }
        public string Icon { get; set; }
        public string Sound { get; set; }
        public float Duration { get; set; } = 2.5f;
        public bool DisableController { get; set; } = false;
    }

    [HarmonyPatch(typeof(Item), nameof(Item.Collect))]
    class Item_Collect_Patch
    {
        public static void Prefix(Item __instance)
        {
            Main.Log.LogDebug($"Item.Collect({__instance}, {__instance.actorID})");
            if (Data.LocationMap.TryGetValue(__instance.itemProperties.itemID, out var location))
            {
                var itemInfo = Main.APManager.ScoutLocation(location);
                var itemBox = FormatItemBox(itemInfo);
                __instance.useItemBox = true;
                __instance.collectedIcon = itemBox.Icon;
                __instance.collectedText = "ARCHIPELAGO:" + itemBox.Message;
                __instance.collectedSound = itemBox.Sound;
            }
        }
    }

    [HarmonyPatch(typeof(Item_PlayerHeart), nameof(Item_PlayerHeart.Collect))]
    class Item_PlayerHeart_Collect_Patch
    {
        public static void Prefix(Item_PlayerHeart __instance)
        {
            Main.Log.LogDebug($"Item_PlayerHeart.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
            if (Main.Settings.RandomizeHealthPickups && Data.HPMap.TryGetValue(__instance.actorID, out var location))
            {
                var itemInfo = Main.APManager.ScoutLocation(location);
                var itemBox = FormatItemBox(itemInfo);
                __instance.heartGain = 0;
                __instance.useItemBox = true;
                __instance.collectedIcon = itemBox.Icon;
                __instance.collectedText = "ARCHIPELAGO:" + itemBox.Message;
                __instance.collectedSound = itemBox.Sound;
            }
        }
    }

    [HarmonyPatch(typeof(Item_PlayerStrength), nameof(Item_PlayerStrength.Collect))]
    class Item_PlayerStrength_Collect_Patch
    {
        public static void Prefix(Item_PlayerStrength __instance)
        {
            Main.Log.LogDebug($"Item_PlayerStrength.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
            if (Main.Settings.RandomizeAttackPickups && Data.AttackMap.TryGetValue(__instance.actorID, out var location))
            {
                var itemInfo = Main.APManager.ScoutLocation(location);
                var itemBox = FormatItemBox(itemInfo);
                __instance.strengthGain = 0;
                __instance.useItemBox = true;
                __instance.collectedIcon = itemBox.Icon;
                __instance.collectedText = "ARCHIPELAGO:" + itemBox.Message;
                __instance.collectedSound = itemBox.Sound;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerData))]
    class PlayerData_Patch
    {
        [HarmonyPatch(nameof(PlayerData.CollectItem), typeof(ItemProperties))]
        [HarmonyPrefix]
        public static bool CollectItem(ItemProperties itemProp, PlayerData __instance)
        {
            Main.Log.LogDebug($"PlayerData.CollectItem({itemProp.itemID})");
            if (!Main.APManager.ReceivingItem && Data.LocationMap.TryGetValue(itemProp.itemID, out var location))
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
            if (Main.Settings.RandomizeHealthPickups && !Main.APManager.ReceivingItem && Data.HPMap.TryGetValue(_id, out var location))
            {
                Main.APManager.SendLocation(location);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(PlayerData.CollectKey))]
        [HarmonyPrefix]
        public static void CollectKey(int _id)
        {
            Main.Log.LogDebug($"PlayerData.CollectKey({_id})");
        }

        [HarmonyPatch(nameof(PlayerData.CollectStrength))]
        [HarmonyPrefix]
        public static bool CollectStrength(int _id)
        {
            Main.Log.LogDebug($"PlayerData.CollectStrength({_id})");
            if (Main.Settings.RandomizeAttackPickups && !Main.APManager.ReceivingItem && Data.AttackMap.TryGetValue(_id, out var location))
            {
                Main.APManager.SendLocation(location);
                return false;
            }
            return true;
        }

        [HarmonyPatch(nameof(PlayerData.UseKey))]
        [HarmonyPostfix]
        public static void UseKey(Key.KeyType keyType)
        {
            if (!Main.Settings.FreeKeys)
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
            if (Main.Settings.FreePurchases)
            {
                return false;
            }
            return true;
        }

        // [HarmonyPatch(nameof(PlayerData.UnlockElevator))]
        // [HarmonyPrefix]
        // public static void UnlockElevatorPrefix()
        // {
        //     Main.Log.LogDebug("PlayerData.UnlockElevatorPrefix()");
        //     Main.Log.LogDebug(Player.PlayerDataLocal.elevatorsOpened);
        //     Main.Log.LogDebug(string.Join(", ", Player.PlayerDataLocal.elevatorsFound.ToArray()));
        // }

        // [HarmonyPatch(nameof(PlayerData.UnlockElevator))]
        // [HarmonyPostfix]
        // public static void UnlockElevatorPostfix()
        // {
        //     Main.Log.LogDebug("PlayerData.UnlockElevatorPostfix()");
        //     Main.Log.LogDebug(Player.PlayerDataLocal.elevatorsOpened);
        //     Main.Log.LogDebug(string.Join(", ", Player.PlayerDataLocal.elevatorsFound.ToArray()));
        // }
    }

    [HarmonyPatch(typeof(Player))]
    class Player_Patch
    {
        [HarmonyPatch(nameof(Player.Damage))]
        [HarmonyPrefix]
        public static bool Damage()
        {
            return !Main.Settings.Invincibility;
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
            Main.APManager.SendDeath();
        }
    }

    [HarmonyPatch(typeof(EnemyEntity), nameof(EnemyEntity.Damage))]
    class EnemyEntity_EnemyEntity_Patch
    {
        public static void Prefix(ref int damageAmount)
        {
            if (Main.Settings.MaxDamage)
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
        public static void DisplayItemBox(string _itemIcon, ref string _itemText, ref bool disableController)
        {
            Main.Log.LogDebug($"GameplayUIManager.DisplayItemBox({_itemIcon}, {_itemText})");
            if (_itemText.StartsWith("ARCHIPELAGO:"))
            {
                _itemText = _itemText.Substring(12);
            }
            disableController = true;
            ItemBoxDisplayed = true;
        }

        // [HarmonyPatch(nameof(GameplayUIManager.DisplayDialogueFull))]
        // [HarmonyPrefix]
        // public static void DisplayDialogueFull(string _characterName, string _dialogue)
        // {
        //     Main.Log.LogDebug($"GameplayUIManager.DisplayDialogueFull({_characterName}, {_dialogue})");
        // }

        // [HarmonyPatch(nameof(GameplayUIManager.EnableMenu))]
        // [HarmonyPrefix]
        // public static void EnableMenu()
        // {
        //     Main.Log.LogDebug($"GameplayUIManager.EnableMenu()");
        // }

        // [HarmonyPatch(nameof(GameplayUIManager.DisableMenu))]
        // [HarmonyPrefix]
        // public static void DisableMenu()
        // {
        //     Main.Log.LogDebug($"GameplayUIManager.DisableMenu()");
        // }
    }

    [HarmonyPatch(typeof(Cutscene), nameof(Cutscene.PlayCutscene))]
    class Cutscene_PlayCutscene_Patch
    {
        public static void Prefix(Cutscene __instance)
        {
            Main.Log.LogDebug($"Cutscene.PlayCutscene({__instance.cutsceneID})");
        }
    }

    [HarmonyPatch(typeof(CS_Ending2), nameof(CS_Ending2.PlayScene))]
    [HarmonyPatch(typeof(CS_EndingAlgus), nameof(CS_EndingAlgus.PlayScene))]
    [HarmonyPatch(typeof(CS_EndingBK), nameof(CS_EndingBK.PlayScene))]
    [HarmonyPatch(typeof(CS_EndingGargoyle), nameof(CS_EndingGargoyle.PlayScene))]
    class CS_Ending_PlayScene_Patch
    {
        public static void Prefix()
        {
            Main.Log.LogDebug("CS_Ending.PlayScene()");
            Main.APManager.SendCompletion();
        }
    }

    [HarmonyPatch(typeof(CutsceneManager))]
    class CutsceneManager_Patch
    {
        [HarmonyPatch(nameof(CutsceneManager.PlayCutscene), typeof(string), typeof(Room), typeof(bool), typeof(Vector2))]
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

    // [HarmonyPatch(typeof(LoadingScreen), nameof(LoadingScreen.Show))]
    // class LoadingScreen_Show_Patch
    // {
    //     public static void Prefix()
    //     {
    //         Main.Log.LogDebug("LoadingScreen.Show()");
    //     }
    // }

    // [HarmonyPatch(typeof(LoadingScreen), nameof(LoadingScreen.Hide))]
    // class LoadingScreen_Hide_Patch
    // {
    //     public static void Prefix()
    //     {
    //         Main.Log.LogDebug("LoadingScreen.Hide()");
    //     }
    // }

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
            if (GameManager.Instance?.player is not null &&
                Input.GetKeyInt(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.LeftControl) &&
                Input.GetKeyInt(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.LeftShift) &&
                Input.GetKeyInt(BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.K))
            {
                GameManager.Instance.player.Kill();
            }
        }
    }

    public static void InitializeSave()
    {
        Main.Log.LogDebug("Initializing Save");

        if (Main.Settings.SkipCutscenes)
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
                if (Player.PlayerDataLocal.elevatorsFound is null)
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

        if (Player.PlayerDataLocal.unlockedCharacters is null)
        {
            Player.PlayerDataLocal.unlockedCharacters = new Il2CppSystem.Collections.Generic.List<CharacterProperties.Character>();
        }

        if (Main.Settings.StartWithZeek)
        {
            if (!Player.PlayerDataLocal.unlockedCharacters.Contains(CharacterProperties.Character.Zeek))
            {
                Player.PlayerDataLocal.unlockedCharacters.Add(CharacterProperties.Character.Zeek);
            }

            var deal = GameManager.Instance.itemManager.GetDealProperties(DealProperties.DealID.Deal_SubMenu_Zeek);
            deal.availableOnStart = true;
        }

        if (Main.Settings.StartWithBram)
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

        if (Main.Settings.StartWithQOL)
        {
            if (Player.PlayerDataLocal.purchasedDeals is null)
            {
                Player.PlayerDataLocal.purchasedDeals = new Il2CppSystem.Collections.Generic.List<DealProperties.DealID>();
            }
            var deals = new DealProperties.DealID[]{
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
    }

    public static bool CanGetItem()
    {
        if (GameManager.Instance?.player?.playerData is null)
        {
            Main.Log.LogWarning("Cannot get item: PlayerData is null");
            return false;
        }
        if (!Player.Instance.playerDataLoaded)
        {
            Main.Log.LogWarning("Cannot get item: PlayerData is not loaded");
            return false;
        }
        // if (GameplayUIManager.Instance is null)
        // {
        //     Main.Log.LogWarning("Cannot get item: GameplayUIManager is null");
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
        if (GameplayUIManager.Instance is null)
        {
            Main.Log.LogWarning("Cannot display message: GameplayUIManager is null");
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
        if (!ItemBoxDisplayed)
        {
            Main.Log.LogWarning("Cannot display message: item box not yet displayed");
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
            if (playerName == "" || playerName is null)
            {
                playerName = "Server";
            }
            message = $"{playerName}'s {message}";
        }

        Data.IconMap.TryGetValue(itemInfo.Name, out var icon);
        if (icon == "" || icon is null)
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
        return true;
    }

    public static bool GiveItem(ItemInfo itemInfo, bool display = true)
    {
        if (!CanGetItem())
        {
            return false;
        }

        var itemName = itemInfo.Name;
        if (itemName == "Attack +1")
        {
            if (Player.PlayerDataLocal.collectedStrengths.Contains((int)itemInfo.LocationID))
            {
                Main.Log.LogWarning($"Attack location {itemInfo.LocationID} already collected");
                return true;
            }

            Player.PlayerDataLocal.strengthBonusShared += 1;
            Player.PlayerDataLocal.collectedStrengths.Add((int)itemInfo.LocationID);
        }
        else if (itemName.StartsWith("Max HP"))
        {
            if (Player.PlayerDataLocal.collectedHearts.Contains((int)itemInfo.LocationID))
            {
                Main.Log.LogWarning($"Max HP location {itemInfo.LocationID} already collected");
                return true;
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
            // GameplayUIManager.Instance.UpdateHealthBar(Player.Instance, true);
        }
        else if (itemName.EndsWith("Orbs"))
        {
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
            Player.PlayerDataLocal.currentOrbs += amount;
            Player.PlayerDataLocal.collectedOrbs += amount;
            // crashing?
            // Player.Instance.CollectOrbs(amount);
            // Player.PlayerDataLocal.AddOrbs(amount);
        }
        else if (Data.ItemMap.TryGetValue(itemName, out var itemID))
        {
            Player.PlayerDataLocal.CollectItem(itemID);
            // Data.IconMap.TryGetValue(itemName, out var icon);
            // DisplayItem(message, itemID, icon, local);
        }
        else
        {
            Main.Log.LogWarning($"Item {itemName} not found");
        }

        if (display)
        {
            DisplayItem(itemInfo);
        }
        return true;
    }

    public static void KillPlayer()
    {
        if (GameManager.Instance?.player is not null)
        {
            GameManager.Instance.player.Kill();
        }
    }
}
