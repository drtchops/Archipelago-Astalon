using System;
using UnityEngine;

namespace Astalon.Randomizer;

public static class Debug
{
    private const int ButtonWidth = 150;
    private const int ButtonHeight = 20;
    private const int ButtonPadding = 4;

    internal struct Button(Func<string> label, Action callback)
    {
        public Func<string> Label = label;
        public Action Callback = callback;
    }

    internal struct Group
    {
        public int Left;
        public int Top;
        public int Height;
        public Button[] Buttons;

        public Group(int left, int top, Button[] buttons)
        {
            Left = left < 0 ? Screen.width - ButtonWidth + left : left;
            Height = (ButtonHeight * buttons.Length) + (ButtonPadding * (buttons.Length - 1));
            Top = top < 0 ? Screen.height - Height + top : top;
            Buttons = buttons;
        }
    }

    public static bool Hidden { get; private set; } = true;
    private static string _roomWarp = "";
    private static string _orbs = "";

    private static readonly Group[] ButtonGroups = [
        new(
            left: (Screen.width / 2) - ButtonWidth - (ButtonPadding / 2),
            top: -ButtonPadding,
            buttons: [
                new(
                    label: () => "Die",
                    callback: () => Game.TriggerDeath = true
                ),
                new(
                    label: () => Plugin.ArchipelagoClient.DeathLinkEnabled() ? "Disable Death Link" : "Enable Death Link",
                    callback: Plugin.ArchipelagoClient.ToggleDeathLink
                ),
                new(
                    label: () => Settings.ShowConnection ? "Hide Connection" : "Show Connection",
                    callback: Plugin.ToggleConnection
                ),
                new(
                    label: () => Settings.ShowConsole ? "Hide Console" : "Show Console",
                    callback: Plugin.ToggleConsole
                ),
                new(
                    label: () => Settings.RunInBackground ? "Pause In Background" : "Run In Background",
                    callback: Plugin.ToggleRunInBackground
                ),
            ]
        ),
        new(
            left: (Screen.width / 2) + (ButtonPadding / 2),
            top: -ButtonPadding,
            buttons: [
                new(
                    label: () => Settings.Invincibility ? "Disable Invincibility" : "Enable Invincibility",
                    callback: () => Settings.Invincibility = !Settings.Invincibility
                ),
                new(
                    label: () => Settings.MaxDamage ? "Disable Max Damage" : "Enable Max Damage",
                    callback: () => Settings.MaxDamage = !Settings.MaxDamage
                ),
                new(
                    label: () => Settings.FreeKeys ? "Disable Free Keys" : "Enable Free Keys",
                    callback: () => Settings.FreeKeys = !Settings.FreeKeys
                ),
                new(
                    label: () => Settings.FreePurchases ? "Disable Free Purchases" : "Enable Free Purchases",
                    callback: () => Settings.FreePurchases = !Settings.FreePurchases
                ),
                new(
                    label: () => Settings.InfiniteJumps ? "Disable Infinite Jumps" : "Enable Infinite Jumps",
                    callback: Game.ToggleInfiniteJumps
                ),
                new(
                    label: () => "Unlock All Elevators",
                    callback: () => Game.UnlockElevators = true
                ),
            ]
        ),
#if DEBUG
        new(
            left: ButtonPadding,
            top: -ButtonPadding,
            buttons: [
                new(
                    label: () => "Dump Room Data",
                    callback: () => Game.DumpRoom = true
                ),
                new(
                    label: () => "Toggle Switches",
                    callback: () => Game.ToggleSwitches = true
                ),
                new(
                    label: () => "Toggle Objects",
                    callback: () => Game.ToggleObjects = true
                ),
                new(
                    label: () => "Reset Doors",
                    callback: () => Game.ResetDoors = true
                ),
            ]
        ),
#endif
    ];

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
        { "Tower Roots", "Dev Room", null, null },
    };

    private static Texture2D _blackTexture;

    private static Texture2D MakeTexture()
    {
        if (_blackTexture != null)
        {
            return _blackTexture;
        }

        var width = 128;
        var height = 64;
        var texture = new Texture2D(width, height);
        var pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.black;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        _blackTexture = texture;
        return texture;
    }

    public static void OnGUI()
    {
        var e = Event.current;
        if (e.type == EventType.KeyUp && e.keyCode == KeyCode.F1)
        {
            Hidden = !Hidden;
        }

        if (Hidden)
        {
            return;
        }

        DebugButtons();

        if (Plugin.State.CampfireWarpsEnabled())
        {
            CampfireWarps();
        }

        if (Plugin.State.IsEyeHunt())
        {
            GoalDisplay();
        }

#if DEBUG
        ExtraDebug();
#endif
    }

    private static void DebugButtons()
    {
        foreach (var g in ButtonGroups)
        {
            GUI.BeginGroup(new(g.Left, g.Top, ButtonWidth, g.Height));
            var y = 0;

            foreach (var b in g.Buttons)
            {
                if (GUI.Button(new(0, y, ButtonWidth, ButtonHeight), b.Label()))
                {
                    b.Callback();
                }

                y += ButtonHeight + ButtonPadding;
            }

            GUI.EndGroup();
        }
    }

    private static void CampfireWarps()
    {
        GUI.BeginGroup(new(ButtonPadding * 2, 124, WarpButtons.GetLength(1) * (ButtonWidth + ButtonPadding),
            WarpButtons.GetLength(0) * (ButtonHeight + ButtonPadding)));

        var y = 0;

        for (var i = 0; i < WarpButtons.GetLength(0); i++)
        {
            var x = 0;

            for (var j = 0; j < WarpButtons.GetLength(1); j++)
            {
                var label = WarpButtons[i, j];

                if (label != null && Game.CanWarp(label) && GUI.Button(new(x, y, ButtonWidth, ButtonHeight), label))
                {
                    Game.WarpDestination = label;
                }

                x += ButtonWidth + ButtonPadding;
            }

            y += ButtonHeight + ButtonPadding;
        }

        GUI.EndGroup();
    }

    private static void ExtraDebug()
    {
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

        _roomWarp = GUI.TextField(new(950, 540, 75, 20), _roomWarp);
        if (GUI.Button(new(1000, 540, 75, 20), "Warp"))
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

        _orbs = GUI.TextField(new(950, 560, 75, 20), _orbs);
        if (GUI.Button(new(1000, 560, 75, 20), "Set Orbs"))
        {
            try
            {
                Player.PlayerDataLocal.currentOrbs = int.Parse(_orbs);
                GameplayUIManager.UpdateOrbs();
            }
            catch
            {
                // ignored
            }
        }
    }

    private static void GoalDisplay()
    {
        var eyes = Plugin.State.GoldEyesCollected();
        var goal = Plugin.State.GoldEyeRequirement();

        var style = new GUIStyle("box");
        style.normal.textColor = eyes >= goal ? Color.green : Color.red;
        style.normal.background = MakeTexture();
        GUI.Box(new(Screen.width - 164, 124, 160, 25), $"Eye Hunt Goal: {eyes} / {goal}", style);
    }
}
