namespace Archipelago;

public class Settings
{
    public string Address { get; set; }
    public int Port { get; set; }
    public string SlotName { get; set; }
    public string Password { get; set; }

    public bool Invincibility { get; set; }
    public bool MaxDamage { get; set; }
    public bool FreeKeys { get; set; }
    public bool FreePurchases { get; set; }

    public bool RandomizeAttackPickups { get; set; }
    public bool RandomizeHealthPickups { get; set; }
    public bool SkipCutscenes { get; set; }
    public bool StartWithZeek { get; set; }
    public bool StartWithBram { get; set; }
    public bool StartWithQOL { get; set; }
    public bool FreeApexElevator { get; set; }
    public bool DeathLink { get; set; }
}
