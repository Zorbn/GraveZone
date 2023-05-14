namespace Common;

public class Inventory
{
    public const int Width = 10;
    public const int Height = 2;
    public const int SlotCount = Width * Height;

    public WeaponStats? EquippedWeaponStats { get; private set; }
    public readonly WeaponStats?[] Weapons = new WeaponStats[Width * Height];
    public WeaponStats? GrabbedWeaponStats { get; private set; }

    public bool IsFull()
    {
        foreach (var weapon in Weapons)
            if (weapon is null)
                return false;

        return true;
    }

    // Returns true if the weapon was successfully added, false otherwise (ie: the inventory is full).
    public bool AddWeapon(WeaponType weaponType)
    {
        var weapon = WeaponStats.Registry[weaponType];

        for (var i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i] is not null) continue;

            Weapons[i] = weapon;

            return true;
        }

        return false;
    }

    private WeaponStats? RemoveWeapon(int i)
    {
        var weapon = Weapons[i];
        Weapons[i] = null;

        return weapon;
    }

    public void GrabSlot(int i)
    {
        if (GrabbedWeaponStats is null)
        {
            var removedWeapon = RemoveWeapon(i);
            GrabbedWeaponStats = removedWeapon;
            return;
        }

        (Weapons[i], GrabbedWeaponStats) = SwapOrEvolve(Weapons[i], GrabbedWeaponStats);
    }

    public void GrabEquippedSlot()
    {
        if (GrabbedWeaponStats is null)
        {
            GrabbedWeaponStats = EquippedWeaponStats;
            EquippedWeaponStats = null;
            return;
        }

        (EquippedWeaponStats, GrabbedWeaponStats) = SwapOrEvolve(EquippedWeaponStats, GrabbedWeaponStats);
    }

    private static ValueTuple<WeaponStats?, WeaponStats?> SwapOrEvolve(WeaponStats? inSlot, WeaponStats? grabbed)
    {
        // The weapons can merge into an evolved version if they have the same type, and have
        // an evolution available. Otherwise just swap the two weapons.
        if (inSlot is null || grabbed is null || inSlot.Evolution == WeaponType.None ||
            inSlot.WeaponType != grabbed.WeaponType) return (grabbed, inSlot);
        
        inSlot = WeaponStats.Registry[inSlot.Evolution];
        grabbed = null;

        return (inSlot, grabbed);
    }

    public void DropGrabbed(Map map, float x, float z, int id)
    {
        if (GrabbedWeaponStats is null) return;

        map.DropWeapon(GrabbedWeaponStats.WeaponType, x, z, id);
        GrabbedWeaponStats = null;
    }

    public void UpdateInventory(UpdateInventory updateInventory)
    {
        if (updateInventory.Weapons is null) return;
        
        for (var i = 0; i < SlotCount; i++) Weapons[i] = WeaponStats.Registry[(WeaponType)updateInventory.Weapons[i]];

        EquippedWeaponStats = WeaponStats.Registry[(WeaponType)updateInventory.EquippedWeapon];
        GrabbedWeaponStats = WeaponStats.Registry[(WeaponType)updateInventory.EquippedWeapon];
    }
}