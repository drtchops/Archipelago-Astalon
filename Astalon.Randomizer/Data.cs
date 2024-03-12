using System.Collections.Generic;

namespace Astalon.Randomizer;

public static class Data
{
    public static readonly Dictionary<ItemProperties.ItemID, string> LocationMap = new()
    {
        { ItemProperties.ItemID.AmuletOfSol, "Hall of the Phantoms - Amulet of Sol" },
        { ItemProperties.ItemID.BanishSpell, "Gorgon Tomb - Banish Spell" },
        { ItemProperties.ItemID.GorgonHeart, "Gorgon Tomb - Gorgonheart" },
        { ItemProperties.ItemID.GriffonClaw, "Hall of the Phantoms - Griffon Claw" },
        { ItemProperties.ItemID.IcarusEmblem, "Ruins of Ash - Icarus Emblem" },
        { ItemProperties.ItemID.LunarianBow, "Catacombs - Lunarian Bow" },
        { ItemProperties.ItemID.RingOfTheAncients, "Gorgon Tomb - Ring of the Ancients" },
        { ItemProperties.ItemID.SwordOfMirrors, "Gorgon Tomb - Sword of Mirrors" },
        { ItemProperties.ItemID.GorgonEyeRed, "Gorgon Tomb - Gorgon Eye (Red)" },
        { ItemProperties.ItemID.GorgonEyeBlue, "Mechanism - Gorgon Eye (Blue)" },
        { ItemProperties.ItemID.GorgonEyeGreen, "Ruins of Ash - Gorgon Eye (Green)" },
        { ItemProperties.ItemID.DeadMaidensRing, "Hall of the Phantoms - Dead Maiden's Ring" },
        { ItemProperties.ItemID.LinusMap, "Gorgon Tomb - Linus' Map" },
        { ItemProperties.ItemID.AthenasBell, "Hall of the Phantoms - Athena's Bell" },
        { ItemProperties.ItemID.VoidCharm, "Gorgon Tomb - Void Charm" },
        { ItemProperties.ItemID.CloakOfLevitation, "Mechanism - Cloak of Levitation" },
        { ItemProperties.ItemID.AdornedKey, "Tower Roots - Adorned Key" },
        // {ItemProperties.ItemID.PrincesCrown, "Cyclops Den - Prince's Crown"},
        { ItemProperties.ItemID.AscendantKey, "Gorgon Tomb - Ascendant Key" },
        { ItemProperties.ItemID.TalariaBoots, "Mechanism - Talaria Boots" },
        // {ItemProperties.ItemID.MonsterBall, "Gorgon Tomb - Monster Ball"},
        { ItemProperties.ItemID.BloodChalice, "The Apex - Blood Chalice" },
        { ItemProperties.ItemID.MorningStar, "Serpent Path - Morning Star" },
        // {ItemProperties.ItemID.CyclopsIdol, "Mechanism - Cyclops Idol"},
        { ItemProperties.ItemID.BoreasGauntlet, "Hall of the Phantoms - Boreas Gauntlet" },
        // {ItemProperties.ItemID.FamiliarGil, "Catacombs - Gil"},
        { ItemProperties.ItemID.MagicBlock, "Cathedral - Magic Block" },
    };

    public static readonly Dictionary<string, ItemProperties.ItemID> ItemMap = new()
    {
        { "Amulet of Sol", ItemProperties.ItemID.AmuletOfSol },
        { "Banish Spell", ItemProperties.ItemID.BanishSpell },
        { "Gorgonheart", ItemProperties.ItemID.GorgonHeart },
        { "Griffon Claw", ItemProperties.ItemID.GriffonClaw },
        { "Icarus Emblem", ItemProperties.ItemID.IcarusEmblem },
        { "Lunarian Bow", ItemProperties.ItemID.LunarianBow },
        { "Ring of the Ancients", ItemProperties.ItemID.RingOfTheAncients },
        { "Sword of Mirrors", ItemProperties.ItemID.SwordOfMirrors },
        { "Gorgon Eye (Red)", ItemProperties.ItemID.GorgonEyeRed },
        { "Gorgon Eye (Blue)", ItemProperties.ItemID.GorgonEyeBlue },
        { "Gorgon Eye (Green)", ItemProperties.ItemID.GorgonEyeGreen },
        { "Dead Maiden's Ring", ItemProperties.ItemID.DeadMaidensRing },
        { "Linus' Map", ItemProperties.ItemID.LinusMap },
        { "Athena's Bell", ItemProperties.ItemID.AthenasBell },
        { "Void Charm", ItemProperties.ItemID.VoidCharm },
        { "Cloak of Levitation", ItemProperties.ItemID.CloakOfLevitation },
        { "Adorned Key", ItemProperties.ItemID.AdornedKey },
        // {"Prince's Crown", ItemProperties.ItemID.PrincesCrown}, // not sure how this is handled
        { "Ascendant Key", ItemProperties.ItemID.AscendantKey },
        { "Talaria Boots", ItemProperties.ItemID.TalariaBoots },
        // {"Monster Ball", ItemProperties.ItemID.MonsterBall},
        { "Blood Chalice", ItemProperties.ItemID.BloodChalice },
        { "Morning Star", ItemProperties.ItemID.MorningStar },
        // {"Cyclops Idol", ItemProperties.ItemID.CyclopsIdol}, // crashes
        { "Boreas Gauntlet", ItemProperties.ItemID.BoreasGauntlet },
        // {"Gil", ItemProperties.ItemID.FamiliarGil}, // crashes
        { "Magic Block", ItemProperties.ItemID.MagicBlock },
    };

    public static readonly Dictionary<string, string> IconMap = new()
    {
        { "Amulet of Sol", "Item_AmuletOfSol" },
        { "Banish Spell", "Item_BanishSpell" },
        { "Gorgonheart", "Item_GorgonHeart" },
        { "Griffon Claw", "Item_GriffonClaw" },
        { "Icarus Emblem", "Item_IcarusEmblem" },
        { "Lunarian Bow", "Item_LunarianBow" },
        { "Ring of the Ancients", "Item_RingOfTheAncients" },
        { "Sword of Mirrors", "Item_SwordOfMirrors" },
        { "Gorgon Eye (Red)", "Item_GorgonEyeRed" },
        { "Gorgon Eye (Blue)", "Item_GorgonEyeBlue" },
        { "Gorgon Eye (Green)", "Item_GorgonEyeGreen" },
        { "Dead Maiden's Ring", "Item_DeadMaidensRing" },
        { "Linus' Map", "Item_LinusMap" },
        { "Athena's Bell", "Item_AthenasBell" },
        { "Void Charm", "Item_VoidCharm" },
        { "Cloak of Levitation", "Item_CloakOfLevitation" },
        { "Adorned Key", "Item_AdornedKey" },
        { "Prince's Crown", "Item_PrincesCrown" },
        { "Ascendant Key", "Item_AscendantKey" },
        { "Talaria Boots", "Item_TalariaBoots" },
        { "Monster Ball", "Item_MonsterBall" },
        { "Blood Chalice", "Item_BloodChalice" },
        { "Morning Star", "Item_MorningStar" },
        { "Cyclops Idol", "Item_CyclopsIdol" },
        { "Boreas Gauntlet", "Item_BoreasGauntlet" },
        { "Gil", "Item_FamiliarGil" },
        { "Magic Block", "Item_MagicBlock" },
        { "Max HP +1", "Item_HealthStone_1" },
        { "Max HP +2", "Item_HealthStone_1" },
        { "Max HP +3", "Item_HealthStone_1" },
        { "Max HP +4", "Item_HealthStone_1" },
        { "Max HP +5", "Item_HealthStone_1" },
        { "50 Orbs", "Deal_OrbReaper" },
        { "100 Orbs", "Deal_OrbReaper" },
        { "200 Orbs", "Deal_OrbReaper" },
        { "Attack +1", "Item_PowerStone_1" },
        { "White Key", "WhiteKey_1" },
        { "Blue Key", "BlueKey_1" },
        { "Red Key", "RedKey_1" },
        { "Red Door (Zeek)", "RedKey_1" },
        { "Red Door (Cathedral)", "RedKey_1" },
        { "Red Door (Serpent Path)", "RedKey_1" },
        { "Red Door (Tower Roots)", "RedKey_1" },
        { "Red Door (Dev Room)", "RedKey_1" },
        { "Death", "Deal_Gift" },
        { "Algus", "ElevatorMenu_Icon_Algus" },
        { "Arias", "ElevatorMenu_Icon_Arias" },
        { "Bram", "ElevatorMenu_Icon_Bram" },
        { "Kyuli", "ElevatorMenu_Icon_Kyuli" },
        { "Zeek", "ElevatorMenu_Icon_Zeek" },
    };

    public static readonly Dictionary<int, string> AttackMap = new()
    {
        { 7094, "Gorgon Tomb - Attack +1" }, // past void charm + green eye
        { 6753, "Mechanism - Attack +1 (Above Volantis)" }, // above volantis, needs griffon claw
        { 9229, "Mechanism - Attack +1 (Morning Star Blocks)" }, // need bram & morning star
        { 8996, "Ruins of Ash - Attack +1" }, // needs morning star
        { 6563, "Catacombs - Attack +1 (Item Chain Red)" }, // item chain, needs red
        { 7105, "Catacombs - Attack +1 (Item Chain Blue)" }, // item chain, needs blue
        { 7107, "Catacombs - Attack +1 (Item Chain Green)" }, // item chain, needs green
        { 6594, "Catacombs - Attack +1 (Climbable Root)" }, // climb up root
        { 10042, "Catacombs - Attack +1 (Poison Roots)" }, // after poison roots
        { 8531, "Cyclops Den - Attack +1" },
        { 8619, "Cathedral - Attack +1" },
        { 8181, "Serpent Path - Attack +1" },
    };

    public static readonly Dictionary<int, string> HealthMap = new()
    {
        { 6145, "Gorgon Tomb - Max HP +1 (Ring of the Ancients)" }, // by ring of the ancients, needs red eye
        { 6134, "Gorgon Tomb - Max HP +5 (Ascendant Key)" }, // above ascendant key, needs griffon claw
        { 9936, "Mechanism - Max HP +1 (Secret Switch)" }, // secret switch
        { 10280, "Mechanism - Max HP +1 (Morning Star Blocks)" }, // tunnel to HotP, needs morning star
        { 6201, "Mechanism - Max HP +3 (Above Checkpoint)" }, // above checkpoint, needs griffon claw
        { 9979, "Hall of the Phantoms - Max HP +1 (Griffon Claw)" }, // left of claw
        { 6173, "Hall of the Phantoms - Max HP +2 (Secret Ladder)" }, // secret ladder
        { 9279, "Hall of the Phantoms - Max HP +2 (Boreas Gauntlet)" }, // after teleporter maze, needs zeek
        { 6436, "Hall of the Phantoms - Max HP +5 (Old Man)" }, // above old man, need claw
        { 6740, "Hall of the Phantoms - Max HP +5 (Teleport Maze)" }, // after tp maze
        { 6518, "Hall of the Phantoms - Max HP +5 (Above Start)" }, // above start
        { 9846, "Ruins of Ash - Max HP +1 (Left of Ascent)" }, // left of ascend
        { 9570, "Ruins of Ash - Max HP +2 (Right Side)" }, // right side, needs boreas or morning star
        { 10025, "Ruins of Ash - Max HP +5 (After Solaria)" }, // after solaria
        { 10070, "Darkness - Max HP +4" },
        { 8731, "The Apex - Max HP +1 (Blood Chalice)" }, // towards blood chalice
        { 6820, "The Apex - Max HP +5 (After Heart)" }, // after heart
        { 6177, "Catacombs - Max HP +1 (First Room)" }, // first room of catacombs
        { 10003, "Catacombs - Max HP +1 (Cyclops Arena)" }, // cyclops room, needs sword of mirrors
        { 10324, "Catacombs - Max HP +1 (Above Poison Roots)" }, // before poison roots
        { 10137, "Catacombs - Max HP +2 (Before Poison Roots)" }, // before poison roots
        { 10043, "Catacombs - Max HP +2 (After Poison Roots)" }, // after poison roots
        { 10581, "Catacombs - Max HP +2 (Before Gemini Bottom)" }, // before boss bottom
        { 10583, "Catacombs - Max HP +2 (Before Gemini Top)" }, // before boss top
        { 10138, "Catacombs - Max HP +2 (Above Gemini)" }, // above boss, need boreas gauntlet & griffon claw
        { 10121, "Catacombs - Max HP +5 (Item Chain)" }, // item chain, needs morning star, griffon claw, bell
        { 9779, "Tower Roots - Max HP +1 (Bottom)" }, // bottom
        { 9778, "Tower Roots - Max HP +2 (Top)" }, // top
        { 8532, "Cyclops Den - Max HP +1" },
        { 9833, "Cathedral - Max HP +1 (Top Left)" }, // top left
        { 9834, "Cathedral - Max HP +1 (Top Right)" }, // top right
        { 10204, "Cathedral - Max HP +2 (Left Climb)" }, // top of left side, needs claw
        { 8609, "Cathedral - Max HP +5 (Bell)" }, // near bell
        { 8202, "Serpent Path - Max HP +1" }, // before frog
    };

    public static readonly Dictionary<int, string> WhiteKeyMap = new()
    {
        { 8, "Gorgon Tomb - White Key (First Room)" },
    };

    public static readonly Dictionary<int, string> BlueKeyMap = new()
    {
        { 432, "Gorgon Tomb - Blue Key (4 Boneworms)" },
    };

    public static readonly Dictionary<int, string> RedKeyMap = new()
    {
        { 9941, "Gorgon Tomb - Red Key" },
        { 9571, "Mechanism - Red Key" },
        { 3090, "Hall of the Phantoms - Red Key" },
        { 7176, "Ruins of Ash - Red Key" },
        { 7273, "Tower Roots - Red Key" },
    };

    public static readonly Dictionary<string, (int, int)> RedDoorMap = new()
    {
        { "Red Door (Zeek)", (3227, 3288) },
        { "Red Door (Cathedral)", (7055, 7252) },
        { "Red Door (Serpent Path)", (5804, 7335) },
        { "Red Door (Tower Roots)", (2706, 8812) },
        { "Red Door (Dev Room)", (2598, 3276) },
    };

    // elevator rooms
    // 6629 - start elevator
    // 248 - gorgon tomb 2
    // 4109 - the apex
}