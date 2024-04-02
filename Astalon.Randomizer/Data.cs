using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astalon.Randomizer;

public class Checkpoint
{
    public int Id;
    public int RoomId;
    public Vector3 PlayerPos;
    public Vector2 CameraPos;
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

    public static readonly Dictionary<string, (int, int)> WhiteDoorMap = new()
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

    public static readonly Dictionary<string, (int, int)> BlueDoorMap = new()
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
        { "Cata Blue Door (Start)", (34, 2037) },
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

    public static readonly Dictionary<string, (int, int)> RedDoorMap = new()
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

    public static readonly Dictionary<string, DealProperties.DealID> LocationToDeal =
        DealToLocation.ToDictionary((kvp) => kvp.Value, (kvp) => kvp.Key);

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

    // elevator rooms
    // 6629 - start elevator
    // 248 - gorgon tomb 2
    // 4109 - the apex
}