using Astalon.Randomizer.Archipelago;
using UnityEngine;

namespace Astalon.Randomizer;

public static class Debug
{
    public static bool Hidden { get; private set; } = true;
    private static string _roomWarp = "";

    private static readonly string[,] WarpButtons =
    {
        { "Last Checkpoint", "Entrance", "Tutorial", null },
        { "GT Bottom", "GT Left", "GT Boss", null },
        { "Mechanism Start", "Mechanism Sword", "Mechanism Bottom", null },
        { "Mechanism Shortcut", "Mechanism Right", "Mechanism Top", "Mechanism Boss" },
        { "CD 1", "CD 2", "CD 3", "CD 4" },
        { "HotP Epimetheus", "HotP Bell", "HotP Claw", "HotP Boss" },
        { "Cathedral 1", "Cathedral 2", null, null },
        { "RoA Start", "RoA Left", "RoA Middle", null },
        { "RoA Elevator", "RoA Boss", null, null },
        { "SP 1", "SP 2", "The Apex", null },
        { "Catacombs Upper", "Catacombs Bow", "Catacombs Roots", "Catacombs Boss" },
        { "Tower Roots", null, null, null },
    };

    public static void OnGUI()
    {
        if (!Plugin.ArchipelagoClient.Connected)
        {
            return;
        }

        var e = Event.current;
        if (e.type == EventType.KeyUp && e.keyCode == KeyCode.F1)
        {
            Hidden = !Hidden;
        }

        if (Hidden)
        {
            return;
        }


        const int width = 150;
        const int height = 20;
        const int padding = 4;
        var left1 = (Screen.width / 2) - width - (padding / 2);
        var left2 = (Screen.width / 2) + (padding / 2);
        var bottom = Screen.height - height - padding;
        const int fullHeight = (height + padding) * 5;
        var top = bottom - fullHeight;

        GUI.BeginGroup(new(left1, top, width, fullHeight));

        if (GUI.Button(new(0, 0, width, height), "Die"))
        {
            Game.TriggerDeath = true;
        }

        if (GUI.Button(new(0, 25, width, height),
                Plugin.ArchipelagoClient.DeathLinkEnabled() ? "Disable Death Link" : "Enable Death Link"))
        {
            Plugin.ArchipelagoClient.ToggleDeathLink();
        }

        if (GUI.Button(new(0, 50, width, height),
                Settings.ShowConnection ? "Hide Connection" : "Show Connection"))
        {
            Plugin.ToggleConnection();
        }

        if (GUI.Button(new(0, 75, width, height), Settings.ShowConsole ? "Hide Console" : "Show Console"))
        {
            Plugin.ToggleConsole();
        }

        if (GUI.Button(new(0, 100, width, height),
                Settings.InfiniteJumps ? "Disable Infinite Jumps" : "Enable Infinite Jumps"))
        {
            Settings.InfiniteJumps = !Settings.InfiniteJumps;
        }

        GUI.EndGroup();

        GUI.BeginGroup(new(left2, top, width, fullHeight));

        if (GUI.Button(new(0, 0, width, height),
                Settings.Invincibility ? "Disable Invincibility" : "Enable Invincibility"))
        {
            Settings.Invincibility = !Settings.Invincibility;
        }

        if (GUI.Button(new(0, 25, width, height),
                Settings.MaxDamage ? "Disable Max Damage" : "Enable Max Damage"))
        {
            Settings.MaxDamage = !Settings.MaxDamage;
        }

        if (GUI.Button(new(0, 50, width, height),
                Settings.FreeKeys ? "Disable Free Keys" : "Enable Free Keys"))
        {
            Settings.FreeKeys = !Settings.FreeKeys;
        }

        if (GUI.Button(new(0, 75, width, height),
                Settings.FreePurchases ? "Disable Free Purchases" : "Enable Free Purchases"))
        {
            Settings.FreePurchases = !Settings.FreePurchases;
        }

        if (GUI.Button(new(0, 100, width, height), "Unlock All Elevators"))
        {
            Game.UnlockElevators = true;
        }

        GUI.EndGroup();

        if (!ArchipelagoClient.ServerData.SlotData.CampfireWarp)
        {
            return;
        }

        GUI.BeginGroup(new(padding * 2, 124, WarpButtons.GetLength(1) * (width + padding),
            WarpButtons.GetLength(0) * (height + padding)));

        var y = 0;

        for (var i = 0; i < WarpButtons.GetLength(0); i++)
        {
            var x = 0;

            for (var j = 0; j < WarpButtons.GetLength(1); j++)
            {
                var label = WarpButtons[i, j];

                if (label != null && Game.CanWarp(label) && GUI.Button(new(x, y, width, height), label))
                {
                    Game.WarpDestination = label;
                }

                x += width + padding;
            }

            y += height + padding;
        }

        GUI.EndGroup();

#if DEBUG
        if (GUI.Button(new(8, bottom - 25, width, height), "Dump Room Data"))
        {
            Game.DumpRoom = true;
        }

        if (GUI.Button(new(8, bottom - 50, width, height), "Toggle Switches"))
        {
            Game.ToggleSwitches = true;
        }

        if (GUI.Button(new(8, bottom - 75, width, height), "Toggle Objects"))
        {
            Game.ToggleObjects = true;
        }

        if (GUI.Button(new(8, bottom - 100, width, height), "Reset Doors"))
        {
            Game.ResetDoors = true;
        }

        if (GUI.Button(new(1000, 500, 50, 20), "Up"))
        {
            Game.MoveDirection = "up";
        }

        if (GUI.Button(new(1000, 520, 50, 20), "Down"))
        {
            Game.MoveDirection = "down";
        }

        if (GUI.Button(new(950, 520, 50, 20), "Left"))
        {
            Game.MoveDirection = "left";
        }

        if (GUI.Button(new(1050, 520, 50, 20), "Right"))
        {
            Game.MoveDirection = "right";
        }

        _roomWarp = GUI.TextField(new(950, 540, 50, 20), _roomWarp);
        if (GUI.Button(new(1000, 540, 50, 20), "Warp"))
        {
            try
            {
                var roomId = int.Parse(_roomWarp);
                Game.RoomWarp = roomId;
            }
            catch
            {
                // ignored
            }
        }
#endif
    }
}