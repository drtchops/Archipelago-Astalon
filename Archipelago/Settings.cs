namespace Archipelago;

public static class Settings
{
    public static string Address { get; set; }
    public static int Port { get; set; }
    public static string SlotName { get; set; }
    public static string Password { get; set; }

    public static bool Invincibility { get; set; }
    public static bool MaxDamage { get; set; }
    public static bool FreeKeys { get; set; }
    public static bool FreePurchases { get; set; }

    public static bool RandomizeAttackPickups { get; set; }
    public static bool RandomizeHealthPickups { get; set; }
    public static bool RandomizeWhiteKeys { get; set; }
    public static bool RandomizeBlueKeys { get; set; }
    public static bool RandomizeRedKeys { get; set; }
    public static bool RandomizeFamiliars { get; set; }
    public static bool SkipCutscenes { get; set; }
    public static bool StartWithZeek { get; set; }
    public static bool StartWithBram { get; set; }
    public static bool StartWithQOL { get; set; }
    public static bool FreeApexElevator { get; set; }
    public static int CostMultiplier { get; set; }
    public static bool DeathLink { get; set; }
}
