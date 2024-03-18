using Astalon.Randomizer.Archipelago;
using HarmonyLib;
using I2.Loc;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Astalon.Randomizer;

[HarmonyPatch(typeof(Item))]
internal class Item_Patch
{
    [HarmonyPatch(nameof(Item.Collect))]
    [HarmonyPrefix]
    public static void Collect(Item __instance)
    {
        Plugin.Logger.LogDebug($"Item.Collect({__instance}, {__instance.actorID})");
        if (Data.LocationMap.ContainsKey(__instance.itemProperties.itemID))
        {
            __instance.useItemBox = false;
            __instance.collectedSound = null;
            __instance.collectedText = null;
        }
    }

    [HarmonyPatch(nameof(Item.Activate))]
    [HarmonyPostfix]
    public static void Activate(Item __instance)
    {
        Plugin.Logger.LogDebug($"Item.Activate({__instance}, {__instance.actorID})");
        Game.UpdateItem(__instance);
    }
}

[HarmonyPatch(typeof(Item_PlayerHeart))]
internal class Item_PlayerHeart_Patch
{
    [HarmonyPatch(nameof(Item_PlayerHeart.Collect))]
    [HarmonyPrefix]
    public static void Collect(Item_PlayerHeart __instance)
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

    [HarmonyPatch(nameof(Item_PlayerHeart.Activate))]
    [HarmonyPostfix]
    public static void Activate(Item_PlayerHeart __instance)
    {
        Plugin.Logger.LogDebug($"Item_PlayerHeart.Activate({__instance}, {__instance.actorID})");
        Game.UpdateEntity(__instance.sprite.gameObject, __instance.actorID);
    }
}

[HarmonyPatch(typeof(Item_PlayerStrength))]
internal class Item_PlayerStrength_Patch
{
    [HarmonyPatch(nameof(Item_PlayerStrength.Collect))]
    [HarmonyPrefix]
    public static void Collect(Item_PlayerStrength __instance)
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

    [HarmonyPatch(nameof(Item_PlayerStrength.Activate))]
    [HarmonyPostfix]
    public static void Activate(Item_PlayerStrength __instance)
    {
        Plugin.Logger.LogDebug($"Item_PlayerStrength.Activate({__instance}, {__instance.actorID})");
        Game.UpdateEntity(__instance.sprite.gameObject, __instance.actorID);
    }
}

[HarmonyPatch(typeof(ItemGorgonHeart))]
internal class ItemGorgonHeart_Patch
{
    [HarmonyPatch(nameof(ItemGorgonHeart.Activate))]
    [HarmonyPostfix]
    public static void Activate(ItemGorgonHeart __instance)
    {
        Plugin.Logger.LogDebug($"ItemGorgonHeart.Activate({__instance}, {__instance.actorID})");
        Game.UpdateItem(__instance);
    }

    [HarmonyPatch(nameof(ItemGorgonHeart.OnDamaged))]
    [HarmonyPostfix]
    public static void OnDamaged(ItemGorgonHeart __instance)
    {
        Plugin.Logger.LogDebug($"ItemGorgonHeart.OnDamaged({__instance}, {__instance.actorID})");
        Game.UpdateItem(__instance);
    }
}

[HarmonyPatch(typeof(Key))]
internal class Key_Patch
{
    [HarmonyPatch(nameof(Key.Collect))]
    [HarmonyPrefix]
    public static void Collect(Key __instance)
    {
        Plugin.Logger.LogDebug($"Key.Collect({__instance}, {__instance.actorID}, {__instance.room.roomID})");
        switch (__instance.keyType)
        {
            case Key.KeyType.White when ArchipelagoClient.ServerData.SlotData.RandomizeWhiteKeys &&
                                        Data.WhiteKeyMap.ContainsKey(__instance.actorID):
            case Key.KeyType.Blue when ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys &&
                                       Data.BlueKeyMap.ContainsKey(__instance.actorID):
            case Key.KeyType.Red when ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys &&
                                      Data.RedKeyMap.ContainsKey(__instance.actorID):
                __instance.useItemBox = false;
                __instance.collectedSound = null;
                __instance.collectedText = null;
                break;
        }
    }

    [HarmonyPatch(nameof(Key.LateActivate))]
    [HarmonyPostfix]
    public static void LateActivate(Key __instance)
    {
        Plugin.Logger.LogDebug($"Key.LateActivate({__instance}, {__instance.actorID})");
        Game.UpdateEntity(__instance.gameObject, __instance.actorID);
    }
}

[HarmonyPatch(typeof(KeyPickable))]
internal class KeyPickable_Collect_Patch
{
    [HarmonyPatch(nameof(KeyPickable.Collect))]
    [HarmonyPrefix]
    public static void Collect(KeyPickable __instance)
    {
        Plugin.Logger.LogDebug(
            $"KeyPickable.Collect({__instance}, {__instance.actorID}, {__instance.room?.roomID}, {__instance.keyType}, {__instance.poolName})");
        if (__instance.keyType == Key.KeyType.Blue && ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys &&
            (Data.BlueKeyMap.ContainsKey(__instance.actorID) ||
             (__instance.actorID == 0 && Data.PotKeyMap.ContainsKey(Player.PlayerDataLocal.currentRoomID))))
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
    public static bool CollectItemID(ItemProperties.ItemID itemID)
    {
        Plugin.Logger.LogDebug($"PlayerData.CollectItemID({itemID})");
        if (!Game.ReceivingItem && itemID == ItemProperties.ItemID.ZeekItem)
        {
            return !Game.CollectItem(itemID);
        }

        return true;
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

    [HarmonyPatch(nameof(PlayerData.UnlockCharacter))]
    [HarmonyPrefix]
    public static void UnlockCharacter(CharacterProperties.Character _character)
    {
        Plugin.Logger.LogDebug($"PlayerData.UnlockCharacter({_character})");
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

[HarmonyPatch(typeof(EnemyEntity))]
internal class EnemyEntity_Damage_Patch
{
    [HarmonyPatch(nameof(EnemyEntity.Damage))]
    [HarmonyPrefix]
    public static void Damage(ref int damageAmount)
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
        if (string.IsNullOrWhiteSpace(_itemText))
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

[HarmonyPatch(typeof(Cutscene))]
internal class Cutscene_PlayCutscene_Patch
{
    [HarmonyPatch(nameof(Cutscene.PlayCutscene))]
    [HarmonyPrefix]
    public static void PlayCutscene(Cutscene __instance)
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

[HarmonyPatch(typeof(CutsceneObject))]
internal class CutsceneObject_LoadData_Patch
{
    [HarmonyPatch(nameof(CutsceneObject.LoadData))]
    [HarmonyPostfix]
    public static void LoadData(CutsceneObject __instance)
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

[HarmonyPatch(typeof(LocalizationManager))]
internal class LocalizationManager_TryGetTranslation_Patch
{
    [HarmonyPatch(nameof(LocalizationManager.TryGetTranslation))]
    [HarmonyPostfix]
    public static void TryGetTranslation(string Term, ref string Translation, bool __result)
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
        Plugin.ArchipelagoClient.Disconnect();
        ArchipelagoClient.ServerData.Clear();
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
        ArchipelagoClient.ServerData.UpdateSave();
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
    [HarmonyPrefix]
    public static void Update()
    {
        Game.Update();
    }
}

[HarmonyPatch(typeof(InputListener))]
internal class InputListener_Patch
{
    [HarmonyPatch(nameof(InputListener.Update))]
    [HarmonyPrefix]
    public static bool Update()
    {
        return ArchipelagoConsole.Hidden || !Settings.ShowConsole;
    }
}

[HarmonyPatch(typeof(Room))]
internal class Room_Patch
{
    [HarmonyPatch(nameof(Room.ActivateInisde))]
    [HarmonyPostfix]
    public static void ActivateInisde(Room __instance)
    {
        Plugin.Logger.LogDebug("Room.ActivateInisde()");
    }
}