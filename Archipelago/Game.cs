using HarmonyLib;
using I2.Loc;
using UnityEngine;
using Archipelago.MultiClient.Net.Enums;

namespace Archipelago;

public class Game
{
    public static bool ItemBoxDisplayed = false;

    public class ItemBox
    {
        public string Message { get; set; }
        public string Icon { get; set; }
        public string Sound { get; set; }
        public float Duration { get; set; } = 0.5f;
        public bool DisableController { get; set; } = false;
    }

    [HarmonyPatch(typeof(Item), nameof(Item.Collect))]
    class Item_Collect_Patch
    {
        public static void Prefix(Item __instance)
        {
            Main.Log.LogInfo($"Item.Collect({__instance}, {__instance.actorID})");
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
            Main.Log.LogInfo($"Item_PlayerHeart.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
            if (Data.HPMap.TryGetValue(__instance.actorID, out var location))
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

    [HarmonyPatch(typeof(PlayerData))]
    class PlayerData_Patch
    {
        [HarmonyPatch(nameof(PlayerData.CollectItem), typeof(ItemProperties))]
        [HarmonyPrefix]
        public static bool CollectItem(ItemProperties itemProp, PlayerData __instance)
        {
            Main.Log.LogInfo($"PlayerData.CollectItem({itemProp.itemID})");
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
            Main.Log.LogInfo($"PlayerData.CollectHeart({_id})");
            if (!Main.APManager.ReceivingItem && Data.HPMap.TryGetValue(_id, out var location))
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
            Main.Log.LogInfo($"PlayerData.CollectKey({_id})");
        }

        [HarmonyPatch(nameof(PlayerData.CollectStrength))]
        [HarmonyPrefix]
        public static bool CollectStrength(int _id)
        {
            Main.Log.LogInfo($"PlayerData.CollectStrength({_id})");
            if (!Main.APManager.ReceivingItem && Data.AttackMap.TryGetValue(_id, out var location))
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
            if (!Main.Settings.freeKeys)
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
            if (Main.Settings.freePurchases)
            {
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Player))]
    class Player_Patch
    {
        [HarmonyPatch(nameof(Player.Damage))]
        [HarmonyPrefix]
        public static bool Damage()
        {
            return !Main.Settings.invincibility;
        }

        [HarmonyPatch(nameof(Player.Activate))]
        [HarmonyPrefix]
        public static void Activate()
        {
            Main.Log.LogInfo("Player.Activate()");
            Main.APManager.Connect();
            // Player.PlayerDataLocal.firstElevatorLit = true;
            // Player.PlayerDataLocal.deaths = 1;
            // Player.PlayerDataLocal.cs_ending1 = true;
        }

        [HarmonyPatch(nameof(Player.Deactivate))]
        [HarmonyPrefix]
        public static void Deactivate()
        {
            Main.Log.LogInfo("Player.Deactivate()");
        }

        // [HarmonyPatch(nameof(Player.GetMaxHealth))]
        // [HarmonyPrefix]
        // public static void GetMaxHealth()
        // {
        //     Main.Log.LogInfo("Player.GetMaxHealth()");
        // }
    }

    [HarmonyPatch(typeof(EnemyEntity), nameof(EnemyEntity.Damage))]
    class EnemyEntity_EnemyEntity_Patch
    {
        public static void Prefix(ref int damageAmount)
        {
            if (Main.Settings.maxDamage)
            {
                damageAmount = 999;
            }
        }
    }

    // [HarmonyPatch(typeof(Collectable), "Collect")]
    // class Collectable_Collect_Patch
    // {
    //     public static void Prefix(Collectable __instance)
    //     {
    //         Main.Log.LogInfo($"Collectable.Collect({__instance})");
    //         Main.Log.LogInfo(__instance is null);
    //         Main.Log.LogInfo(__instance.GetType());
    //         Main.Log.LogInfo(__instance.collectedText);
    //         Main.Log.LogInfo(__instance.collectedText is null);
    //         Main.Log.LogInfo(__instance.collectedText.GetType());
    //         if (__instance is Key)
    //         {
    //             __instance.collectedText = "Cool key";
    //         }
    //         else
    //         {
    //             __instance.collectedText = "Cool item";
    //         }
    //     }
    // }

    [HarmonyPatch(typeof(GameplayUIManager))]
    class GameplayUIManager_Patch
    {
        [HarmonyPatch(nameof(GameplayUIManager.DisplayItemBox))]
        [HarmonyPrefix]
        public static void DisplayItemBox(string _itemIcon, ref string _itemText, ref bool disableController)
        {
            Main.Log.LogInfo($"GameplayUIManager.DisplayItemBox({_itemIcon}, {_itemText})");
            if (_itemText.StartsWith("ARCHIPELAGO:"))
            {
                _itemText = _itemText.Substring(12);
            }
            disableController = false;
            ItemBoxDisplayed = true;
        }

        // [HarmonyPatch(nameof(GameplayUIManager.DisplayDialogueFull))]
        // [HarmonyPrefix]
        // public static void DisplayDialogueFull(string _characterName, string _dialogue)
        // {
        //     Main.Log.LogInfo($"GameplayUIManager.DisplayDialogueFull({_characterName}, {_dialogue})");
        // }

        // [HarmonyPatch(nameof(GameplayUIManager.EnableMenu))]
        // [HarmonyPrefix]
        // public static void EnableMenu()
        // {
        //     Main.Log.LogInfo($"GameplayUIManager.EnableMenu()");
        // }

        // [HarmonyPatch(nameof(GameplayUIManager.DisableMenu))]
        // [HarmonyPrefix]
        // public static void DisableMenu()
        // {
        //     Main.Log.LogInfo($"GameplayUIManager.DisableMenu()");
        // }
    }

    [HarmonyPatch(typeof(Cutscene), nameof(Cutscene.PlayScene))]
    class Cutscene_PlayScene_Patch
    {
        public static void Prefix(Cutscene __instance)
        {
            Main.Log.LogInfo($"Cutscene.PlayScene({__instance.cutsceneID})");
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
            Main.Log.LogInfo("CS_Ending.PlayScene()");
            Main.APManager.SendCompletion();
        }
    }

    [HarmonyPatch(typeof(CutsceneManager), nameof(CutsceneManager.PlayCutscene), typeof(string), typeof(Room), typeof(bool), typeof(Vector2))]
    class CutsceneManager_PlayCutscene_Patch1
    {
        public static void Prefix(string ID)
        {
            Main.Log.LogInfo($"CutsceneManager.PlayCutscene1({ID})");
        }
    }

    [HarmonyPatch(typeof(CutsceneManager), nameof(CutsceneManager.PlayCutscene), typeof(string), typeof(Room), typeof(bool), typeof(int), typeof(int))]
    class CutsceneManager_PlayCutscene_Patch2
    {
        public static void Prefix(string ID)
        {
            Main.Log.LogInfo($"CutsceneManager.PlayCutscene2({ID})");
        }
    }

    [HarmonyPatch(typeof(CreditsManager), nameof(CreditsManager.Open))]
    class CreditsManager_Open_Patch
    {
        public static void Prefix()
        {
            Main.Log.LogInfo($"CreditsManager.Open()");
        }
    }

    [HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.TryGetTranslation))]
    class LocalizationManager_TryGetTranslation_Patch
    {
        public static void Postfix(string Term, ref string Translation, bool __result)
        {
            // Main.Log.LogInfo($"LocalizationManager.TryGetTranslation({Term})");
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
    //         Main.Log.LogInfo("LoadingScreen.Show()");
    //     }
    // }

    // [HarmonyPatch(typeof(LoadingScreen), nameof(LoadingScreen.Hide))]
    // class LoadingScreen_Hide_Patch
    // {
    //     public static void Prefix()
    //     {
    //         Main.Log.LogInfo("LoadingScreen.Hide()");
    //     }
    // }

    [HarmonyPatch(typeof(GameLoader))]
    class GameLoader_Patch
    {
        [HarmonyPatch(nameof(GameLoader.LoadGame))]
        [HarmonyPostfix]
        public static void LoadGame(int slot)
        {
            Main.Log.LogInfo($"GameLoader.LoadGame({slot})");
        }

        [HarmonyPatch(nameof(GameLoader.LoadNewGame))]
        [HarmonyPostfix]
        public static void LoadNewGame(int slot)
        {
            Main.Log.LogInfo($"GameLoader.LoadNewGame({slot})");
        }

        [HarmonyPatch(nameof(GameLoader.LoadMainMenu))]
        [HarmonyPostfix]
        public static void LoadMainMenu()
        {
            Main.Log.LogInfo("GameLoader.LoadMainMenu()");
            Main.APManager.Disconnect();
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
        var sound = "pickup";
        if (itemInfo.Flags == ItemFlags.Advancement)
        {
            sound = "secret";
        }
        else if (itemInfo.Flags == ItemFlags.Trap)
        {
            sound = "player-hit";
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
        // Main.Log.LogInfo($"dialogueRunning={GameplayUIManager.Instance.dialogueRunning}");
        // Main.Log.LogInfo($"isDialoguePlaying={GameplayUIManager.isDialoguePlaying}");
        // Main.Log.LogInfo($"menuEnabled={GameplayUIManager.menuEnabled}");
        // Main.Log.LogInfo($"isOnMainMenu={GameplayUIManager.Instance.isOnMainMenu}");
        // Main.Log.LogInfo($"mapInitialized={GameplayUIManager.Instance.mapInitialized}");
        if (!CanDisplayMessage())
        {
            return false;
        }

        // GameplayUIManager.Instance.InitializeGameplayUI();
        // GameplayUIManager.Instance.ToggleUIContainers(true);
        // Main.Log.LogInfo($"itemBox={GameplayUIManager.Instance.itemBox}");
        var itemBox = FormatItemBox(itemInfo);
        GameplayUIManager.Instance.DisplayItemBox(itemBox.Icon, itemBox.Message, itemBox.Duration, itemBox.DisableController);
        // GameplayUIManager.Instance.DisplayDialogueFull("Algus", message);
        // GameplayUIManager.Instance.ShowDialogueLine(new Dialogue(message), null);
        // var item = new Item
        // {
        //     collectedIcon = icon,
        //     collectedText = message,
        //     useItemBox = true,
        //     room = GameManager.GetRoomFromID(Player.PlayerDataLocal.currentRoomID),
        //     itemProperties = GameManager.Instance.itemManager.GetItemProperties(itemID),
        // };
        // foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(collect))
        // {
        //     string name = descriptor.Name;
        //     object value = descriptor.GetValue(collect);
        //     Main.Log.LogInfo($"{name}={value}");
        // }
        // item.Collect(GameManager.Instance.player.gameObject);
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
}
