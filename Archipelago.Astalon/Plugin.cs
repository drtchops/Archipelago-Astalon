using System.Reflection;
using Archipelago.Astalon.Archipelago;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using UnityEngine;

namespace Archipelago.Astalon;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Astalon.exe")]
public class Plugin : BasePlugin
{
    public static ManualLogSource Logger { get; private set; }
    public static ArchipelagoClient ArchipelagoClient { get; private set; }
    public static State State { get; set; }

    private static ConfigEntry<string> _configUri;
    private static ConfigEntry<string> _configSlotName;
    private static ConfigEntry<string> _configPassword;
    private static ConfigEntry<bool> _configShowConnection;
    private static ConfigEntry<bool> _configShowConsole;
    private static ConfigEntry<bool> _configRunInBackground;
    private static ConfigEntry<bool> _configSkipIntroScenes;

    public override void Load()
    {
        var configEnabled = Config.Bind("Archipelago", "enabled", true, "Enable or disable the mod as a whole");

        _configUri = Config.Bind("Archipelago", "uri", "archipelago.gg:38281",
            "The address and port of the archipelago server to connect to");
        _configSlotName = Config.Bind("Archipelago", "slotName", "Player1",
            "The slot name of the player you are connecting as");
        _configPassword = Config.Bind("Archipelago", "password", "",
            "The password for the player you are connecting as");

        _configShowConnection = Config.Bind("UI", "showConnection", true,
            "Show or hide the AP connection info when connected");
        _configShowConsole = Config.Bind("UI", "showConsole", true,
            "Show or hide the AP message console at the top of the screen");
        _configRunInBackground = Config.Bind("UI", "runInBackground", true,
            "Control if the game should continue running when not in focus");
        _configSkipIntroScenes = Config.Bind("UI", "skipIntroScenes", true,
            "Whether to automatically skip the intro screens when opening the game");

        if (!configEnabled.Value)
        {
            Logger.LogWarning("Archipelago disabled");
            return;
        }

        Settings.ShowConnection = _configShowConnection.Value;
        Settings.ShowConsole = _configShowConsole.Value;
        Settings.RunInBackground = _configRunInBackground.Value;
        Settings.SkipIntroScreens = _configSkipIntroScenes.Value;

        Application.runInBackground = Settings.RunInBackground;

        Logger = Log;

        // Game.Awake();

        var harmony = new Harmony("Archipelago");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        State = new(_configUri.Value, _configSlotName.Value, _configPassword.Value);
        ArchipelagoClient = new();

        Il2CppBase.Initialize(this);

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public static void UpdateConnectionInfo()
    {
        State.Uri = _configUri.Value;
        State.SlotName = _configSlotName.Value;
        State.Password = _configPassword.Value;
    }

    public static void UpdateConnectionInfo(string uri, string slotName, string password)
    {
        _configUri.Value = uri;
        _configSlotName.Value = slotName;
        _configPassword.Value = password;
        UpdateConnectionInfo();
    }

    public static void ToggleConnection()
    {
        var enabled = !Settings.ShowConnection;
        _configShowConnection.Value = enabled;
        Settings.ShowConnection = enabled;
    }

    public static void ToggleConsole()
    {
        var enabled = !Settings.ShowConsole;
        _configShowConsole.Value = enabled;
        Settings.ShowConsole = enabled;
    }

    public static void ToggleRunInBackground()
    {
        var enabled = !Settings.RunInBackground;
        _configRunInBackground.Value = enabled;
        Settings.RunInBackground = enabled;
        Application.runInBackground = enabled;
    }
}

//  adapted from https://github.com/gmh5225/IL2CPP-GUI-BepInEx-IL2CPPBase
public class Il2CppBase : MonoBehaviour
{
    public const string ModDisplayInfo = $"Astalon-{MyPluginInfo.PLUGIN_NAME} v{MyPluginInfo.PLUGIN_VERSION}";
    private const string ArchipelagoDisplayInfo = "Archipelago";

    public static bool ConnectionFocused { get; private set; }

    public static void Initialize(Plugin plugin)
    {
        var addComponent = plugin.AddComponent<Il2CppBase>();
        addComponent.hideFlags = HideFlags.HideAndDontSave;
        DontDestroyOnLoad(addComponent.gameObject);
    }

    private void Start()
    {
        ArchipelagoConsole.Awake();
    }

    // adapted from https://github.com/alwaysintreble/BepInEx5ArchipelagoPluginTemplate
    private void OnGUI()
    {
        ArchipelagoConsole.OnGUI();
        Debug.OnGUI();

        if (Plugin.ArchipelagoClient.Connected)
        {
            ConnectionFocused = false;

            if (!Settings.ShowConnection)
            {
                return;
            }

            GUI.BeginGroup(new(Screen.width - 304, Screen.height - 52, 304, 48));
            GUI.Box(new(0, 0, 300, 48), "");
            GUI.Label(new(4, 0, 300, 20), $"{ModDisplayInfo} (F1 for Debug)");
            GUI.Label(new(4, 24, 300, 20), $"{ArchipelagoDisplayInfo} Status: Connected");
            GUI.EndGroup();
            return;
        }

        GUI.BeginGroup(new(Screen.width - 308, Screen.height - 132, 308, 128));

        GUI.Box(new(0, 0, 300, 124), "");

        GUI.Label(new(4, 0, 300, 20), ModDisplayInfo);
        GUI.Label(new(4, 20, 300, 20), $"{ArchipelagoDisplayInfo} Status: Disconnected");
        GUI.Label(new(4, 40, 150, 20), "Host: ");
        GUI.Label(new(4, 60, 150, 20), "Player Name: ");
        GUI.Label(new(4, 80, 150, 20), "Password: ");

        var e = Event.current;
        var control = GUI.GetNameOfFocusedControl();
        var pressedEnter = e.type == EventType.KeyUp &&
                            control is "uri" or "slotName" or "password" &&
                            e.keyCode is KeyCode.KeypadEnter or KeyCode.Return;

        ConnectionFocused = control is "uri" or "slotName" or "password";

        GUI.SetNextControlName("uri");
        var uri = GUI.TextField(new(134, 40, 150, 20), Plugin.State.Uri);
        GUI.SetNextControlName("slotName");
        var slotName = GUI.TextField(new(134, 60, 150, 20), Plugin.State.SlotName);
        GUI.SetNextControlName("password");
        var password = GUI.PasswordField(new(134, 80, 150, 20), Plugin.State.Password, "*"[0]);
        Plugin.UpdateConnectionInfo(uri, slotName, password);

        var pressedButton = GUI.Button(new(4, 100, 100, 20), "Connect");
        if (pressedEnter || pressedButton)
        {
            Plugin.ArchipelagoClient.Connect();
        }

        GUI.EndGroup();
    }
}
