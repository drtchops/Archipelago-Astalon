using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;

namespace Archipelago;

public static class Archipelago
{
    public static ManualLogSource Log = Logger.CreateLogSource("Archipelago");

    public static Dictionary<ItemProperties.ItemID, string> LocationMap = new Dictionary<ItemProperties.ItemID, string>(){
        {ItemProperties.ItemID.AmuletOfSol, "Hall of the Phantoms - Amulet of Sol"},
        {ItemProperties.ItemID.BanishSpell, "Gorgon Tomb - Banish Spell"},
        {ItemProperties.ItemID.GorgonHeart, "Gorgon Tomb - Gorgonheart"},
        {ItemProperties.ItemID.GriffonClaw, "Hall of the Phantoms - Griffon Claw"},
        {ItemProperties.ItemID.IcarusEmblem, "Ruins of Ash - Icarus Emblem"},
        {ItemProperties.ItemID.LunarianBow, "Catacombs - Lunarian Bow"},
        {ItemProperties.ItemID.RingOfTheAncients, "Gorgon Tomb - Ring of the Ancients"},
        {ItemProperties.ItemID.SwordOfMirrors, "Gorgon Tomb - Sword of Mirrors"},
        {ItemProperties.ItemID.GorgonEyeRed, "Gorgon Tomb - Gorgon Eye (Red)"},
        {ItemProperties.ItemID.GorgonEyeBlue, "Mechanism - Gorgon Eye (Blue)"},
        {ItemProperties.ItemID.GorgonEyeGreen, "Ruins of Ash - Gorgon Eye (Green)"},
        {ItemProperties.ItemID.DeadMaidensRing, "Hall of the Phantoms - Dead Maiden's Ring"},
        {ItemProperties.ItemID.LinusMap, "Gorgon Tomb - Linus' Map"},
        {ItemProperties.ItemID.AthenasBell, "Hall of the Phantoms - Athena's Bell"},
        {ItemProperties.ItemID.VoidCharm, "Gorgon Tomb - Void Charm"},
        {ItemProperties.ItemID.CloakOfLevitation, "Mechanism - Cloak of Levitation"},
        {ItemProperties.ItemID.AdornedKey, "Tower Roots - Adorned Key"},
        {ItemProperties.ItemID.PrincesCrown, "Cyclops Den - Prince's Crown"},
        {ItemProperties.ItemID.AscendantKey, "Gorgon Tomb - Ascendant Key"},
        {ItemProperties.ItemID.TalariaBoots, "Mechanism - Talaria Boots"},
        {ItemProperties.ItemID.MonsterBall, "Gorgon Tomb - Monster Ball"},
        {ItemProperties.ItemID.BloodChalice, "The Apex - Blood Chalice"},
        {ItemProperties.ItemID.MorningStar, "Serpent Path - Morning Star"},
        {ItemProperties.ItemID.CyclopsIdol, "Mechanism - Cyclops Idol"},
        {ItemProperties.ItemID.BoreasGauntlet, "Hall of the Phantoms - Boreas Gauntlet"},
        {ItemProperties.ItemID.FamiliarGil, "Catacombs - Gil"},
        {ItemProperties.ItemID.MagicBlock, "Cathedral - Magic Block"},
    };

    public static Dictionary<string, ItemProperties.ItemID> ItemMap = new Dictionary<string, ItemProperties.ItemID>(){
        {"Amulet of Sol", ItemProperties.ItemID.AmuletOfSol},
        {"Banish Spell", ItemProperties.ItemID.BanishSpell},
        {"Gorgonheart", ItemProperties.ItemID.GorgonHeart},
        {"Griffon Claw", ItemProperties.ItemID.GriffonClaw},
        {"Icarus Emblem", ItemProperties.ItemID.IcarusEmblem},
        {"Lunarian Bow", ItemProperties.ItemID.LunarianBow},
        {"Ring of the Ancients", ItemProperties.ItemID.RingOfTheAncients},
        {"Sword of Mirrors", ItemProperties.ItemID.SwordOfMirrors},
        {"Gorgon Eye (Red)", ItemProperties.ItemID.GorgonEyeRed},
        {"Gorgon Eye (Blue)", ItemProperties.ItemID.GorgonEyeBlue},
        {"Gorgon Eye (Green)", ItemProperties.ItemID.GorgonEyeGreen},
        {"Dead Maiden's Ring", ItemProperties.ItemID.DeadMaidensRing},
        {"Linus' Map", ItemProperties.ItemID.LinusMap},
        {"Athena's Bell", ItemProperties.ItemID.AthenasBell},
        {"Void Charm", ItemProperties.ItemID.VoidCharm},
        {"Cloak of Levitation", ItemProperties.ItemID.CloakOfLevitation},
        {"Adorned Key", ItemProperties.ItemID.AdornedKey},
        {"Prince's Crown", ItemProperties.ItemID.PrincesCrown},
        {"Ascendant Key", ItemProperties.ItemID.AscendantKey},
        {"Talaria Boots", ItemProperties.ItemID.TalariaBoots},
        {"Monster Ball", ItemProperties.ItemID.MonsterBall},
        {"Blood Chalice", ItemProperties.ItemID.BloodChalice},
        {"Morning Star", ItemProperties.ItemID.MorningStar},
        {"Cyclops Idol", ItemProperties.ItemID.CyclopsIdol},
        {"Boreas Gauntlet", ItemProperties.ItemID.BoreasGauntlet},
        {"Gil", ItemProperties.ItemID.FamiliarGil},
        {"Magic Block", ItemProperties.ItemID.MagicBlock},
    };

    public static GameManager gm;
    public static GameplayUIManager ui;

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.Awake))]
    class GameManager_Awake_Patch
    {
        public static void Prefix(GameManager __instance)
        {
            Log.LogInfo("GameManager.Awake()");
            gm = __instance;
        }
    }

    [HarmonyPatch(typeof(Item), nameof(Item.Collect))]
    class Item_Collect_Patch
    {
        public static void Prefix(Item __instance)
        {
            Log.LogInfo($"Item.Collect({__instance}, {__instance.collectedSound})");
            if (LocationMap.ContainsKey(__instance.itemProperties.itemID))
            {
                __instance.useItemBox = false;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.CollectItem), typeof(ItemProperties))]
    class PlayerData_CollectItem_Patch
    {
        public static bool Prefix(ItemProperties itemProp, PlayerData __instance)
        {
            Log.LogInfo($"PlayerData.CollectItem({itemProp.itemID})");
            Log.LogInfo($"check: {gm.player.playerData == __instance}");
            if (LocationMap.TryGetValue(itemProp.itemID, out var location))
            {
                APState.SendLocation(location);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.CollectHeart))]
    class PlayerData_CollectHeart_Patch
    {
        public static void Prefix(int _id, PlayerData __instance)
        {
            Log.LogInfo($"PlayerData.CollectHeart({_id})");
            Log.LogInfo($"check: {gm.player.playerData == __instance}");
        }
    }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.CollectKey))]
    class PlayerData_CollectKey_Patch
    {
        public static void Prefix(int _id, PlayerData __instance)
        {
            Log.LogInfo($"PlayerData.CollectKey({_id})");
            Log.LogInfo($"check: {gm.player.playerData == __instance}");

        }
    }

    // [HarmonyPatch(typeof(Key), nameof(Key.Collect))]
    // class Key_Collect_Patch
    // {
    //     public static void Prefix(Key __instance)
    //     {
    //         Log.LogInfo($"Key.Collect()");
    //         __instance.collectedText = $"ARCHIPELAGO_KEY_WHITE_{__instance.actorID}";
    //     }
    // }

    [HarmonyPatch(typeof(PlayerData), nameof(PlayerData.CollectStrength))]
    class PlayerData_CollectStrength_Patch
    {
        public static void Prefix(int _id, PlayerData __instance)
        {
            Log.LogInfo($"PlayerData.CollectStrength({_id})");
            Log.LogInfo($"check: {gm.player.playerData == __instance}");
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.Damage))]
    class Player_Damage_Patch
    {
        public static bool Prefix()
        {
            return !Plugin.config.invincibility;
        }
    }

    [HarmonyPatch(typeof(EnemyEntity), nameof(EnemyEntity.Damage))]
    class EnemyEntity_EnemyEntity_Patch
    {
        public static void Prefix(ref int damageAmount)
        {
            if (Plugin.config.maxDamage)
            {
                damageAmount = 99;
            }
        }
    }

    // [HarmonyPatch(typeof(Collectable), "Collect")]
    // class Collectable_Collect_Patch
    // {
    //     public static void Prefix(Collectable __instance)
    //     {
    //         Log.LogInfo($"Collectable.Collect({__instance})");
    //         Log.LogInfo(__instance is null);
    //         Log.LogInfo(__instance.GetType());
    //         Log.LogInfo(__instance.collectedText);
    //         Log.LogInfo(__instance.collectedText is null);
    //         Log.LogInfo(__instance.collectedText.GetType());
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

    [HarmonyPatch(typeof(GameplayUIManager), "DisplayItemBox")]
    class GameplayUIManager_DisplayItemBox_Patch
    {
        public static void Prefix(string _itemIcon, string _itemText)
        {
            Log.LogInfo($"GameplayUIManager.DisplayItemBox({_itemIcon}, {_itemText})");
        }
    }

    [HarmonyPatch(typeof(Cutscene), nameof(Cutscene.PlayScene))]
    class Cutscene_PlayScene_Patch
    {
        public static void Prefix(Cutscene __instance)
        {
            Log.LogInfo($"Cutscene.PlayScene({__instance.cutsceneID})");
        }
    }

    [HarmonyPatch(typeof(CS_Ending2), nameof(CS_Ending2.PlayScene))]
    class CS_Ending2_PlayScene_Patch
    {
        public static void Prefix(CS_Ending2 __instance)
        {
            Log.LogInfo($"CS_Ending2.PlayScene({__instance.cutsceneID})");
            APState.SendCompletion();
        }
    }

    [HarmonyPatch(typeof(I2.Loc.LocalizationManager), "TryGetTranslation")]
    class LocalizationManager_TryGetTranslation_Patch
    {
        public static void Postfix(string Term, ref string Translation, bool __result)
        {
            // Log.LogInfo($"LocalizationManager.TryGetTranslation({Term})");
            if (Term.StartsWith("ARCHIPELAGO_"))
            {
                Translation = "ChopsBlas's Humerus of McMittens, the Nurse";
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(GameplayUIManager), nameof(GameplayUIManager.Awake))]
    class GameplayUIManager_Awake_Patch
    {
        public static void Prefix(GameplayUIManager __instance)
        {
            ui = __instance;
        }
    }

    public static void DisplayItem(string message, bool local = false)
    {
        if (ui is null)
        {
            Log.LogWarning("GameplayUIManager is null");
            return;
        }

        ui.DisplayItemBox("", message, 2.5f, false);
    }

    public static void GiveItem(string itemName)
    {
        if (gm?.player?.playerData is null)
        {
            Log.LogWarning("PlayerData is null");
            return;
        }

        if (ItemMap.TryGetValue(itemName, out var itemID))
        {
            gm.player.playerData.CollectItem(itemID);
        }
        else
        {
            Log.LogWarning($"Item {itemName} not found");
        }
    }
}
