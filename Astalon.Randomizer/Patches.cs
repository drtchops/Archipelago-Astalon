using System.Collections.Generic;
using Astalon.Randomizer.Archipelago;
using HarmonyLib;
using I2.Loc;
using UnityEngine;

// ReSharper disable InconsistentNaming

namespace Astalon.Randomizer;

[HarmonyPatch(typeof(Collectable))]
internal class Collectable_Patch
{
    [HarmonyPatch(nameof(Collectable.Activate))]
    [HarmonyPostfix]
    public static void Activate(Collectable __instance)
    {
        Plugin.Logger.LogDebug($"Collectable.Activate({__instance.actorID})");
        Game.UpdateEntity(__instance.gameObject, __instance.actorID);
    }
}

[HarmonyPatch(typeof(Item))]
internal class Item_Patch
{
    [HarmonyPatch(nameof(Item.Collect))]
    [HarmonyPrefix]
    public static void Collect(Item __instance)
    {
        Plugin.Logger.LogDebug($"Item.Collect({__instance}, {__instance.actorID})");
        if (Game.TryGetItemLocation(__instance.itemProperties.itemID, out _))
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
    public static void CollectPre(Item_PlayerHeart __instance)
    {
        Plugin.Logger.LogDebug(
            $"Item_PlayerHeart.Collect({__instance}, {__instance.actorID}, {__instance.collectedIcon})");
        if (Game.TryGetEntityLocation(__instance.actorID, out _))
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
        if (Game.TryGetEntityLocation(__instance.actorID, out _))
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
        if (Game.TryGetEntityLocation(__instance.actorID, out _))
        {
            __instance.useItemBox = false;
            __instance.collectedSound = null;
            __instance.collectedText = null;
        }
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
        if (Game.TryGetEntityLocation(__instance.actorID, out _))
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
        return Game.CanDoorOpen(keyType);
    }

    [HarmonyPatch(nameof(PlayerData.AddKeys))]
    [HarmonyPrefix]
    public static bool AddKeys(Key.KeyType keyType, int amount)
    {
        Plugin.Logger.LogDebug($"PlayerData.AddKeys({keyType}, {amount})");
        return Game.CanDoorOpen(keyType);
    }

    [HarmonyPatch(nameof(PlayerData.UseKey))]
    [HarmonyPrefix]
    public static bool UseKeyPrefix(Key.KeyType keyType)
    {
        Plugin.Logger.LogDebug($"PlayerData.UseKeyPrefix({keyType})");
        return Game.CanDoorOpen(keyType);
    }

    [HarmonyPatch(nameof(PlayerData.UseKey))]
    [HarmonyPostfix]
    public static void UseKeyPostfix(Key.KeyType keyType)
    {
        Plugin.Logger.LogDebug($"PlayerData.UseKeyPostfix({keyType})");

        if (!Settings.FreeKeys)
        {
            return;
        }

        switch (keyType)
        {
            case Key.KeyType.White when !ArchipelagoClient.ServerData.SlotData.RandomizeWhiteKeys:
                Player.PlayerDataLocal.whiteKeys += 1;
                break;
            case Key.KeyType.Blue when !ArchipelagoClient.ServerData.SlotData.RandomizeBlueKeys:
                Player.PlayerDataLocal.blueKeys += 1;
                break;
            case Key.KeyType.Red when !ArchipelagoClient.ServerData.SlotData.RandomizeRedKeys:
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
    [HarmonyPrefix]
    public static void UnlockElevatorPre(int roomID)
    {
        Plugin.Logger.LogDebug($"PlayerData.UnlockElevatorPre({roomID})");
    }

    [HarmonyPatch(nameof(PlayerData.UnlockElevator))]
    [HarmonyPostfix]
    public static void UnlockElevatorPost()
    {
        Game.UpdateElevatorList();
    }

    [HarmonyPatch(nameof(PlayerData.UnlockCharacter))]
    [HarmonyPrefix]
    public static bool UnlockCharacter(CharacterProperties.Character _character)
    {
        Plugin.Logger.LogDebug($"PlayerData.UnlockCharacter({_character})");
        return !Game.CharacterUnlocked(_character);
    }

    [HarmonyPatch(nameof(PlayerData.IsDealPurchased))]
    [HarmonyPrefix]
    public static bool IsDealPurchased(DealProperties.DealID dealID, ref bool __result)
    {
        //Plugin.Logger.LogDebug($"PlayerData.IsDealPurchased({dealID})");
        if (Game.ShouldCheckDeal(dealID))
        {
            __result = Game.IsDealReceived(dealID);
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(PlayerData.PurchaseDeal))]
    [HarmonyPrefix]
    public static void PurchaseDealPre(DealProperties.DealID _dealID)
    {
        Plugin.Logger.LogDebug($"PlayerData.PurchaseDealPre({_dealID})");
        Game.PurchaseDeal(_dealID);
    }

    [HarmonyPatch(nameof(PlayerData.AddDefaultDeals))]
    [HarmonyPostfix]
    public static void AddDefaultDeals()
    {
        Plugin.Logger.LogDebug("PlayerData.AddDefaultDeals()");
        Game.MakeCharacterDealsUnavailable();
    }

    [HarmonyPatch(nameof(PlayerData.HasItem))]
    [HarmonyPrefix]
    public static bool HasItem(ItemProperties.ItemID itemID, ref bool __result)
    {
        // Plugin.Logger.LogDebug($"PlayerData.HasItem({itemID})");
        if (Game.TryHasItem(itemID, out var result))
        {
            __result = result;
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(PlayerData.HasUnlockedCharacter), typeof(CharacterProperties.Character))]
    [HarmonyPrefix]
    public static bool HasUnlockedCharacter(CharacterProperties.Character _character, ref bool __result)
    {
        // Plugin.Logger.LogDebug($"PlayerData.HasUnlockedCharacter({_character})");
        if (Game.TryHasUnlockedCharacter(_character, out var result))
        {
            __result = result;
            return false;
        }

        return true;
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
        Game.LoadSave();
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
        //Plugin.Logger.LogDebug($"Player.AssignRoom({_room.roomID})");
        Game.ExploreRoom(_room);
    }

    [HarmonyPatch(nameof(Player.ChangeCharacters))]
    [HarmonyPrefix]
    public static bool ChangeCharacters()
    {
        Plugin.Logger.LogDebug("Player.ChangeCharacters()");
        return Game.CanCycleCharacter();
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

[HarmonyPatch(typeof(Boss_Tauros))]
internal class Boss_Tauros_Patch
{
    [HarmonyPatch(nameof(Boss_Tauros.Death))]
    [HarmonyPrefix]
    public static void Death(Boss_Tauros __instance)
    {
        Plugin.Logger.LogDebug($"Boss_Tauros.Death()");
        if (__instance.taurosEye.gameObject.TryGetComponent<Item>(out var item))
        {
            Game.UpdateItem(item);
        }
    }
}

[HarmonyPatch(typeof(Boss_Worm))]
internal class Boss_Worm_Patch
{
    [HarmonyPatch(nameof(Boss_Worm.Death))]
    [HarmonyPrefix]
    public static void Death(Boss_Worm __instance)
    {
        Plugin.Logger.LogDebug($"Boss_Worm.Death()");
        if (__instance.gorgonEye.gameObject.TryGetComponent<Item>(out var item))
        {
            Game.UpdateItem(item);
        }
    }

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

[HarmonyPatch(typeof(Boss_Maw))]
internal class Boss_Maw_Patch
{
    [HarmonyPatch(nameof(Boss_Maw.Death))]
    [HarmonyPrefix]
    public static void Death(Boss_Maw __instance)
    {
        Plugin.Logger.LogDebug($"Boss_Maw.Death()");
        if (__instance.gorgonEye.gameObject.TryGetComponent<Item>(out var item))
        {
            Game.UpdateItem(item);
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
        return !string.IsNullOrWhiteSpace(_itemText);
    }

    [HarmonyPatch(nameof(GameplayUIManager.OpenEpimetheusShop))]
    [HarmonyPrefix]
    public static void OpenEpimetheusShop()
    {
        Plugin.Logger.LogDebug("GameplayUIManager.OpenEpimetheusShop()");
        Game.IsInShop = true;
    }

    [HarmonyPatch(nameof(GameplayUIManager.InitializeEpimetheusShop))]
    [HarmonyPrefix]
    public static void InitializeEpimetheusShop()
    {
        Plugin.Logger.LogDebug("GameplayUIManager.InitializeEpimetheusShop()");
    }

    [HarmonyPatch(nameof(GameplayUIManager.CloseEpimetheusShop))]
    [HarmonyPrefix]
    public static void CloseEpimetheusShop()
    {
        Plugin.Logger.LogDebug("GameplayUIManager.CloseEpimetheusShop()");
        Game.IsInShop = false;
    }

    [HarmonyPatch(nameof(GameplayUIManager.ReplaceVariable))]
    [HarmonyPrefix]
    public static bool ReplaceVariable(string _text, ref string __result)
    {
        //Plugin.Logger.LogDebug($"GameplayUIManager.ReplaceVariable({_text})");
        if (_text.StartsWith("ARCHIPELAGO:"))
        {
            __result = _text[12..];
            return false;
        }

        return true;
    }

    [HarmonyPatch(nameof(GameplayUIManager.OpenConfirmPurchaseMenu))]
    [HarmonyPostfix]
    public static void OpenConfirmPurchaseMenu(DealProperties _confirmPurchaseDeal, GameplayUIManager __instance)
    {
        Plugin.Logger.LogDebug($"GameplayUIManager.OpenConfirmPurchaseMenu({_confirmPurchaseDeal.dealID})");
        if (Game.TryUpdateDeal(_confirmPurchaseDeal.dealID, out _, out var name, out var playerName))
        {
            // TODO: make text box wider
            var title = name;
            if (playerName != null)
            {
                title = $"{playerName}'s {name}";
            }

            __instance.confirmPurchaseDealTitle.text = title;
        }
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
        Game.SetupNewSave();
    }

    [HarmonyPatch(nameof(GameLoader.RestartGame))]
    [HarmonyPostfix]
    public static void RestartGame()
    {
        Plugin.Logger.LogDebug($"GameLoader.RestartGame()");
        Game.ExitSave();
        Plugin.ArchipelagoClient.Disconnect();
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
        Game.LoadSave();
        Game.ConnectSave();
        Game.UpdateSaveData();
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
        return (ArchipelagoConsole.Hidden || !Settings.ShowConsole) && !Il2CppBase.ConnectionFocused;
    }
}

[HarmonyPatch(typeof(PlayerPhysics))]
internal class PlayerPhysics_Patch
{
    [HarmonyPatch(nameof(PlayerPhysics.Jump))]
    [HarmonyPrefix]
    public static void JumpPre(PlayerPhysics __instance)
    {
        //Plugin.Logger.LogDebug("PlayerPhysics.Jump()");
        if (Settings.InfiniteJumps)
        {
            __instance.grounded = true;
        }
    }
}

[HarmonyPatch(typeof(LocalizationManager))]
internal class LocalizationManager_Patch
{
    [HarmonyPatch(nameof(LocalizationManager.TryGetTranslation))]
    [HarmonyPostfix]
    public static void TryGetTranslation(string Term, ref string Translation, ref bool __result)
    {
        if (Term.StartsWith("ARCHIPELAGO:"))
        {
            Translation = Term[12..];
            __result = true;
        }
    }
}

[HarmonyPatch(typeof(ItemManager))]
internal class ItemManager_Patch
{
    [HarmonyPatch(nameof(ItemManager.GetDealProperties))]
    [HarmonyPostfix]
    public static void GetDealProperties(ref DealProperties __result)
    {
        if (Game.TryUpdateDeal(__result.dealID, out var sprite, out var name, out _))
        {
            __result.dealIcon = sprite;
            __result.dealName = "ARCHIPELAGO:" + name;
        }
    }
}

[HarmonyPatch(typeof(ShopSubMenu))]
internal class ShopSubMenu_Patch
{
    [HarmonyPatch(nameof(ShopSubMenu.UpdateDeal))]
    [HarmonyPostfix]
    public static void UpdateDeal(DealProperties _deal, int _dealIndex, ShopSubMenu __instance)
    {
        Plugin.Logger.LogDebug($"ShopSubMenu.UpdateDeal({_deal.dealID}, {_dealIndex})");
        if (Game.TryUpdateDeal(_deal.dealID, out _, out var name, out var playerName))
        {
            var description = name;
            if (playerName != null)
            {
                description = $"{playerName}'s {name}";
            }

            __instance.dealTitle.text = name;
            __instance.dealDescription.text = description;
        }
    }
}

[HarmonyPatch(typeof(SavePoint))]
internal class SavePoint_Patch
{
    [HarmonyPatch(nameof(SavePoint.ChangeCharacters))]
    [HarmonyPrefix]
    public static bool ChangeCharacters()
    {
        Plugin.Logger.LogDebug("SavePoint.ChangeCharacters()");
        return Game.CanCycleCharacter();
    }

    [HarmonyPatch(nameof(SavePoint.UpdateCharacters))]
    [HarmonyPrefix]
    public static void UpdateCharacters(SavePoint __instance)
    {
        Plugin.Logger.LogDebug("SavePoint.UpdateCharacters()");
        Game.UpdateSaveCharacters(__instance);
    }
}

[HarmonyPatch(typeof(Room))]
internal class Room_Patch
{
    [HarmonyPatch(nameof(Room.ActivateInisde))]
    [HarmonyPostfix]
    public static void ActivateInisde(Room __instance)
    {
        Plugin.Logger.LogDebug($"Room {__instance.RoomID} '{__instance.name}' (Area {__instance.GetRoomArea()})");

        // if (__instance.objectSwitches.Count > 0)
        // {
        //     List<int> ids = [];
        //     foreach (var objSwitch in __instance.objectSwitches)
        //     {
        //         ids.Add(objSwitch.actorID);
        //     }

        //     Plugin.Logger.LogDebug($"Switches: {string.Join(", ", ids)}");
        // }

        // if (__instance.switchableObjects.Count > 0)
        // {
        //     List<int> ids = [];
        //     foreach (var switchObj in __instance.switchableObjects)
        //     {
        //         ids.Add(switchObj.actorID);
        //     }

        //     Plugin.Logger.LogDebug($"Switchables: {string.Join(", ", ids)}");
        // }
    }
}

[HarmonyPatch(typeof(ObjectSwitch))]
internal class ObjectSwitch_Patch
{
    [HarmonyPatch(nameof(ObjectSwitch.ActivateObject))]
    [HarmonyPrefix]
    public static void ActivateObject(ObjectSwitch __instance)
    {
        Plugin.Logger.LogDebug(
            $"ObjectSwitch.ActivateObject({__instance.actorID}, {__instance.room?.roomID}, {__instance.switchID})");
        var roomId = __instance.room?.roomID ?? Player.PlayerDataLocal.currentRoomID;
        Game.PressSwitch(roomId, __instance.switchID);
    }
}

[HarmonyPatch(typeof(MagicCrystal))]
internal class MagicCrystal_Patch
{
    [HarmonyPatch(nameof(MagicCrystal.ActivateObject))]
    [HarmonyPrefix]
    public static void ActivateObject(MagicCrystal __instance)
    {
        Plugin.Logger.LogDebug(
            $"MagicCrystal.ActivateObject({__instance.actorID}, {__instance.room?.roomID}, {__instance.switchID})");
        var roomId = __instance.room?.roomID ?? Player.PlayerDataLocal.currentRoomID;
        Game.PressSwitch(roomId, __instance.switchID);
    }
}

[HarmonyPatch(typeof(SwitchableObject))]
internal class SwitchableObject_Patch
{
    [HarmonyPatch(nameof(SwitchableObject.ToggleObject))]
    [HarmonyPrefix]
    public static bool ToggleObject(SwitchableObject __instance)
    {
        Plugin.Logger.LogDebug(
            $"SwitchableObject.ToggleObject({__instance.actorID}, {__instance.objectType}, {__instance.linkID})");
        var roomId = __instance.room?.roomID ?? Player.PlayerDataLocal.currentRoomID;
        return !Game.IsSwitchRandomized(roomId, __instance.linkID, out _);
    }
}

[HarmonyPatch(typeof(Elevator))]
internal class Elevator_Patch
{
    [HarmonyPatch(nameof(Elevator.UnlockElevator))]
    [HarmonyPrefix]
    public static void UnlockElevator(Elevator __instance)
    {
        Plugin.Logger.LogDebug($"Elevator.UnlockElevator({__instance.actorID}, {__instance.room?.roomID})");
        Game.ElevatorUnlocked(__instance.room?.roomID ?? -1);
    }

    [HarmonyPatch(nameof(Elevator.TriggerElevatorMenu))]
    [HarmonyPrefix]
    public static void TriggerElevatorMenu(Elevator __instance)
    {
        Plugin.Logger.LogDebug($"Elevator.TriggerElevatorMenu({__instance.actorID}, {__instance.room?.roomID})");
        Game.ElevatorUnlocked(__instance.room?.roomID ?? -1);
    }
}

[HarmonyPatch(typeof(Room_Zeek))]
internal class Room_Zeek_Patch
{
    [HarmonyPatch(nameof(Room_Zeek.Activate))]
    [HarmonyPrefix]
    public static void ActivatePre()
    {
        Plugin.Logger.LogDebug("Room_Zeek.ActivatePre()");
        Game.ActivateZeekRoom();
    }

    [HarmonyPatch(nameof(Room_Zeek.Activate))]
    [HarmonyPostfix]
    public static void ActivatePost()
    {
        Plugin.Logger.LogDebug("Room_Zeek.ActivatePost()");
        Game.DeactivateZeekRoom();
    }
}

[HarmonyPatch(typeof(Room_Bram))]
internal class Room_Bram_Patch
{
    [HarmonyPatch(nameof(Room_Bram.Activate))]
    [HarmonyPrefix]
    public static void ActivatePre()
    {
        Plugin.Logger.LogDebug("Room_Bram.ActivatePre()");
        Game.ActivateBramRoom();
    }

    [HarmonyPatch(nameof(Room_Bram.Activate))]
    [HarmonyPostfix]
    public static void ActivatePost()
    {
        Plugin.Logger.LogDebug("Room_Bram.ActivatePost()");
        Game.DeactivateBramRoom();
    }
}

[HarmonyPatch(typeof(WeaponKyuliOrbBow))]
internal class WeaponKyuliOrbBow_Patch
{
    [HarmonyPatch(nameof(WeaponKyuliOrbBow.Shoot))]
    [HarmonyPrefix]
    public static void Shoot()
    {
        Game.ShootKyuliRay();
    }
}
