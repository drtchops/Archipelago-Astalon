using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astalon.Randomizer;

public readonly struct Checkpoint
{
    public int Id { get; init; }
    public int RoomId { get; init; }
    public Vector3 PlayerPos { get; init; }
    public Vector2 CameraPos { get; init; }
}

public readonly struct SwitchData
{
    public string Id { get; init; }
    public int RoomId { get; init; }
    public string ItemName { get; init; }
    public string LocationName { get; init; }
    public int[] ObjectsToEnable { get; init; }
    public int[] ObjectsToDisable { get; init; }
}

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
        //{ ItemProperties.ItemID.PrincesCrown, "Cyclops Den - Prince's Crown" },
        { ItemProperties.ItemID.AscendantKey, "Gorgon Tomb - Ascendant Key" },
        { ItemProperties.ItemID.TalariaBoots, "Mechanism - Talaria Boots" },
        //{ ItemProperties.ItemID.MonsterBall, "Gorgon Tomb - Monster Ball" },
        { ItemProperties.ItemID.BloodChalice, "The Apex - Blood Chalice" },
        { ItemProperties.ItemID.MorningStar, "Serpent Path - Morning Star" },
        //{ ItemProperties.ItemID.ZeekItem, "Mechanism - Cyclops Idol" },
        { ItemProperties.ItemID.BoreasGauntlet, "Hall of the Phantoms - Boreas Gauntlet" },
        //{ ItemProperties.ItemID.FamiliarGil, "Catacombs - Gil" },
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
        //{ "Prince's Crown", ItemProperties.ItemID.PrincesCrown }, // not sure how this is handled
        { "Ascendant Key", ItemProperties.ItemID.AscendantKey },
        { "Talaria Boots", ItemProperties.ItemID.TalariaBoots },
        //{ "Monster Ball", ItemProperties.ItemID.MonsterBall },
        { "Blood Chalice", ItemProperties.ItemID.BloodChalice },
        { "Morning Star", ItemProperties.ItemID.MorningStar },
        //{ "Cyclops Idol", ItemProperties.ItemID.ZeekItem },
        { "Boreas Gauntlet", ItemProperties.ItemID.BoreasGauntlet },
        //{ "Gil", ItemProperties.ItemID.FamiliarGil }, // crashes
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
        { "Attack +1", "Item_PowerStone_1" },
        { "White Key", "WhiteKey_1" },
        { "Blue Key", "BlueKey_1" },
        { "Red Key", "RedKey_1" },
        { "Death", "Deal_Gift" },
        { "Algus", "ElevatorMenu_Icon_Algus" },
        { "Arias", "ElevatorMenu_Icon_Arias" },
        { "Bram", "ElevatorMenu_Icon_Bram" },
        { "Kyuli", "ElevatorMenu_Icon_Kyuli" },
        { "Zeek", "ElevatorMenu_Icon_Zeek" },
        { "Knowledge", "Deal_Knowledge" },
        { "Orb Seeker", "Deal_OrbReaper" },
        { "Titan's Ego", "Deal_TitanEgo" },
        { "Map Reveal", "Deal_MapReveal" },
        { "Gift", "Deal_Gift" },
        { "Cartographer", "Deal_LockedDoors" },
        { "Death Orb", "Deal_DeathOrb" },
        { "Death Point", "Deal_MapDeath" },
        { "Mercy", "Deal_Mercy" },
        { "Algus's Arcanist", "Deal_Algus_Arcanist" },
        { "Algus's Shock Field", "Deal_Algus_DamageField" },
        { "Algus's Meteor Rain", "Deal_Algus_MeteorRain" },
        { "Arias's Gorgonslayer", "Deal_Arias_GorgonSlayer" },
        { "Arias's Last Stand", "Deal_Arias_LastStand" },
        { "Arias's Lionheart", "Deal_Arias_LionHeart" },
        { "Kyuli's Assassin Strike", "Deal_Kyuli_AssassinStrike" },
        { "Kyuli's Bullseye", "Deal_Kyuli_Bullseye" },
        { "Kyuli's Shining Ray", "Deal_Kyuli_ShiningRay" },
        { "Zeek's Junkyard Hunt", "Deal_Zeek_JunkyardHunt" },
        { "Zeek's Orb Monger", "Deal_Zeek_OrbDigger" },
        { "Zeek's Bigger Loot", "Deal_BiggerLoot" },
        { "Bram's Golden Axe", "Deal_Bram_GoldenAxe" },
        { "Bram's Monster Hunter", "Deal_Bram_MonsterHunter" },
        { "Bram's Whiplash", "Deal_Bram_Whiplash" },
    };

    public static readonly Dictionary<int, string> AttackMap = new()
    {
        { 7094, "Gorgon Tomb - Attack +1" }, // past void charm + green eye
        { 6753, "Mechanism - Attack +1 (Above Volantis)" }, // above volantis, needs griffon claw
        { 9229, "Mechanism - Attack +1 (Morning Star Blocks)" }, // need bram & morning star
        { 8996, "Ruins of Ash - Attack +1" }, // needs morning star
        { 6563, "Caves - Attack +1 (Item Chain Red)" }, // item chain, needs red
        { 7105, "Caves - Attack +1 (Item Chain Blue)" }, // item chain, needs blue
        { 7107, "Caves - Attack +1 (Item Chain Green)" }, // item chain, needs green
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
        { 6177, "Caves - Max HP +1 (First Room)" }, // first room of catacombs
        { 10003, "Caves - Max HP +1 (Cyclops Arena)" }, // cyclops room, needs sword of mirrors
        { 10121, "Caves - Max HP +5 (Item Chain)" }, // item chain, needs morning star, griffon claw, bell
        { 10324, "Catacombs - Max HP +1 (Above Poison Roots)" }, // before poison roots
        { 10137, "Catacombs - Max HP +2 (Before Poison Roots)" }, // before poison roots
        { 10043, "Catacombs - Max HP +2 (After Poison Roots)" }, // after poison roots
        { 10581, "Catacombs - Max HP +2 (Before Gemini Bottom)" }, // before boss bottom
        { 10583, "Catacombs - Max HP +2 (Before Gemini Top)" }, // before boss top
        { 10138, "Catacombs - Max HP +2 (Above Gemini)" }, // above boss, need boreas gauntlet & griffon claw
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
        { 79, "Gorgon Tomb - White Key (Right Side)" },
        { 1088, "Gorgon Tomb - White Key (Before Boss)" },
        { 483, "Mechanism - White Key (Below Linus)" },
        { 6158, "Mechanism - White Key (Black Knight)" },
        { 6163, "Mechanism - White Key (Enemy Arena)" },
        { 1763, "Mechanism - White Key (Top)" },
        { 1193, "Hall of the Phantoms - White Key (Left of Start)" },
        { 10136, "Hall of the Phantoms - White Key (Ghost)" },
        { 6171, "Hall of the Phantoms - White Key (Old Man)" },
        { 6546, "Hall of the Phantoms - White Key (Boss)" },
        { 3772, "Ruins of Ash - White Key (Checkpoint)" },
        { 8650, "Ruins of Ash - White Key (Three Reapers)" },
        { 8678, "Ruins of Ash - White Key (Torches)" },
        { 4714, "Ruins of Ash - White Key (Void Portal)" },
        { 3070, "Darkness - White Key" },
        { 2491, "Catacombs - White Key (On Head)" },
        { 2431, "Catacombs - White Key (Dev Room)" },
        { 4764, "Catacombs - White Key (Prison)" },
    };

    public static readonly Dictionary<int, string> BlueKeyMap = new()
    {
        { 432, "Gorgon Tomb - Blue Key (Bonesnakes)" },
        { 2075, "Gorgon Tomb - Blue Key (Butt)" },
        { 6135, "Gorgon Tomb - Blue Key (Inside Wall)" },
        { 6374, "Mechanism - Blue Key (Void Charm)" },
        { 7140, "Mechanism - Blue Key (Snake Head)" },
        { 7186, "Mechanism - Blue Key (Linus)" },
        { 6261, "Mechanism - Blue Key (Sacrifice)" },
        { 7099, "Mechanism - Blue Key (To Red Key)" },
        { 7167, "Mechanism - Blue Key (Arias)" },
        { 6159, "Mechanism - Blue Key (Switch Blocks)" },
        { 6143, "Mechanism - Blue Key (Top Path)" },
        { 6292, "Mechanism - Blue Key (Old Man)" },
        { 8806, "Mechanism - Blue Key (Checkpoint)" },
        { 6752, "Hall of the Phantoms - Blue Key (Epimetheus)" },
        { 6775, "Hall of the Phantoms - Blue Key (Gold Hint)" },
        { 6483, "Hall of the Phantoms - Blue Key (Amulet of Sol)" },
        { 7090, "Hall of the Phantoms - Blue Key (Secret Ladder)" },
        { 6170, "Hall of the Phantoms - Blue Key (Spike Teleporters)" },
        { 6556, "Hall of the Phantoms - Blue Key (Teleport Maze)" },
        { 6910, "Ruins of Ash - Blue Key (Face)" },
        { 10707, "Ruins of Ash - Blue Key (Flames)" },
        { 7046, "Ruins of Ash - Blue Key (Baby Gorgon)" },
        { 4618, "Ruins of Ash - Blue Key (Top)" },
        { 6739, "The Apex - Blue Key" },
        { 7343, "Catacombs - Blue Key (Slime Water)" },
        { 6880, "Catacombs - Blue Key (Eyeballs)" },
        { 7988, "Serpent Path - Blue Key (Bubbles)" },
        { 8170, "Serpent Path - Blue Key (Morning Star)" },
        { 8111, "Serpent Path - Blue Key (Painting)" },
        { 8009, "Serpent Path - Blue Key (Arias)" },
    };

    public static readonly Dictionary<int, string> RedKeyMap = new()
    {
        { 9941, "Gorgon Tomb - Red Key" },
        { 9571, "Mechanism - Red Key" },
        { 3090, "Hall of the Phantoms - Red Key" },
        { 7176, "Ruins of Ash - Red Key" },
        { 7273, "Tower Roots - Red Key" },
    };

    public static readonly Dictionary<int, string> SpawnedKeyMap = new()
    {
        { 20, "Gorgon Tomb - Blue Key (Bonesnakes)" },
        { 892, "Gorgon Tomb - Blue Key (Butt)" },
        { 27, "Gorgon Tomb - Blue Key (Pot)" },
        { 1177, "Mechanism - Blue Key (Snake Head)" },
        { 1388, "Mechanism - White Key (Enemy Arena)" },
        { 1216, "Mechanism - Blue Key (Pot)" },
        { 3273, "Ruins of Ash - Blue Key (Pot)" },
        { 2671, "Catacombs - Blue Key (Eyeballs)" },
    };

    public static readonly Dictionary<string, (int roomId, int objectId)> WhiteDoorMap = new()
    {
        { "GT White Door (1st Room)", (3, 9) },
        { "GT White Door (Linus' Map)", (268, 445) },
        { "GT White Door (Tauros)", (62, 413) },
        { "Mech White Door (2nd Room)", (302, 474) },
        { "Mech White Door (Black Knight)", (343, 795) },
        { "Mech White Door (Enemy Arena)", (1214, 6162) },
        { "Mech White Door (Top)", (1633, 1672) },
        { "HotP White Door (1st Room)", (820, 6166) },
        { "HotP White Door (Griffon Claw)", (3012, 6174) },
        { "HotP White Door (Boss)", (2889, 6545) },
        { "RoA White Door (Worms)", (3271, 4138) },
        { "RoA White Door (Ascend)", (3273, 8658) },
        { "RoA White Door (Spike Balls)", (4096, 4709) },
        { "RoA White Door (Spike Spinners)", (4249, 5770) },
        { "RoA White Door (Skippable)", (4305, 5371) },
        { "Cata White Door (Top)", (60, 2262) },
        { "Cata White Door (After Blue Door)", (984, 4444) },
        { "Cata White Door (Prison)", (4336, 4615) },
    };

    public static readonly Dictionary<string, (int roomId, int objectId)> BlueDoorMap = new()
    {
        { "GT Blue Door (Bestiary)", (6628, 9467) },
        { "GT Blue Door (Ring of the Ancients)", (30, 6373) },
        { "GT Blue Door (Bonus Orbs)", (32, 9470) },
        { "GT Blue Door (Ascendant Key)", (38, 444) },
        { "GT Blue Door (Sword of Mirrors)", (477, 6139) },
        { "Mech Blue Door (Red Key)", (1300, 6137) },
        { "Mech Blue Door (Shortcut)", (7235, 8810) },
        { "Mech Blue Door (Music Test)", (324, 6275) },
        { "Mech Blue Door (Talaria Boots)", (336, 6156) },
        { "Mech Blue Door (Void Charm)", (330, 6146) },
        { "Mech Blue Door (Cyclops Den)", (1623, 8808) },
        { "HotP Blue Door (Above Start)", (821, 6165) },
        { "HotP Blue Door (Epimetheus)", (2676, 6344) },
        { "HotP Blue Door (Dead Maiden)", (2890, 6175) },
        { "RoA Blue Door (Flames)", (4173, 8652) },
        { "RoA Blue Door (Blood Pot)", (4106, 8763) },
        { "Apex Blue Door", (7071, 7100) },
        { "Caves Blue Door", (34, 2037) },
        { "Cata Blue Door (Bonus Orbs)", (61, 6996) },
        { "Cata Blue Door (Checkpoint)", (978, 2073) },
        { "Cata Blue Door (Lunarian Bow)", (2416, 6997) },
        { "Cata Blue Door (Poison Roots)", (2607, 6180) },
        { "Cata Blue Door (Prison Cyclops)", (4336, 6184) },
        { "Cata Blue Door (Prison Left)", (4335, 6185) },
        { "Cata Blue Door (Prison Right)", (4335, 6186) },
        { "TR Blue Door", (2706, 8815) },
        { "SP Blue Door", (8088, 8809) },
    };

    public static readonly Dictionary<string, (int roomId, int objectId)> RedDoorMap = new()
    {
        { "Red Door (Zeek)", (3227, 3288) },
        { "Red Door (Cathedral)", (7055, 7252) },
        { "Red Door (Serpent Path)", (5804, 7335) },
        { "Red Door (Tower Roots)", (2706, 8812) },
        { "Red Door (Dev Room)", (2598, 3276) },
    };

    public static readonly Dictionary<string, Checkpoint> Checkpoints = new()
    {
        {
            "Entrance", new()
            {
                Id = -1,
                RoomId = 5,
                PlayerPos = new(4584, -26901, 0),
                CameraPos = new(4584, -26840),
            }
        },
        {
            "Tutorial", new()
            {
                Id = 6696,
                RoomId = 6670,
                PlayerPos = new(5024, -27381, 0),
                CameraPos = new(5016, -27320),
            }
        },
        {
            "GT Bottom", new()
            {
                Id = 18,
                RoomId = 15,
                PlayerPos = new(6744, -26853, 0),
                CameraPos = new(6744, -26840),
            }
        },
        {
            "GT Left", new()
            {
                Id = 292,
                RoomId = 38,
                PlayerPos = new(5080, -26181, 0),
                CameraPos = new(5016, -26120),
            }
        },
        {
            "GT Boss", new()
            {
                Id = 293,
                RoomId = 63,
                PlayerPos = new(5880, -25429, 0),
                CameraPos = new(5880, -25400),
            }
        },
        {
            "Mechanism Start", new()
            {
                Id = 1140,
                RoomId = 304,
                PlayerPos = new(7544, -25493, 0),
                CameraPos = new(7608, -25400),
            }
        },
        {
            "Mechanism Sword", new()
            {
                Id = 1556,
                RoomId = 333,
                PlayerPos = new(8088, -25877, 0),
                CameraPos = new(8040, -25880),
            }
        },
        {
            "Mechanism Bottom", new()
            {
                Id = 813,
                RoomId = 1282,
                PlayerPos = new(8904, -26437, 0),
                CameraPos = new(8904, -26360),
            }
        },
        {
            "Mechanism Shortcut", new()
            {
                Id = 712,
                RoomId = 711,
                PlayerPos = new(7608, -24981, 0),
                CameraPos = new(7608, -24920),
            }
        },
        {
            "Mechanism Right", new()
            {
                Id = 3547,
                RoomId = 3546,
                PlayerPos = new(10136, -24005, 0),
                CameraPos = new(10200, -23960),
            }
        },
        {
            "Mechanism Top", new()
            {
                Id = 1634,
                RoomId = 1633,
                PlayerPos = new(9304, -23093, 0),
                CameraPos = new(9336, -23000),
            }
        },
        {
            "Mechanism Boss", new()
            {
                Id = 819,
                RoomId = 801,
                PlayerPos = new(7560, -23797, 0),
                CameraPos = new(7608, -23720),
            }
        },
        {
            "CD 1", new()
            {
                Id = 7507,
                RoomId = 7360,
                PlayerPos = new(11064, -22837, 0),
                CameraPos = new(11064, -22760),
            }
        },
        {
            "CD 2", new()
            {
                Id = 7577,
                RoomId = 7368,
                PlayerPos = new(10200, -22085, 0),
                CameraPos = new(10200, -22040),
            }
        },
        {
            "CD 3", new()
            {
                Id = 7703,
                RoomId = 7371,
                PlayerPos = new(10200, -21589, 0),
                CameraPos = new(10200, -21560),
            }
        },
        {
            "CD 4", new()
            {
                Id = 7774,
                RoomId = 7772,
                PlayerPos = new(9416, -20917, 0),
                CameraPos = new(9336, -20840),
            }
        },
        {
            "HotP Epimetheus", new()
            {
                Id = 5019,
                RoomId = 5018,
                PlayerPos = new(6312, -24517, 0),
                CameraPos = new(6312, -24440),
            }
        },
        {
            "HotP Bell", new()
            {
                Id = 6421,
                RoomId = 6417,
                PlayerPos = new(4152, -24453, 0),
                CameraPos = new(4152, -24440),
            }
        },
        {
            "HotP Claw", new()
            {
                Id = 3207,
                RoomId = 2901,
                PlayerPos = new(5880, -22757, 0),
                CameraPos = new(5880, -22760),
            }
        },
        {
            "HotP Boss", new()
            {
                Id = 2904,
                RoomId = 2889,
                PlayerPos = new(6312, -21813, 0),
                CameraPos = new(6312, -21800),
            }
        },
        {
            "Cathedral 1", new()
            {
                Id = 10203,
                RoomId = 7266,
                PlayerPos = new(2008, -23477, 0),
                CameraPos = new(1992, -23480),
            }
        },
        {
            "Cathedral 2", new()
            {
                Id = 10260,
                RoomId = 7271,
                PlayerPos = new(1992, -22613, 0),
                CameraPos = new(1992, -22520),
            }
        },
        {
            "RoA Start", new()
            {
                Id = 3726,
                RoomId = 3266,
                PlayerPos = new(8040, -21845, 0),
                CameraPos = new(8040, -21800),
            }
        },
        {
            "RoA Left", new()
            {
                Id = 7088,
                RoomId = 7087,
                PlayerPos = new(5832, -20885, 0),
                CameraPos = new(5880, -20840),
            }
        },
        {
            "RoA Middle", new()
            {
                Id = 7086,
                RoomId = 4080,
                PlayerPos = new(7592, -20437, 0),
                CameraPos = new(7608, -20360),
            }
        },
        {
            "RoA Elevator", new()
            {
                Id = 4685,
                RoomId = 4104,
                PlayerPos = new(7576, -18197, 0),
                CameraPos = new(7608, -18200),
            }
        },
        {
            "RoA Boss", new()
            {
                Id = 10026,
                RoomId = 10016,
                PlayerPos = new(5848, -17557, 0),
                CameraPos = new(5880, -17480),
            }
        },
        {
            "SP 1", new()
            {
                Id = 7436,
                RoomId = 7386,
                PlayerPos = new(3720, -19925, 0),
                CameraPos = new(3720, -19880),
            }
        },
        {
            "SP 2", new()
            {
                Id = 8243,
                RoomId = 7396,
                PlayerPos = new(4184, -21157, 0),
                CameraPos = new(4152, -21080),
            }
        },
        {
            "The Apex", new()
            {
                Id = 4635,
                RoomId = 4246,
                PlayerPos = new(8056, -17317, 0),
                CameraPos = new(8040, -17240),
            }
        },
        {
            "Catacombs Upper", new()
            {
                Id = 7109,
                RoomId = 7042,
                PlayerPos = new(8536, -27877, 0),
                CameraPos = new(8472, -27800),
            }
        },
        {
            "Catacombs Bow", new()
            {
                Id = 2524,
                RoomId = 978,
                PlayerPos = new(7208, -28357, 0),
                CameraPos = new(7176, -28280),
            }
        },
        {
            "Catacombs Roots", new()
            {
                Id = 2610,
                RoomId = 982,
                PlayerPos = new(7608, -29029, 0),
                CameraPos = new(7608, -29000),
            }
        },
        {
            "Catacombs Boss", new()
            {
                Id = 2669,
                RoomId = 2655,
                PlayerPos = new(6696, -29781, 0),
                CameraPos = new(6744, -29720),
            }
        },
        {
            "Tower Roots", new()
            {
                Id = 9056,
                RoomId = 2704,
                PlayerPos = new(5880, -30997, 0),
                CameraPos = new(5880, -30920),
            }
        },
        {
            "Dev Room", new()
            {
                Id = 9161,
                RoomId = 2779,
                PlayerPos = new(11968, -28341, 0),
                CameraPos = new(11928, -28280),
            }
        },
    };

    public static readonly Dictionary<DealProperties.DealID, string> DealToLocation = new()
    {
        { DealProperties.DealID.Deal_Knowledge, "Shop - Knowledge" },
        { DealProperties.DealID.Deal_OrbReaper, "Shop - Orb Seeker" },
        { DealProperties.DealID.Deal_TitanEgo, "Shop - Titan's Ego" },
        { DealProperties.DealID.Deal_MapReveal, "Shop - Map Reveal" },
        { DealProperties.DealID.Deal_Gift, "Shop - Gift" },
        { DealProperties.DealID.Deal_LockedDoors, "Shop - Cartographer" },
        { DealProperties.DealID.Deal_DeathOrb, "Shop - Death Orb" },
        { DealProperties.DealID.Deal_MapDeath, "Shop - Death Point" },
        { DealProperties.DealID.Deal_Mercy, "Shop - Mercy" },
        { DealProperties.DealID.Deal_Algus_Arcanist, "Shop - Algus's Arcanist" },
        { DealProperties.DealID.Deal_Algus_DamageField, "Shop - Algus's Shock Field" },
        { DealProperties.DealID.Deal_Algus_MeteorRain, "Shop - Algus's Meteor Rain" },
        { DealProperties.DealID.Deal_Arias_Gorgonslayer, "Shop - Arias's Gorgonslayer" },
        { DealProperties.DealID.Deal_Arias_LastStand, "Shop - Arias's Last Stand" },
        { DealProperties.DealID.Deal_Arias_Lionheart, "Shop - Arias's Lionheart" },
        { DealProperties.DealID.Deal_Kyuli_AssassinStrike, "Shop - Kyuli's Assassin Strike" },
        { DealProperties.DealID.Deal_Kyuli_Bullseye, "Shop - Kyuli's Bullseye" },
        { DealProperties.DealID.Deal_Kyuli_ShiningRay, "Shop - Kyuli's Shining Ray" },
        { DealProperties.DealID.Deal_Zeek_JunkyardHunt, "Shop - Zeek's Junkyard Hunt" },
        { DealProperties.DealID.Deal_Zeek_GoldDigger, "Shop - Zeek's Orb Monger" },
        { DealProperties.DealID.Deal_Zeek_BiggerLoot, "Shop - Zeek's Bigger Loot" },
        { DealProperties.DealID.Deal_Bram_GoldenAxe, "Shop - Bram's Golden Axe" },
        { DealProperties.DealID.Deal_Bram_MonsterHunter, "Shop - Bram's Monster Hunter" },
        { DealProperties.DealID.Deal_Bram_Whiplash, "Shop - Bram's Whiplash" },
    };

    public static readonly Dictionary<string, DealProperties.DealID> ItemToDeal = new()
    {
        { "Knowledge", DealProperties.DealID.Deal_Knowledge },
        { "Orb Seeker", DealProperties.DealID.Deal_OrbReaper },
        { "Titan's Ego", DealProperties.DealID.Deal_TitanEgo },
        { "Map Reveal", DealProperties.DealID.Deal_MapReveal },
        { "Gift", DealProperties.DealID.Deal_Gift },
        { "Cartographer", DealProperties.DealID.Deal_LockedDoors },
        { "Death Orb", DealProperties.DealID.Deal_DeathOrb },
        { "Death Point", DealProperties.DealID.Deal_MapDeath },
        { "Mercy", DealProperties.DealID.Deal_Mercy },
        { "Algus's Arcanist", DealProperties.DealID.Deal_Algus_Arcanist },
        { "Algus's Shock Field", DealProperties.DealID.Deal_Algus_DamageField },
        { "Algus's Meteor Rain", DealProperties.DealID.Deal_Algus_MeteorRain },
        { "Arias's Gorgonslayer", DealProperties.DealID.Deal_Arias_Gorgonslayer },
        { "Arias's Last Stand", DealProperties.DealID.Deal_Arias_LastStand },
        { "Arias's Lionheart", DealProperties.DealID.Deal_Arias_Lionheart },
        { "Kyuli's Assassin Strike", DealProperties.DealID.Deal_Kyuli_AssassinStrike },
        { "Kyuli's Bullseye", DealProperties.DealID.Deal_Kyuli_Bullseye },
        { "Kyuli's Shining Ray", DealProperties.DealID.Deal_Kyuli_ShiningRay },
        { "Zeek's Junkyard Hunt", DealProperties.DealID.Deal_Zeek_JunkyardHunt },
        { "Zeek's Orb Monger", DealProperties.DealID.Deal_Zeek_GoldDigger },
        { "Zeek's Bigger Loot", DealProperties.DealID.Deal_Zeek_BiggerLoot },
        { "Bram's Golden Axe", DealProperties.DealID.Deal_Bram_GoldenAxe },
        { "Bram's Monster Hunter", DealProperties.DealID.Deal_Bram_MonsterHunter },
        { "Bram's Whiplash", DealProperties.DealID.Deal_Bram_Whiplash },
    };

    public static readonly Dictionary<string, CharacterProperties.Character> ItemToCharacter = new()
    {
        { "Algus", CharacterProperties.Character.Algus },
        { "Arias", CharacterProperties.Character.Arias },
        { "Kyuli", CharacterProperties.Character.Kyuli },
        { "Zeek", CharacterProperties.Character.Zeek },
        { "Bram", CharacterProperties.Character.Bram },
    };

    public static readonly Dictionary<CharacterProperties.Character, string> CharacterToItem =
        ItemToCharacter.ToDictionary((kvp) => kvp.Value, (kvp) => kvp.Key);

    public static readonly SwitchData[] Switches =
    [
        new()
        {
            Id = "1",
            RoomId = 3,
            ObjectsToEnable = [],
            ObjectsToDisable = [12],
            ItemName = "GT Switch 2nd Room",
            LocationName = "Gorgon Tomb - Switch (2nd Room)",
        },
        new()
        {
            Id = "2",
            RoomId = 32,
            ObjectsToEnable = [],
            ObjectsToDisable = [246],
            ItemName = "GT Switch 1st Cyclops",
            LocationName = "Gorgon Tomb - Switch (1st Cyclops)",
        },
        new()
        {
            Id = "3",
            RoomId = 47,
            ObjectsToEnable = [],
            ObjectsToDisable = [349],
            ItemName = "GT Switch Spike Tunnel",
            LocationName = "Gorgon Tomb - Switch (Spike Tunnel Access)",
        },
        new()
        {
            Id = "4",
            RoomId = 48,
            ObjectsToEnable = [],
            ObjectsToDisable = [346],
            ItemName = "GT Switch Butt Access",
            LocationName = "Gorgon Tomb - Switch (Butt Access)",
        },
        new()
        {
            Id = "5",
            RoomId = 35,
            ObjectsToEnable = [431],
            ObjectsToDisable = [],
            ItemName = "GT Switch Gorgonheart",
            LocationName = "Gorgon Tomb - Switch (Gorgonheart)",
        },
        new()
        {
            Id = "9",
            RoomId = 303,
            ObjectsToEnable = [],
            ObjectsToDisable = [486],
            ItemName = "Mech Switch Watcher",
            LocationName = "Mechanism - Switch (Watcher)",
        },
        new()
        {
            Id = "10",
            RoomId = 335,
            ObjectsToEnable = [],
            ObjectsToDisable = [535],
            ItemName = "GT Crystal Ladder",
            LocationName = "Gorgon Tomb - Crystal (Ladder)",
        },
        new()
        {
            Id = "11",
            RoomId = 302,
            ObjectsToEnable = [],
            ObjectsToDisable = [538],
            ItemName = "Mech Crystal Cannon",
            LocationName = "Mechanism - Crystal (Cannon)",
        },
        new()
        {
            Id = "12",
            RoomId = 30,
            ObjectsToEnable = [],
            ObjectsToDisable = [548],
            ItemName = "GT Switch RotA",
            LocationName = "Gorgon Tomb - Switch (Ring of the Ancients)",
        },
        new()
        {
            Id = "16",
            RoomId = 74,
            ObjectsToEnable = [895, 896],
            ObjectsToDisable = [],
            ItemName = "GT Switch Upper Path Blocks",
            LocationName = "Gorgon Tomb - Switch (Upper Path Blocks)",
        },
        new()
        {
            Id = "17",
            RoomId = 74,
            ObjectsToEnable = [],
            ObjectsToDisable = [677],
            ItemName = "GT Switch Upper Path Access",
            LocationName = "Gorgon Tomb - Switch (Upper Path Access)",
        },
        new()
        {
            Id = "17",
            RoomId = 343,
            ObjectsToEnable = [],
            ObjectsToDisable = [830],
            ItemName = "Mech Switch Chains",
            LocationName = "Mechanism - Switch (Chains)",
        },
        new()
        {
            Id = "19",
            RoomId = 65,
            ObjectsToEnable = [],
            ObjectsToDisable = [9204],
            ItemName = "HotP Switch Rock",
            LocationName = "Hall of the Phantoms - Switch (Rock)",
        },
        new()
        {
            Id = "20",
            RoomId = 2812,
            ObjectsToEnable = [],
            ObjectsToDisable = [889],
            ItemName = "HotP Switch Below Start",
            LocationName = "Hall of the Phantoms - Switch (Below Start)",
        },
        new()
        {
            Id = "21",
            RoomId = 800,
            ObjectsToEnable = [],
            ObjectsToDisable = [989],
            ItemName = "Mech Switch Boss 1",
            LocationName = "Mechanism - Switch (Boss Access 1)",
        },
        new()
        {
            Id = "22",
            RoomId = 800,
            ObjectsToEnable = [],
            ObjectsToDisable = [990],
            ItemName = "Mech Switch Boss 2",
            LocationName = "Mechanism - Switch (Boss Access 2)",
        },
        new()
        {
            Id = "23",
            RoomId = 36,
            ObjectsToEnable = [],
            ObjectsToDisable = [1032],
            ItemName = "GT Switch Crosses",
            LocationName = "Gorgon Tomb - Switch (Crosses)",
        },
        new()
        {
            Id = "24",
            RoomId = 806,
            ObjectsToEnable = [],
            ObjectsToDisable = [1050],
            ItemName = "Mech Switch Split Path",
            LocationName = "Mechanism - Switch (Split Path)",
        },
        new()
        {
            Id = "25",
            RoomId = 1,
            ObjectsToEnable = [1066, 1067],
            ObjectsToDisable = [],
            ItemName = "GT Switch GH Shortcut",
            LocationName = "Gorgon Tomb - Switch (Gorgonheart Shortcut)",
        },
        new()
        {
            Id = "25",
            RoomId = 65,
            ObjectsToEnable = [],
            ObjectsToDisable = [987],
            ItemName = "HotP Crystal Rock Access",
            LocationName = "Hall of the Phantoms - Crystal (Rock Access)",
        },
        new()
        {
            Id = "26",
            RoomId = 46,
            ObjectsToEnable = [1075, 6131],
            ObjectsToDisable = [],
            ItemName = "GT Switch Arias",
            LocationName = "Gorgon Tomb - Switch (Arias's Path)",
        },
        new()
        {
            Id = "28",
            RoomId = 328,
            ObjectsToEnable = [],
            ObjectsToDisable = [1113],
            ItemName = "Mech Switch Snake 1",
            LocationName = "Mechanism - Switch (Snake 1)",
        },
        new()
        {
            Id = "30",
            RoomId = 331,
            ObjectsToEnable = [],
            ObjectsToDisable = [1185],
            ItemName = "Mech Switch Boots",
            LocationName = "Mechanism - Switch (Boots Access)",
        },
        new()
        {
            Id = "31",
            RoomId = 871,
            ObjectsToEnable = [],
            ObjectsToDisable = [1198],
            ItemName = "HotP Switch Left 2",
            LocationName = "Hall of the Phantoms - Switch (Left 2)",
        },
        new()
        {
            Id = "32",
            RoomId = 871,
            ObjectsToEnable = [],
            ObjectsToDisable = [1200],
            ItemName = "HotP Switch Left 1",
            LocationName = "Hall of the Phantoms - Switch (Left 1)",
        },
        new()
        {
            Id = "33",
            RoomId = 667,
            ObjectsToEnable = [],
            ObjectsToDisable = [1283],
            ItemName = "Mech Crystal Linus",
            LocationName = "Mechanism - Crystal (Linus)",
        },
        new()
        {
            Id = "34",
            RoomId = 339,
            ObjectsToEnable = [],
            ObjectsToDisable = [1302, 5719],
            ItemName = "Mech Switch to Upper GT",
            LocationName = "Mechanism - Switch (Upper GT Access)",
        },
        new()
        {
            Id = "35",
            RoomId = 1316,
            ObjectsToEnable = [],
            ObjectsToDisable = [1327, 1330, 1331, 1332],
            ItemName = "Mech Switch Upper Void Drop",
            LocationName = "Mechanism - Switch (Upper Void Drop)",
        },
        new()
        {
            Id = "37",
            RoomId = 811,
            ObjectsToEnable = [],
            ObjectsToDisable = [1383],
            ItemName = "Mech Switch Upper Void",
            LocationName = "Mechanism - Switch (Upper Void)",
        },
        new()
        {
            Id = "38",
            RoomId = 667,
            ObjectsToEnable = [],
            ObjectsToDisable = [1418],
            ItemName = "Mech Switch Linus",
            LocationName = "Mechanism - Switch (Linus)",
        },
        new()
        {
            Id = "40",
            RoomId = 336,
            ObjectsToEnable = [1441, 1442, 1443],
            ObjectsToDisable = [],
            ItemName = "Mech Crystal Lower",
            LocationName = "Mechanism - Crystal (Lower)",
        },
        new()
        {
            Id = "43",
            RoomId = 799,
            ObjectsToEnable = [1527, 1528, 1525, 1526, 1523, 1524, 1529, 1532, 1521, 1522, 1530, 1531],
            ObjectsToDisable = [],
            ItemName = "Mech Crystal To Boss 3",
            LocationName = "Mechanism - Crystal (To Boss 3)",
        },
        new()
        {
            Id = "44",
            RoomId = 73,
            ObjectsToEnable = [],
            ObjectsToDisable = [1558],
            ItemName = "GT Switch Sword Access",
            LocationName = "Gorgon Tomb - Switch (Sword Access)",
        },
        new()
        {
            Id = "45",
            RoomId = 73,
            ObjectsToEnable = [1561, 2359],
            ObjectsToDisable = [],
            ItemName = "GT Switch Sword Backtrack",
            LocationName = "Gorgon Tomb - Switch (Sword Backtrack)",
        },
        new()
        {
            Id = "46",
            RoomId = 799,
            ObjectsToEnable = [],
            ObjectsToDisable = [1605],
            ItemName = "Mech Switch To Boss 2",
            LocationName = "Mechanism - Switch (To Boss 2)",
        },
        new()
        {
            Id = "47",
            RoomId = 1216,
            ObjectsToEnable = [],
            ObjectsToDisable = [6380, 1619],
            ItemName = "Mech Switch Pots",
            LocationName = "Mechanism - Switch (Pots)",
        },
        new()
        {
            Id = "48",
            RoomId = 1622,
            ObjectsToEnable = [],
            ObjectsToDisable = [1620],
            ItemName = "HotP Switch Lower Shortcut",
            LocationName = "Hall of the Phantoms - Switch (Lower Shortcut)",
        },
        new()
        {
            Id = "50",
            RoomId = 1632,
            ObjectsToEnable = [1642, 1640, 1638, 1637, 1639, 1641],
            ObjectsToDisable = [],
            ItemName = "Mech Switch Maze Backdoor",
            LocationName = "Mechanism - Switch (Maze Backdoor)",
        },
        new()
        {
            Id = "51",
            RoomId = 1630,
            ObjectsToEnable = [],
            ObjectsToDisable = [1714],
            ItemName = "Mech Crystal Triple 1",
            LocationName = "Mechanism - Crystal (Triple 1)",
        },
        new()
        {
            Id = "52",
            RoomId = 1630,
            ObjectsToEnable = [],
            ObjectsToDisable = [1715],
            ItemName = "Mech Crystal Triple 2",
            LocationName = "Mechanism - Crystal (Triple 2)",
        },
        new()
        {
            Id = "53",
            RoomId = 1630,
            ObjectsToEnable = [],
            ObjectsToDisable = [1716],
            ItemName = "Mech Crystal Triple 3",
            LocationName = "Mechanism - Crystal (Triple 3)",
        },
        new()
        {
            Id = "54",
            RoomId = 799,
            ObjectsToEnable = [],
            ObjectsToDisable = [1722, 1723],
            ItemName = "Mech Switch To Boss 1",
            LocationName = "Mechanism - Switch (To Boss 1)",
        },
        new()
        {
            Id = "55",
            RoomId = 1633,
            ObjectsToEnable = [1743],
            ObjectsToDisable = [],
            ItemName = "Mech Crystal Top",
            LocationName = "Mechanism - Crystal (Top)",
        },
        new()
        {
            Id = "56",
            RoomId = 34,
            ObjectsToEnable = [1910, 1911, 2612, 2613],
            ObjectsToDisable = [],
            ItemName = "Caves Face 1st Room",
            LocationName = "Caves - Face (1st Room)",
        },
        new()
        {
            Id = "57",
            RoomId = 1353,
            ObjectsToEnable = [1934, 6742, 6743, 6744],
            ObjectsToDisable = [],
            ItemName = "Mech Switch Block Stairs",
            LocationName = "Mechanism - Switch (Block Stairs)",
        },
        new()
        {
            Id = "58",
            RoomId = 53,
            ObjectsToEnable = [],
            ObjectsToDisable = [2050],
            ItemName = "Caves Switch Skeletons",
            LocationName = "Caves - Switch (Skeletons)",
        },
        new()
        {
            Id = "59",
            RoomId = 54,
            ObjectsToEnable = [],
            ObjectsToDisable = [2059],
            ItemName = "Caves Switch Cata 1",
            LocationName = "Caves - Switch (Catacombs Access 1)",
        },
        new()
        {
            Id = "60",
            RoomId = 54,
            ObjectsToEnable = [],
            ObjectsToDisable = [2061],
            ItemName = "Caves Switch Cata 2",
            LocationName = "Caves - Switch (Catacombs Access 2)",
        },
        new()
        {
            Id = "61",
            RoomId = 54,
            ObjectsToEnable = [],
            ObjectsToDisable = [2063],
            ItemName = "Caves Switch Cata 3",
            LocationName = "Caves - Switch (Catacombs Access 3)",
        },
        new()
        {
            Id = "62",
            RoomId = 66,
            ObjectsToEnable = [2142],
            ObjectsToDisable = [],
            ItemName = "GT Switch Sword",
            LocationName = "Gorgon Tomb - Switch (Sword)",
        },
        new()
        {
            Id = "65",
            RoomId = 1631,
            ObjectsToEnable = [],
            ObjectsToDisable = [2278],
            ItemName = "Mech Switch Arias Cyclops",
            LocationName = "Mechanism - Switch (Arias Cyclops)",
        },
        new()
        {
            Id = "65",
            RoomId = 713,
            ObjectsToEnable = [],
            ObjectsToDisable = [2367, 2688, 2362, 2363, 2364, 2365, 2366],
            ItemName = "Mech Switch Boots Lower",
            LocationName = "Mechanism - Switch (Boots Lower)",
        },
        new()
        {
            Id = "66",
            RoomId = 797,
            ObjectsToEnable = [1354, 1357, 1358],
            ObjectsToDisable = [],
            ItemName = "Mech Switch Chains Gap",
            LocationName = "Mechanism - Switch (Chains Gap)",
        },
        new()
        {
            Id = "67",
            RoomId = 338,
            ObjectsToEnable = [],
            ObjectsToDisable = [2378],
            ItemName = "Mech Switch Lower Key",
            LocationName = "Mechanism - Switch (Lower Key)",
        },
        new()
        {
            Id = "68",
            RoomId = 58,
            ObjectsToEnable = [],
            ObjectsToDisable = [2415],
            ItemName = "Cata Switch Elevator",
            LocationName = "Catacombs - Switch (Elevator)",
        },
        new()
        {
            Id = "69",
            RoomId = 979,
            ObjectsToEnable = [2437, 2435, 2436, 2434],
            ObjectsToDisable = [],
            ItemName = "Cata Face After Bow",
            LocationName = "Catacombs - Face (After Bow)",
        },
        new()
        {
            Id = "70",
            RoomId = 2416,
            ObjectsToEnable = [],
            ObjectsToDisable = [2471],
            ItemName = "Cata Switch Vertical Shortcut",
            LocationName = "Catacombs - Switch (Vertical Shortcut)",
        },
        new()
        {
            Id = "71",
            RoomId = 977,
            ObjectsToEnable = [],
            ObjectsToDisable = [2504, 2505, 7065],
            ItemName = "Cata Switch Top",
            LocationName = "Catacombs - Switch (Top)",
        },
        new()
        {
            Id = "72",
            RoomId = 981,
            ObjectsToEnable = [],
            ObjectsToDisable = [2533],
            ItemName = "Cata Switch Claw 1",
            LocationName = "Catacombs - Switch (Claw 1)",
        },
        new()
        {
            Id = "73",
            RoomId = 981,
            ObjectsToEnable = [],
            ObjectsToDisable = [2535],
            ItemName = "Cata Switch Claw 2",
            LocationName = "Catacombs - Switch (Claw 2)",
        },
        new()
        {
            Id = "74",
            RoomId = 2531,
            ObjectsToEnable = [],
            ObjectsToDisable = [2537],
            ItemName = "Cata Switch Water 1",
            LocationName = "Catacombs - Switch (Water 1)",
        },
        new()
        {
            Id = "75",
            RoomId = 2531,
            ObjectsToEnable = [],
            ObjectsToDisable = [2538],
            ItemName = "Cata Switch Water 2",
            LocationName = "Catacombs - Switch (Water 2)",
        },
        new()
        {
            Id = "76",
            RoomId = 2573,
            ObjectsToEnable = [2584],
            ObjectsToDisable = [],
            ItemName = "Cata Switch Dev Room",
            LocationName = "Catacombs - Switch (Dev Room)",
        },
        new()
        {
            Id = "80",
            RoomId = 828,
            ObjectsToEnable = [],
            ObjectsToDisable = [2847],
            ItemName = "HotP Crystal Bottom",
            LocationName = "Hall of the Phantoms - Crystal (Bottom)",
        },
        new()
        {
            Id = "81",
            RoomId = 1622,
            ObjectsToEnable = [2855, 2857, 2853, 2856],
            ObjectsToDisable = [],
            ItemName = "HotP Crystal Lower",
            LocationName = "Hall of the Phantoms - Crystal (Lower)",
        },
        new()
        {
            Id = "82",
            RoomId = 2802,
            ObjectsToEnable = [2868, 2869, 2864, 2867],
            ObjectsToDisable = [],
            ItemName = "HotP Switch Bell",
            LocationName = "Hall of the Phantoms - Switch (Bell)",
        },
        new()
        {
            Id = "84",
            RoomId = 2892,
            ObjectsToEnable = [],
            ObjectsToDisable = [2907],
            ItemName = "HotP Switch Ghost Blood",
            LocationName = "Hall of the Phantoms - Switch (Ghost Blood)",
        },
        new()
        {
            Id = "88",
            RoomId = 2891,
            ObjectsToEnable = [],
            ObjectsToDisable = [3024],
            ItemName = "HotP Switch Teleports",
            LocationName = "Hall of the Phantoms - Switch (Teleports)",
        },
        new()
        {
            Id = "97",
            RoomId = 2893,
            ObjectsToEnable = [],
            ObjectsToDisable = [3072],
            ItemName = "HotP Switch Worm Pillar",
            LocationName = "Hall of the Phantoms - Switch (Worm Pillar)",
        },
        new()
        {
            Id = "101",
            RoomId = 3216,
            ObjectsToEnable = [],
            ObjectsToDisable = [3213],
            ItemName = "HotP Crystal After Claw",
            LocationName = "Hall of the Phantoms - Crystal (After Claw)",
        },
        new()
        {
            Id = "102",
            RoomId = 2905,
            ObjectsToEnable = [],
            ObjectsToDisable = [3224],
            ItemName = "Mech Switch Arias",
            LocationName = "Mechanism - Switch (Arias)",
        },
        new()
        {
            Id = "104",
            RoomId = 3091,
            ObjectsToEnable = [],
            ObjectsToDisable = [3314],
            ItemName = "Mech Crystal Cloak",
            LocationName = "Mechanism - Crystal (Cloak)",
        },
        new()
        {
            Id = "108",
            RoomId = 1932,
            ObjectsToEnable = [3543, 3542],
            ObjectsToDisable = [],
            ItemName = "Mech Crystal Slimes",
            LocationName = "Mechanism - Crystal (Slimes)",
        },
        new()
        {
            Id = "110",
            RoomId = 3650,
            ObjectsToEnable = [3655, 3653],
            ObjectsToDisable = [],
            ItemName = "GT Crystal Old Man 1",
            LocationName = "Gorgon Tomb - Crystal (Old Man 1)",
        },
        new()
        {
            Id = "111",
            RoomId = 3650,
            ObjectsToEnable = [3654],
            ObjectsToDisable = [],
            ItemName = "GT Crystal Old Man 2",
            LocationName = "Gorgon Tomb - Crystal (Old Man 2)",
        },
        new()
        {
            Id = "111",
            RoomId = 3617,
            ObjectsToEnable = [],
            ObjectsToDisable = [3670, 3671, 3672, 3673, 3674],
            ItemName = "HotP Face Old Man",
            LocationName = "Hall of the Phantoms - Face (Old Man)",
        },
        new()
        {
            Id = "117",
            RoomId = 2472,
            ObjectsToEnable = [9519, 9520, 9517, 9518, 9515, 9516, 9513, 9514],
            ObjectsToDisable = [],
            ItemName = "Cata Face Bow",
            LocationName = "Catacombs - Face (Bow)",
        },
        new()
        {
            Id = "118",
            RoomId = 1054,
            ObjectsToEnable = [],
            ObjectsToDisable = [4120],
            ItemName = "RoA Crystal 1st Room",
            LocationName = "Ruins of Ash - Crystal (1st Room)",
        },
        new()
        {
            Id = "119",
            RoomId = 3938,
            ObjectsToEnable = [],
            ObjectsToDisable = [4171],
            ItemName = "RoA Crystal Baby Gorgon",
            LocationName = "Ruins of Ash - Crystal (Baby Gorgon)",
        },
        new()
        {
            Id = "121",
            RoomId = 3273,
            ObjectsToEnable = [4244],
            ObjectsToDisable = [],
            ItemName = "RoA Switch Ascend",
            LocationName = "Ruins of Ash - Switch (Ascend)",
        },
        new()
        {
            Id = "124",
            RoomId = 984,
            ObjectsToEnable = [4319],
            ObjectsToDisable = [],
            ItemName = "Cata Switch After Blue Door",
            LocationName = "Catacombs - Switch (After Blue Door)",
        },
        new()
        {
            Id = "125",
            RoomId = 2672,
            ObjectsToEnable = [],
            ObjectsToDisable = [4331],
            ItemName = "Cata Face x4",
            LocationName = "Catacombs - Face (x4)",
        },
        new()
        {
            Id = "126",
            RoomId = 2655,
            ObjectsToEnable = [],
            ObjectsToDisable = [4339, 4340, 4341, 4342],
            ItemName = "Cata Face Campfire",
            LocationName = "Catacombs - Face (Campfire)",
        },
        new()
        {
            Id = "127",
            RoomId = 2674,
            ObjectsToEnable = [],
            ObjectsToDisable = [4384],
            ItemName = "Cata Face Double Door",
            LocationName = "Catacombs - Face (Double Door)",
        },
        new()
        {
            Id = "128",
            RoomId = 3272,
            ObjectsToEnable = [],
            ObjectsToDisable = [4447, 4448],
            ItemName = "RoA Switch After Worms",
            LocationName = "Ruins of Ash - Switch (After Worms)",
        },
        new()
        {
            Id = "130",
            RoomId = 4085,
            ObjectsToEnable = [4466, 4467],
            ObjectsToDisable = [],
            ItemName = "RoA Crystal Ladder Right",
            LocationName = "Ruins of Ash - Crystal (Ladder Right)",
        },
        new()
        {
            Id = "131",
            RoomId = 4085,
            ObjectsToEnable = [4468, 4469],
            ObjectsToDisable = [],
            ItemName = "RoA Crystal Ladder Left",
            LocationName = "Ruins of Ash - Crystal (Ladder Left)",
        },
        new()
        {
            Id = "132",
            RoomId = 4084,
            ObjectsToEnable = [],
            ObjectsToDisable = [4487, 4488],
            ItemName = "RoA Switch Right Path",
            LocationName = "Ruins of Ash - Switch (Right Path)",
        },
        new()
        {
            Id = "134",
            RoomId = 2653,
            ObjectsToEnable = [],
            ObjectsToDisable = [4548],
            ItemName = "Cata Switch Shortcut Access",
            LocationName = "Catacombs - Switch (Shortcut Access)",
        },
        new()
        {
            Id = "135",
            RoomId = 2653,
            ObjectsToEnable = [],
            ObjectsToDisable = [4549, 4552, 4553],
            ItemName = "Cata Switch Ladder Blocks",
            LocationName = "Catacombs - Switch (Ladder Blocks)",
        },
        new()
        {
            Id = "136",
            RoomId = 4108,
            ObjectsToEnable = [],
            ObjectsToDisable = [4639, 10020, 10021, 10022],
            ItemName = "RoA Switch Apex Access",
            LocationName = "Ruins of Ash - Switch (Apex Access)",
        },
        new()
        {
            Id = "138",
            RoomId = 4107,
            ObjectsToEnable = [],
            ObjectsToDisable = [4663, 4664, 4665, 4666, 4667, 4668, 4669, 4670],
            ItemName = "RoA Crystal Centaur",
            LocationName = "Ruins of Ash - Crystal (Centaur)",
        },
        new()
        {
            Id = "139",
            RoomId = 4101,
            ObjectsToEnable = [],
            ObjectsToDisable = [4677, 4682, 4681, 4683],
            ItemName = "RoA Switch Icarus",
            LocationName = "Ruins of Ash - Switch (Icarus Emblem)",
        },
        new()
        {
            Id = "140",
            RoomId = 4093,
            ObjectsToEnable = [4694],
            ObjectsToDisable = [],
            ItemName = "RoA Switch Shaft Left",
            LocationName = "Ruins of Ash - Switch (Shaft Left)",
        },
        new()
        {
            Id = "141",
            RoomId = 4093,
            ObjectsToEnable = [4695],
            ObjectsToDisable = [],
            ItemName = "RoA Switch Shaft Right",
            LocationName = "Ruins of Ash - Switch (Shaft Right)",
        },
        new()
        {
            Id = "144",
            RoomId = 4105,
            ObjectsToEnable = [4741],
            ObjectsToDisable = [9465, 4761, 4759, 4760, 4758, 4757, 4756],
            ItemName = "RoA Switch Elevator",
            LocationName = "Ruins of Ash - Switch (Elevator)",
        },
        new()
        {
            Id = "145",
            RoomId = 4096,
            ObjectsToEnable = [4848, 4849, 4850, 4851, 4852, 4853, 4854],
            ObjectsToDisable = [],
            ItemName = "RoA Crystal Spike Balls",
            LocationName = "Ruins of Ash - Crystal (Spike Balls)",
        },
        new()
        {
            Id = "147",
            RoomId = 4097,
            ObjectsToEnable = [4889, 4890, 4891, 4892, 4893, 10476, 10477, 10478, 10479, 10480],
            ObjectsToDisable = [],
            ItemName = "RoA Switch Shaft Downwards",
            LocationName = "Ruins of Ash - Switch (Shaft Downwards)",
        },
        new()
        {
            Id = "148",
            RoomId = 4247,
            ObjectsToEnable = [],
            ObjectsToDisable = [4935, 4934, 4933],
            ItemName = "RoA Switch Spiders Top",
            LocationName = "Ruins of Ash - Switch (Spiders Top)",
        },
        new()
        {
            Id = "149",
            RoomId = 4248,
            ObjectsToEnable = [],
            ObjectsToDisable = [4943, 4944, 4945, 4946],
            ItemName = "RoA Switch Spiders Bottom",
            LocationName = "Ruins of Ash - Switch (Spiders Bottom)",
        },
        new()
        {
            Id = "156",
            RoomId = 4305,
            ObjectsToEnable = [9931, 5375, 9930],
            ObjectsToDisable = [5376, 5372, 5374],
            ItemName = "RoA Switch Dark Room",
            LocationName = "Ruins of Ash - Switch (Dark Room)",
        },
        new()
        {
            Id = "160",
            RoomId = 4620,
            ObjectsToEnable = [4253, 4254, 4251, 4252],
            ObjectsToDisable = [],
            ItemName = "Apex Switch",
            LocationName = "The Apex - Switch",
        },
        new()
        {
            Id = "161",
            RoomId = 2896,
            ObjectsToEnable = [],
            ObjectsToDisable = [5568],
            ItemName = "HotP Switch To Claw 1",
            LocationName = "Hall of the Phantoms - Switch (To Claw 1)",
        },
        new()
        {
            Id = "162",
            RoomId = 2896,
            ObjectsToEnable = [5573, 5574, 5575, 10461, 10460],
            ObjectsToDisable = [],
            ItemName = "HotP Switch To Claw 2",
            LocationName = "Hall of the Phantoms - Switch (To Claw 2)",
        },
        new()
        {
            Id = "163",
            RoomId = 3940,
            ObjectsToEnable = [],
            ObjectsToDisable = [10483, 10485, 10484, 10486, 5812, 8662, 8664, 8661, 8663, 8665],
            ItemName = "RoA Switch Ascend Shortcut",
            LocationName = "Ruins of Ash - Switch (Ascend Shortcut)",
        },
        new()
        {
            Id = "164",
            RoomId = 3196,
            ObjectsToEnable = [],
            ObjectsToDisable = [5834, 5835],
            ItemName = "HotP Switch Claw Access",
            LocationName = "Hall of the Phantoms - Switch (Claw Access)",
        },
        new()
        {
            Id = "165",
            RoomId = 874,
            ObjectsToEnable = [],
            ObjectsToDisable = [5853],
            ItemName = "HotP Switch Ghosts",
            LocationName = "Hall of the Phantoms - Switch (Ghosts)",
        },
        new()
        {
            Id = "171",
            RoomId = 328,
            ObjectsToEnable = [],
            ObjectsToDisable = [6155],
            ItemName = "Mech Switch Snake 2",
            LocationName = "Mechanism - Switch (Snake 2)",
        },
        new()
        {
            Id = "173",
            RoomId = 3727,
            ObjectsToEnable = [3730, 3731, 3732, 3733],
            ObjectsToDisable = [],
            ItemName = "Mech Crystal To CD",
            LocationName = "Mechanism - Crystal (To CD)",
        },
        new()
        {
            Id = "175",
            RoomId = 797,
            ObjectsToEnable = [],
            ObjectsToDisable = [6321, 6322, 6323, 6324, 6320, 6319, 6318],
            ItemName = "Mech Switch Key Blocks",
            LocationName = "Mechanism - Switch (Key Blocks)",
        },
        new()
        {
            Id = "176",
            RoomId = 871,
            ObjectsToEnable = [],
            ObjectsToDisable = [6333, 6336, 6337, 6338, 6339, 6340],
            ItemName = "HotP Switch Left 3",
            LocationName = "Hall of the Phantoms - Switch (Left 3)",
        },
        new()
        {
            Id = "178",
            RoomId = 302,
            ObjectsToEnable = [],
            ObjectsToDisable = [6404],
            ItemName = "Mech Switch Cannon",
            LocationName = "Mechanism - Switch (Cannon)",
        },
        new()
        {
            Id = "181",
            RoomId = 3666,
            ObjectsToEnable = [],
            ObjectsToDisable = [6431],
            ItemName = "HotP Switch Above Old Man",
            LocationName = "Hall of the Phantoms - Switch (Above Old Man)",
        },
        new()
        {
            Id = "182",
            RoomId = 3624,
            ObjectsToEnable = [6445, 6447],
            ObjectsToDisable = [],
            ItemName = "HotP Crystal Maiden 1",
            LocationName = "Hall of the Phantoms - Crystal (Dead Maiden 1)",
        },
        new()
        {
            Id = "183",
            RoomId = 3625,
            ObjectsToEnable = [],
            ObjectsToDisable = [6467],
            ItemName = "HotP Crystal Maiden 2",
            LocationName = "Hall of the Phantoms - Crystal (Dead Maiden 2)",
        },
        new()
        {
            Id = "184",
            RoomId = 2898,
            ObjectsToEnable = [6529, 6527, 6528, 6530],
            ObjectsToDisable = [],
            ItemName = "HotP Switch To Above Old Man",
            LocationName = "Hall of the Phantoms - Switch (To Above Old Man)",
        },
        new()
        {
            Id = "186",
            RoomId = 3256,
            ObjectsToEnable = [],
            ObjectsToDisable = [6548],
            ItemName = "HotP Switch TP Puzzle",
            LocationName = "Hall of the Phantoms - Switch (Teleport Puzzle)",
        },
        new()
        {
            Id = "190",
            RoomId = 2874,
            ObjectsToEnable = [],
            ObjectsToDisable = [9801, 9432, 9800, 9431],
            ItemName = "Mech Face Volantis",
            LocationName = "Mechanism - Face (Above Volantis)",
        },
        new()
        {
            Id = "191",
            RoomId = 2892,
            ObjectsToEnable = [6695],
            ObjectsToDisable = [6694, 6693, 6692, 6691],
            ItemName = "HotP Switch Eyeball Shortcut",
            LocationName = "Hall of the Phantoms - Switch (Eyeball Shortcut)",
        },
        new()
        {
            Id = "196",
            RoomId = 3272,
            ObjectsToEnable = [6790, 6791],
            ObjectsToDisable = [],
            ItemName = "RoA Switch 1st Shortcut",
            LocationName = "Ruins of Ash - Switch (1st Shortcut)",
        },
        new()
        {
            Id = "197",
            RoomId = 6785,
            ObjectsToEnable = [],
            ObjectsToDisable = [6792],
            ItemName = "RoA Switch Spike Climb",
            LocationName = "Ruins of Ash - Switch (Spike Climb)",
        },
        new()
        {
            Id = "198",
            RoomId = 2800,
            ObjectsToEnable = [6863, 6865, 6872],
            ObjectsToDisable = [],
            ItemName = "HotP Crystal Bell Access",
            LocationName = "Hall of the Phantoms - Crystal (Bell Access)",
        },
        new()
        {
            Id = "199",
            RoomId = 3217,
            ObjectsToEnable = [6882, 6883, 6884, 6885],
            ObjectsToDisable = [],
            ItemName = "HotP Crystal Heart",
            LocationName = "Hall of the Phantoms - Crystal (Heart)",
        },
        new()
        {
            Id = "199",
            RoomId = 2801,
            ObjectsToEnable = [],
            ObjectsToDisable = [6870, 6871],
            ItemName = "HotP Switch Bell Access",
            LocationName = "Hall of the Phantoms - Switch (Bell Access)",
        },
        new()
        {
            Id = "200",
            RoomId = 6787,
            ObjectsToEnable = [],
            ObjectsToDisable = [6903, 6904, 6905],
            ItemName = "RoA Face Blue Key",
            LocationName = "Ruins of Ash - Face (Blue Key)",
        },
        new()
        {
            Id = "201",
            RoomId = 3588,
            ObjectsToEnable = [],
            ObjectsToDisable = [6925, 6926],
            ItemName = "RoA Crystal Left Ascend",
            LocationName = "Ruins of Ash - Crystal (Left Ascend)",
        },
        new()
        {
            Id = "202",
            RoomId = 4095,
            ObjectsToEnable = [],
            ObjectsToDisable = [8718],
            ItemName = "RoA Crystal Shaft",
            LocationName = "Ruins of Ash - Crystal (Shaft)",
        },
        new()
        {
            Id = "204",
            RoomId = 4083,
            ObjectsToEnable = [6948, 6949],
            ObjectsToDisable = [],
            ItemName = "RoA Crystal Branch Right",
            LocationName = "Ruins of Ash - Crystal (Branch Right)",
        },
        new()
        {
            Id = "205",
            RoomId = 4083,
            ObjectsToEnable = [6950, 6951],
            ObjectsToDisable = [],
            ItemName = "RoA Crystal Branch Left",
            LocationName = "Ruins of Ash - Crystal (Branch Left)",
        },
        new()
        {
            Id = "207",
            RoomId = 2473,
            ObjectsToEnable = [6982],
            ObjectsToDisable = [],
            ItemName = "Cata Switch Mid Shortcut",
            LocationName = "Catacombs - Switch (Mid Shortcut)",
        },
        new()
        {
            Id = "208",
            RoomId = 334,
            ObjectsToEnable = [],
            ObjectsToDisable = [7120],
            ItemName = "GT Switch Upper Arias",
            LocationName = "Gorgon Tomb - Switch (Upper Arias)",
        },
        new()
        {
            Id = "209",
            RoomId = 1215,
            ObjectsToEnable = [],
            ObjectsToDisable = [7132],
            ItemName = "Mech Switch Eyeball",
            LocationName = "Mechanism - Switch (Eyeball)",
        },
        new()
        {
            Id = "214",
            RoomId = 341,
            ObjectsToEnable = [],
            ObjectsToDisable = [7208],
            ItemName = "Mech Crystal Campfire",
            LocationName = "Mechanism - Crystal (Campfire)",
        },
        new()
        {
            Id = "215",
            RoomId = 7256,
            ObjectsToEnable = [],
            ObjectsToDisable = [7285],
            ItemName = "Cath Crystal 1st Room",
            LocationName = "Cathedral - Crystal (1st Room)",
        },
        new()
        {
            Id = "216",
            RoomId = 7260,
            ObjectsToEnable = [],
            ObjectsToDisable = [7317, 7318, 10184, 10187, 10188, 10185, 10186, 10189],
            ItemName = "Cath Switch Bottom",
            LocationName = "Cathedral - Switch (Bottom)",
        },
        new()
        {
            Id = "218",
            RoomId = 7264,
            ObjectsToEnable = [],
            ObjectsToDisable = [7325],
            ItemName = "Cath Crystal Shaft",
            LocationName = "Cathedral - Crystal (Shaft)",
        },
        new()
        {
            Id = "219",
            RoomId = 59,
            ObjectsToEnable = [],
            ObjectsToDisable = [1101],
            ItemName = "Cata Switch 1st Room",
            LocationName = "Catacombs - Switch (1st Room)",
        },
        new()
        {
            Id = "220",
            RoomId = 7361,
            ObjectsToEnable = [],
            ObjectsToDisable = [10474, 10475, 10471, 10473],
            ItemName = "CD Crystal Backtrack",
            LocationName = "Cyclops Den - Crystal (Backtrack)",
        },
        new()
        {
            Id = "220",
            RoomId = 187,
            ObjectsToEnable = [2272],
            ObjectsToDisable = [],
            ItemName = "Mech Crystal 1st Room",
            LocationName = "Mechanism - Crystal (1st Room)",
        },
        new()
        {
            Id = "221",
            RoomId = 6160,
            ObjectsToEnable = [7351, 7352, 7353, 7349],
            ObjectsToDisable = [],
            ItemName = "Mech Crystal Old Man",
            LocationName = "Mechanism - Crystal (Old Man)",
        },
        new()
        {
            Id = "222",
            RoomId = 7383,
            ObjectsToEnable = [7430, 7429, 7431, 7432, 7433],
            ObjectsToDisable = [7419, 7420, 7421, 7422, 7423, 7424, 7425, 7426, 7427, 7428],
            ItemName = "SP Crystal Blocks",
            LocationName = "Serpent Path - Crystal (Blocks)",
        },
        new()
        {
            Id = "223",
            RoomId = 7360,
            ObjectsToEnable = [],
            ObjectsToDisable = [7512],
            ItemName = "CD Crystal Start",
            LocationName = "Cyclops Den - Crystal (Start)",
        },
        new()
        {
            Id = "225",
            RoomId = 7361,
            ObjectsToEnable = [],
            ObjectsToDisable = [7536],
            ItemName = "CD Switch 1",
            LocationName = "Cyclops Den - Switch 1",
        },
        new()
        {
            Id = "226",
            RoomId = 7362,
            ObjectsToEnable = [],
            ObjectsToDisable = [7547, 7548],
            ItemName = "CD Switch 2",
            LocationName = "Cyclops Den - Switch 2",
        },
        new()
        {
            Id = "227",
            RoomId = 7364,
            ObjectsToEnable = [],
            ObjectsToDisable = [7557, 7556, 7555],
            ItemName = "CD Switch 3",
            LocationName = "Cyclops Den - Switch 3",
        },
        new()
        {
            Id = "228",
            RoomId = 7368,
            ObjectsToEnable = [7581],
            ObjectsToDisable = [],
            ItemName = "CD Switch Campfire",
            LocationName = "Cyclops Den - Switch (Campfire)",
        },
        new()
        {
            Id = "229",
            RoomId = 7371,
            ObjectsToEnable = [],
            ObjectsToDisable = [7707],
            ItemName = "CD Crystal Campfire",
            LocationName = "Cyclops Den - Crystal (Campfire)",
        },
        new()
        {
            Id = "230",
            RoomId = 7374,
            ObjectsToEnable = [],
            ObjectsToDisable = [7735],
            ItemName = "CD Crystal Steps",
            LocationName = "Cyclops Den - Crystal (Steps)",
        },
        new()
        {
            Id = "231",
            RoomId = 7377,
            ObjectsToEnable = [7744, 7745],
            ObjectsToDisable = [],
            ItemName = "CD Switch Top",
            LocationName = "Cyclops Den - Switch (Top)",
        },
        new()
        {
            Id = "233",
            RoomId = 7393,
            ObjectsToEnable = [],
            ObjectsToDisable = [7820, 10597],
            ItemName = "SP Switch Double Doors",
            LocationName = "Serpent Path - Switch (Double Doors)",
        },
        new()
        {
            Id = "234",
            RoomId = 7387,
            ObjectsToEnable = [],
            ObjectsToDisable = [8005],
            ItemName = "SP Switch Bubbles",
            LocationName = "Serpent Path - Switch (Bubbles)",
        },
        new()
        {
            Id = "235",
            RoomId = 8090,
            ObjectsToEnable = [],
            ObjectsToDisable = [8154, 8156],
            ItemName = "SP Crystal Star",
            LocationName = "Serpent Path - Crystal (Star)",
        },
        new()
        {
            Id = "236",
            RoomId = 7309,
            ObjectsToEnable = [],
            ObjectsToDisable = [8242],
            ItemName = "Cath Crystal Spike Pit",
            LocationName = "Cathedral - Crystal (Spike Pit)",
        },
        new()
        {
            Id = "237",
            RoomId = 8219,
            ObjectsToEnable = [8384],
            ObjectsToDisable = [],
            ItemName = "Cath Crystal Top Left",
            LocationName = "Cathedral - Crystal (Top Left)",
        },
        new()
        {
            Id = "238",
            RoomId = 8219,
            ObjectsToEnable = [8386, 8388],
            ObjectsToDisable = [],
            ItemName = "Cath Crystal Top Right",
            LocationName = "Cathedral - Crystal (Top Right)",
        },
        new()
        {
            Id = "239",
            RoomId = 8427,
            ObjectsToEnable = [],
            ObjectsToDisable = [8460],
            ItemName = "Cath Crystal Shaft Access",
            LocationName = "Cathedral - Crystal (Shaft Access)",
        },
        new()
        {
            Id = "240",
            RoomId = 8427,
            ObjectsToEnable = [],
            ObjectsToDisable = [8467, 8466],
            ItemName = "Cath Switch Beside Shaft",
            LocationName = "Cathedral - Switch (Beside Shaft)",
        },
        new()
        {
            Id = "242",
            RoomId = 3583,
            ObjectsToEnable = [],
            ObjectsToDisable = [8656],
            ItemName = "RoA Crystal 3 Reapers",
            LocationName = "Ruins of Ash - Crystal (3 Reapers)",
        },
        new()
        {
            Id = "244",
            RoomId = 4098,
            ObjectsToEnable = [8697, 9479, 9480, 9481, 9482, 9483, 9484, 9485, 9486, 9487, 9488, 9489, 9490],
            ObjectsToDisable = [],
            ItemName = "RoA Switch Above Centaur",
            LocationName = "Ruins of Ash - Switch (Above Centaur)",
        },
        new()
        {
            Id = "245",
            RoomId = 4106,
            ObjectsToEnable = [8769, 8770],
            ObjectsToDisable = [10684, 10682, 10683, 10685, 10686, 10687],
            ItemName = "RoA Switch Blood Pot",
            LocationName = "Ruins of Ash - Switch (Blood Pot)",
        },
        new()
        {
            Id = "255",
            RoomId = 806,
            ObjectsToEnable = [],
            ObjectsToDisable = [9233, 9234, 9235],
            ItemName = "Mech Crystal Top Chains",
            LocationName = "Mechanism - Crystal (Top Chains)",
        },
        new()
        {
            Id = "256",
            RoomId = 820,
            ObjectsToEnable = [],
            ObjectsToDisable = [9241, 9242, 9243],
            ItemName = "HotP Switch 1st Room",
            LocationName = "Hall of the Phantoms - Switch (1st Room)",
        },
        new()
        {
            Id = "257",
            RoomId = 875,
            ObjectsToEnable = [],
            ObjectsToDisable = [9270, 9269],
            ItemName = "HotP Switch Left Backtrack",
            LocationName = "Hall of the Phantoms - Switch (Left Backtrack)",
        },
        new()
        {
            Id = "260",
            RoomId = 4139,
            ObjectsToEnable = [],
            ObjectsToDisable = [9411],
            ItemName = "RoA Switch Worms",
            LocationName = "Ruins of Ash - Switch (Worms)",
        },
        new()
        {
            Id = "262",
            RoomId = 342,
            ObjectsToEnable = [],
            ObjectsToDisable = [2274, 2275],
            ItemName = "Mech Crystal BK",
            LocationName = "Mechanism - Crystal (Black Knight)",
        },
        new()
        {
            Id = "264",
            RoomId = 3941,
            ObjectsToEnable = [],
            ObjectsToDisable = [9497, 9498, 9499, 9500],
            ItemName = "RoA Switch Triple 1",
            LocationName = "Ruins of Ash - Switch (Triple 1)",
        },
        new()
        {
            Id = "265",
            RoomId = 3941,
            ObjectsToEnable = [],
            ObjectsToDisable = [9502],
            ItemName = "RoA Crystal Triple 2",
            LocationName = "Ruins of Ash - Crystal (Triple 2)",
        },
        new()
        {
            Id = "266",
            RoomId = 3941,
            ObjectsToEnable = [],
            ObjectsToDisable = [9504],
            ItemName = "RoA Switch Triple 3",
            LocationName = "Ruins of Ash - Switch (Triple 3)",
        },
        new()
        {
            Id = "267",
            RoomId = 4172,
            ObjectsToEnable = [],
            ObjectsToDisable = [10044, 10045],
            ItemName = "RoA Switch Baby Gorgon",
            LocationName = "Ruins of Ash - Switch (Baby Gorgon)",
        },
        new()
        {
            Id = "268",
            RoomId = 9739,
            ObjectsToEnable = [],
            ObjectsToDisable = [9742],
            ItemName = "TR Switch Adorned Left",
            LocationName = "Tower Roots - Switch (Adorned Key Left)",
        },
        new()
        {
            Id = "269",
            RoomId = 9739,
            ObjectsToEnable = [],
            ObjectsToDisable = [9744],
            ItemName = "TR Switch Adorned Middle",
            LocationName = "Tower Roots - Switch (Adorned Key Middle)",
        },
        new()
        {
            Id = "270",
            RoomId = 9739,
            ObjectsToEnable = [],
            ObjectsToDisable = [9746],
            ItemName = "TR Switch Adorned Right",
            LocationName = "Tower Roots - Switch (Adorned Key Right)",
        },
        new()
        {
            Id = "272",
            RoomId = 9719,
            ObjectsToEnable = [],
            ObjectsToDisable = [9769],
            ItemName = "TR Crystal Gold",
            LocationName = "Tower Roots - Crystal (Gold)",
        },
        new()
        {
            Id = "273",
            RoomId = 3257,
            ObjectsToEnable = [],
            ObjectsToDisable = [9905, 9908, 9907, 9909],
            ItemName = "HotP Crystal Below Puzzle",
            LocationName = "Hall of the Phantoms - Crystal (Below Puzzle)",
        },
        new()
        {
            Id = "276",
            RoomId = 811,
            ObjectsToEnable = [9933, 9932],
            ObjectsToDisable = [9934],
            ItemName = "Mech Switch Invisible",
            LocationName = "Mechanism - Switch (Invisible)",
        },
        new()
        {
            Id = "278",
            RoomId = 10014,
            ObjectsToEnable = [],
            ObjectsToDisable = [10028, 10029, 10030, 10031],
            ItemName = "RoA Switch Boss Access",
            LocationName = "Ruins of Ash - Switch (Boss Access)",
        },
        new()
        {
            Id = "280",
            RoomId = 8860,
            ObjectsToEnable = [],
            ObjectsToDisable = [10067],
            ItemName = "Darkness Switch",
            LocationName = "Darkness - Switch",
        },
        new()
        {
            Id = "283",
            RoomId = 2667,
            ObjectsToEnable = [],
            ObjectsToDisable = [10139, 10141, 10142],
            ItemName = "Cata Switch Flames 2",
            LocationName = "Catacombs - Switch (Flames 2)",
        },
        new()
        {
            Id = "284",
            RoomId = 2667,
            ObjectsToEnable = [10145, 10147, 10148, 10149],
            ObjectsToDisable = [],
            ItemName = "Cata Switch Flames 1",
            LocationName = "Catacombs - Switch (Flames 1)",
        },
        new()
        {
            Id = "286",
            RoomId = 7259,
            ObjectsToEnable = [],
            ObjectsToDisable = [10166, 10165, 10164, 10162, 10163, 10156, 10157, 10155],
            ItemName = "Cath Face Left",
            LocationName = "Cathedral - Face (Left)",
        },
        new()
        {
            Id = "287",
            RoomId = 7259,
            ObjectsToEnable = [],
            ObjectsToDisable = [10161, 10160, 10159],
            ItemName = "Cath Face Right",
            LocationName = "Cathedral - Face (Right)",
        },
        new()
        {
            Id = "289",
            RoomId = 8425,
            ObjectsToEnable = [],
            ObjectsToDisable = [10249],
            ItemName = "Cath Crystal Orbs",
            LocationName = "Cathedral - Crystal (Orbs)",
        },
        new()
        {
            Id = "290",
            RoomId = 7310,
            ObjectsToEnable = [],
            ObjectsToDisable = [10268],
            ItemName = "Cath Switch Top Campfire",
            LocationName = "Cathedral - Switch (Top Campfire)",
        },
        new()
        {
            Id = "292",
            RoomId = 2696,
            ObjectsToEnable = [],
            ObjectsToDisable = [10288],
            ItemName = "TR Switch Elevator",
            LocationName = "Tower Roots - Switch (Elevator)",
        },
        new()
        {
            Id = "293",
            RoomId = 9717,
            ObjectsToEnable = [],
            ObjectsToDisable = [10310, 10311],
            ItemName = "TR Switch Bottom",
            LocationName = "Tower Roots - Switch (Bottom)",
        },
        new()
        {
            Id = "294",
            RoomId = 2608,
            ObjectsToEnable = [10442],
            ObjectsToDisable = [],
            ItemName = "Cata Crystal Poison Roots",
            LocationName = "Catacombs - Crystal (Poison Roots)",
        },
        new()
        {
            Id = "298",
            RoomId = 9718,
            ObjectsToEnable = [10490, 10492],
            ObjectsToDisable = [],
            ItemName = "TR Crystal Dark Arias",
            LocationName = "Tower Roots - Crystal (Dark Arias)",
        },
        new()
        {
            Id = "300",
            RoomId = 2793,
            ObjectsToEnable = [],
            ObjectsToDisable = [10579, 10578, 10577],
            ItemName = "Cata Face Bottom",
            LocationName = "Catacombs - Face (Bottom)",
        },
        new()
        {
            Id = "303",
            RoomId = 8093,
            ObjectsToEnable = [],
            ObjectsToDisable = [10596],
            ItemName = "SP Switch After Star",
            LocationName = "Serpent Path - Switch (After Star)",
        },
        new()
        {
            Id = "305",
            RoomId = 4116,
            ObjectsToEnable = [],
            ObjectsToDisable = [10678],
            ItemName = "RoA Switch Blood Pot Left",
            LocationName = "Ruins of Ash - Switch (Blood Pot Left)",
        },
        new()
        {
            Id = "306",
            RoomId = 4116,
            ObjectsToEnable = [],
            ObjectsToDisable = [10680],
            ItemName = "RoA Switch Blood Pot Right",
            LocationName = "Ruins of Ash - Switch (Blood Pot Right)",
        },
        new()
        {
            Id = "309",
            RoomId = 7437,
            ObjectsToEnable = [],
            ObjectsToDisable = [10756],
            ItemName = "RoA Switch Lower Void",
            LocationName = "Ruins of Ash - Switch (Lower Void)",
        },
    ];

    public static readonly Dictionary<(int, string), string> LinkToLocation =
        Switches.ToDictionary((data) => (data.RoomId, data.Id), (data) => data.LocationName);

    public static readonly Dictionary<string, SwitchData> ItemToLink = Switches.ToDictionary((data) => data.ItemName);

    public static readonly Dictionary<int, string> ElevatorToLocation = new()
    {
        //{ 6629, "Gorgon Tomb - Elevator 1" },
        { 248, "Gorgon Tomb - Elevator 2" },
        { 3947, "Mechanism - Elevator 1" },
        { 803, "Mechanism - Elevator 2" },
        { 10535, "Hall of the Phantoms - Elevator" },
        { 1080, "Ruins of Ash - Elevator 1" },
        { 8771, "Ruins of Ash - Elevator 2" },
        { 4109, "The Apex - Elevator" },
        { 61, "Catacombs - Elevator 1" },
        { 2574, "Catacombs - Elevator 2" },
        { 2705, "Tower Roots - Elevator" },
    };

    public static readonly Dictionary<string, int> ItemToElevator = new()
    {
        //{ "GT 1 Elevator", 6629 },
        { "GT 2 Elevator", 248 },
        { "Mech 1 Elevator", 3947 },
        { "Mech 2 Elevator", 803 },
        { "HotP Elevator", 10535 },
        { "RoA 1 Elevator", 1080 },
        { "RoA 2 Elevator", 8771 },
        { "Apex Elevator", 4109 },
        { "Cata 1 Elevator", 61 },
        { "Cata 2 Elevator", 2574 },
        { "TR Elevator", 2705 },
    };
}