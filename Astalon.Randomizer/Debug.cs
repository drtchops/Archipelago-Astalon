using Astalon.Randomizer.Archipelago;
using UnityEngine;

namespace Astalon.Randomizer;

public static class Debug
{
    public static bool Hidden { get; private set; } = true;

    public static void OnGUI()
    {
        if (!ArchipelagoClient.Connected)
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


        var left1 = Screen.width / 2 - 204;
        var left2 = Screen.width / 2 + 4;
        var bottom = Screen.height - 25;
        var top = bottom - 125;
        const int width = 200;
        const int height = 20;

        GUI.BeginGroup(new(left1, top, width, 125));

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

#if DEBUG
        if (GUI.Button(new(0, 100, width, height), "Dump Room Data"))
        {
            Game.DumpRoom = true;
        }
#endif

        GUI.EndGroup();

        GUI.BeginGroup(new(left2, top, width, 125));

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
    }
}