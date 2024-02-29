using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using BepInEx.Logging;

namespace Archipelago;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Astalon.exe")]
public class Main : BasePlugin
{
    public static new ManualLogSource Log = Logger.CreateLogSource("Archipelago");
    public static Settings Settings;
    public static Game Game;
    public static APManager APManager;

    public override void Load()
    {
        var configAddress = Config.Bind("General", "address", "archipelago.gg", "The address of the archipelago server to connect to");
        var configPort = Config.Bind("General", "port", 38281, "The port of the archipelago server to connect to");
        var configSlotName = Config.Bind("General", "slotName", "Player1", "The slot name of the player you are connecting as");
        var configPassword = Config.Bind("General", "password", "", "The password for the player you are connecting as");
        var configInvincibility = Config.Bind("Cheats", "invincibility", false, "Whether the player character is invincible");
        var configMaxDamage = Config.Bind("Cheats", "maxDamage", false, "Whether enemies always take 99 damage from attacks");
        var configFreeKeys = Config.Bind("Cheats", "freeKeys", false, "Whether keys are not used when opening doors");

        Settings = new Settings
        {
            address = configAddress.Value,
            port = configPort.Value,
            slotName = configSlotName.Value,
            password = configPassword.Value,
            invincibility = configInvincibility.Value,
            maxDamage = configMaxDamage.Value,
            freeKeys = configFreeKeys.Value,
        };

        Game = new Game();
        APManager = new APManager();

        var harmony = new Harmony("Archipelago");
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }
}
