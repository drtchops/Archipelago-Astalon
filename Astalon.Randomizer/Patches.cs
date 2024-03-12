using Astalon.Randomizer.Archipelago;
using BepInEx;
using HarmonyLib;
using I2.Loc;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Astalon.Randomizer;

[HarmonyPatch(typeof(Item), nameof(Item.Collect))]
internal class Item_Collect_Patch
{
    public static void Prefix(Item __instance)
    {
        Plugin.Logger.LogDebug($"Item.Collect({__instance}, {__instance.actorID})");
        if (Data.LocationMap.ContainsKey(__instance.itemProperties.itemID))
        {
            __instance.useItemBox = false;
            __instance.collectedSound = null;
            __instance.collectedText = null;
        }
    }
}

[HarmonyPatch(typeof(Item_PlayerHeart), nameof(Item_PlayerHeart.Collect))]
internal class Item_PlayerHeart_Collect_Patch
{
    public static void Prefix(Item_PlayerHeart __instance)
    {
        Plugin.Logger.LogDebug(
            $"Item_PlayerHeart.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
        if (ArchipelagoClient.ServerData.SlotData.RandomizeHealthPickups &&
            Data.HealthMap.ContainsKey(__instance.actorID))
        {
            __instance.heartGain = 0;
            __instance.useItemBox = false;
            __instance.collectedSound = null;
            __instance.collectedText = null;
        }
    }
}

[HarmonyPatch(typeof(Item_PlayerStrength), nameof(Item_PlayerStrength.Collect))]
internal class Item_PlayerStrength_Collect_Patch
{
    public static void Prefix(Item_PlayerStrength __instance)
    {
        Plugin.Logger.LogDebug(
            $"Item_PlayerStrength.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
        if (ArchipelagoClient.ServerData.SlotData.RandomizeAttackPickups &&
            Data.AttackMap.ContainsKey(__instance.actorID))
        {
            __instance.strengthGain = 0;
            __instance.useItemBox = false;
            __instance.collectedSound = null;
            __instance.collectedText = null;
        }
    }
}

[HarmonyPatch(typeof(Key), nameof(Key.Collect))]
internal class Key_Collect_Patch
{
    public static void Prefix(Key __instance)
    {
        Plugin.Logger.LogDebug($"Key.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
        if (__instance.keyType == Key.KeyType.Red && ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys &&
            Data.RedKeyMap.ContainsKey(__instance.actorID))
        {
            __instance.useItemBox = false;
            __instance.collectedSound = null;
            __instance.collectedText = null;
        }
    }
}

[HarmonyPatch(typeof(PlayerData))]
internal class PlayerData_Patch
{
    [HarmonyPatch(nameof(PlayerData.CollectItem), typeof(ItemProperties.ItemID))]
    [HarmonyPrefix]
    public static void CollectItemID(ItemProperties.ItemID itemID)
    {
        Plugin.Logger.LogDebug($"PlayerData.CollectItemID({itemID})");
    }

    [HarmonyPatch(nameof(PlayerData.CollectItem), typeof(ItemProperties))]
    [HarmonyPrefix]
    public static bool CollectItem(ItemProperties itemProp)
    {
        Plugin.Logger.LogDebug($"PlayerData.CollectItemProps({itemProp.itemID})");
        return !Game.CollectItem(itemProp.itemID);
    }

    [HarmonyPatch(nameof(PlayerData.CollectHeart))]
    [HarmonyPrefix]
    public static bool CollectHeart(int _id)
    {
        Plugin.Logger.LogDebug($"PlayerData.CollectHeart({_id})");
        return !Game.CollectEntity(_id);
    }

    [HarmonyPatch(nameof(PlayerData.CollectKey))]
    [HarmonyPrefix]
    public static bool CollectKey(int _id)
    {
        Plugin.Logger.LogDebug($"PlayerData.CollectKey({_id})");
        return !Game.CollectEntity(_id);
    }

    [HarmonyPatch(nameof(PlayerData.CollectStrength))]
    [HarmonyPrefix]
    public static bool CollectStrength(int _id)
    {
        Plugin.Logger.LogDebug($"PlayerData.CollectStrength({_id})");
        return !Game.CollectEntity(_id);
    }

    [HarmonyPatch(nameof(PlayerData.AddKey))]
    [HarmonyPrefix]
    public static bool AddKey(Key.KeyType keyType)
    {
        Plugin.Logger.LogDebug($"PlayerData.AddKey({keyType})");

        switch (keyType)
        {
            case Key.KeyType.White when ArchipelagoClient.ServerData.SlotData.RandomizeWhiteKeys:
            case Key.KeyType.Blue when ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys:
            case Key.KeyType.Red when ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys:
                return false;
            default:
                return true;
        }
    }

    [HarmonyPatch(nameof(PlayerData.AddKeys))]
    [HarmonyPrefix]
    public static void AddKeys(Key.KeyType keyType, int amount)
    {
        Plugin.Logger.LogDebug($"PlayerData.AddKeys({keyType}, {amount})");
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
        return !Settings.FreePurchases;
    }

    [HarmonyPatch(nameof(PlayerData.UnlockElevator))]
    [HarmonyPostfix]
    public static void UnlockElevatorPostfix()
    {
        Plugin.Logger.LogDebug("PlayerData.UnlockElevatorPostfix()");
        Game.RemoveFreeElevator();
    }
}

[HarmonyPatch(typeof(Player))]
internal class Player_Patch
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
        Plugin.Logger.LogDebug("Player.Activate()");
        Game.CanInitializeSave = true;
        Game.InitializeSave();
    }

    [HarmonyPatch(nameof(Player.Deactivate))]
    [HarmonyPrefix]
    public static void Deactivate()
    {
        Plugin.Logger.LogDebug("Player.Deactivate()");
    }

    [HarmonyPatch(nameof(Player.DeathSequence), typeof(bool), typeof(bool))]
    [HarmonyPrefix]
    public static void DeathSequence()
    {
        Plugin.Logger.LogDebug("Player.DeathSequence()");
        Game.HandleDeath();
    }

    [HarmonyPatch(nameof(Player.AssignRoom))]
    [HarmonyPrefix]
    public static void AssignRoom(Room _room)
    {
        Plugin.Logger.LogDebug($"Player.AssignRoom({_room.roomID})");
    }
}

[HarmonyPatch(typeof(EnemyEntity), nameof(EnemyEntity.Damage))]
internal class EnemyEntity_Damage_Patch
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
internal class Boss_Worm_DamageBody_Patch
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
internal class GameplayUIManager_Patch
{
    [HarmonyPatch(nameof(GameplayUIManager.DisplayItemBox))]
    [HarmonyPrefix]
    public static bool DisplayItemBox(string _itemIcon, ref string _itemText)
    {
        Plugin.Logger.LogDebug($"GameplayUIManager.DisplayItemBox({_itemIcon}, {_itemText})");
        if (_itemText.IsNullOrWhiteSpace())
        {
            return false;
        }

        if (_itemText.StartsWith("ARCHIPELAGO:"))
        {
            _itemText = _itemText[12..];
        }

        return true;
    }
}

[HarmonyPatch(typeof(Cutscene), nameof(Cutscene.PlayCutscene))]
internal class Cutscene_PlayCutscene_Patch
{
    public static void Prefix(Cutscene __instance)
    {
        Plugin.Logger.LogDebug($"Cutscene.PlayCutscene({__instance.cutsceneID})");
    }
}

[HarmonyPatch(typeof(CutsceneManager))]
internal class CutsceneManager_Patch
{
    [HarmonyPatch(nameof(CutsceneManager.PlayCutscene), typeof(string), typeof(Room), typeof(bool), typeof(Vector2))]
    [HarmonyPrefix]
    public static void PlayCutscene1(string ID)
    {
        Plugin.Logger.LogDebug($"CutsceneManager.PlayCutscene1({ID})");
    }

    [HarmonyPatch(nameof(CutsceneManager.PlayCutscene), typeof(string), typeof(Room), typeof(bool), typeof(int),
        typeof(int))]
    [HarmonyPostfix]
    public static void PlayCutscene2(string ID)
    {
        Plugin.Logger.LogDebug($"CutsceneManager.PlayCutscene2({ID})");
    }
}

[HarmonyPatch(typeof(CutsceneController))]
internal class CutsceneController_Patch
{
    [HarmonyPatch(nameof(CutsceneController.Play))]
    [HarmonyPrefix]
    public static void Play()
    {
        Plugin.Logger.LogDebug("CutsceneController.Play()");
    }

    [HarmonyPatch(nameof(CutsceneController.Stop))]
    [HarmonyPrefix]
    public static void Stop()
    {
        Plugin.Logger.LogDebug("CutsceneController.Stop()");
    }
}

[HarmonyPatch(typeof(CutsceneObject), nameof(CutsceneObject.LoadData))]
internal class CutsceneObject_LoadData_Patch
{
    public static void Postfix(CutsceneObject __instance)
    {
        Plugin.Logger.LogDebug($"CutsceneObject.LoadData({__instance.sceneID})");
    }
}

[HarmonyPatch(typeof(CS_Scene3), nameof(CS_Scene3.PlayScene))]
[HarmonyPatch(typeof(CS_Scene4), nameof(CS_Scene4.PlayScene))]
[HarmonyPatch(typeof(CS_Scene9), nameof(CS_Scene9.PlayScene))]
internal class CS_Scene_Patch
{
    public static void Prefix(object __instance)
    {
        Plugin.Logger.LogDebug($"{__instance.GetType()}.PlayScene()");
    }
}

[HarmonyPatch(typeof(CreditsManager), nameof(CreditsManager.Open))]
internal class CreditsManager_Open_Patch
{
    public static void Prefix()
    {
        Plugin.Logger.LogDebug($"CreditsManager.Open()");
    }
}

[HarmonyPatch(typeof(LocalizationManager), nameof(LocalizationManager.TryGetTranslation))]
internal class LocalizationManager_TryGetTranslation_Patch
{
    public static void Postfix(string Term, ref string Translation, bool __result)
    {
        // Main.Log.LogDebug($"LocalizationManager.TryGetTranslation({Term})");
        if (Term.StartsWith("ARCHIPELAGO:"))
        {
            Translation = Term[12..];
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(GameLoader))]
internal class GameLoader_Patch
{
    [HarmonyPatch(nameof(GameLoader.LoadGame))]
    [HarmonyPostfix]
    public static void LoadGame(int slot)
    {
        Plugin.Logger.LogDebug($"GameLoader.LoadGame({slot})");
    }

    [HarmonyPatch(nameof(GameLoader.LoadNewGame))]
    [HarmonyPostfix]
    public static void LoadNewGame(int slot)
    {
        Plugin.Logger.LogDebug($"GameLoader.LoadNewGame({slot})");
    }

    [HarmonyPatch(nameof(GameLoader.RestartGame))]
    [HarmonyPostfix]
    public static void RestartGame()
    {
        Plugin.Logger.LogDebug($"GameLoader.RestartGame()");
        Game.CanInitializeSave = false;
        Game.ExitSave();
    }

    [HarmonyPatch(nameof(GameLoader.PlayIntro))]
    [HarmonyPostfix]
    public static void PlayIntro()
    {
        Plugin.Logger.LogDebug($"GameLoader.PlayIntro()");
    }

    [HarmonyPatch(nameof(GameLoader.LoadMainMenu))]
    [HarmonyPostfix]
    public static void LoadMainMenu()
    {
        Plugin.Logger.LogDebug("GameLoader.LoadMainMenu()");
    }
}

[HarmonyPatch(typeof(SaveManager))]
internal class SaveManager_Patch
{
    [HarmonyPatch(nameof(SaveManager.ApplyCurrentSave))]
    [HarmonyPrefix]
    public static void ApplyCurrentSave(bool showIcon)
    {
        Plugin.Logger.LogDebug($"SaveManager.ApplyCurrentSave({showIcon})");
        Game.CanInitializeSave = true;
        Game.InitializeSave();

        //foreach (var data in SaveManager.CurrentSave.objectsData)
        //{
        //    if (data.RoomID == 248)
        //    {
        //        Main.Log.LogDebug($"Id={data.Id} Room={data.RoomID} Data='{data.Data}'");
        //    }
        //}
    }

    [HarmonyPatch(nameof(SaveManager.InitializeFirstSave))]
    [HarmonyPostfix]
    public static void InitializeFirstSave()
    {
        Plugin.Logger.LogDebug("SaveManager.InitializeFirstSave()");
    }
}

[HarmonyPatch(typeof(Boss_BlackKnightFinal))]
internal class Boss_BlackKnightFinal_Patch
{
    [HarmonyPatch(nameof(Boss_BlackKnightFinal.MedusaDied))]
    [HarmonyPrefix]
    public static void MedusaDied()
    {
        Plugin.Logger.LogDebug("Boss_BlackKnightFinal.MedusaDied()");
        Plugin.ArchipelagoClient.SendCompletion();
    }
}

[HarmonyPatch(typeof(AstalonDebug))]
internal class Debug_Patch
{
    [HarmonyPatch(nameof(AstalonDebug.Update))]
    public static void Prefix()
    {
        Game.Update();
    }
}