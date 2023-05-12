namespace Common;

public class Inventory
{
    public const int Width = 10;
    public const int Height = 2;
    public const int SlotCount = Width * Height;

    public WeaponStats EquippedWeaponStats { get; private set; }
    public readonly WeaponStats[] Weapons = new WeaponStats[Width * Height];
    public WeaponStats GrabbedWeaponStats { get; private set; }

    public bool IsFull()
    {
        for (var i = 0; i < Weapons.Length; i++)
            if (Weapons[i] is null)
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

    public WeaponStats RemoveWeapon(int i)
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

        (Weapons[i], GrabbedWeaponStats) = (GrabbedWeaponStats, Weapons[i]);
    }

    public void GrabEquippedSlot()
    {
        if (GrabbedWeaponStats is null)
        {
            GrabbedWeaponStats = EquippedWeaponStats;
            EquippedWeaponStats = null;
            return;
        }

        (EquippedWeaponStats, GrabbedWeaponStats) = (GrabbedWeaponStats, EquippedWeaponStats);
    }

    public void DropGrabbed(Map map, float x, float z, int id)
    {
        if (GrabbedWeaponStats is null) return;

        map.DropWeapon(GrabbedWeaponStats.WeaponType, x, z, id);
        GrabbedWeaponStats = null;
    }

    public void UpdateInventory(UpdateInventory updateInventory)
    {
        for (var i = 0; i < SlotCount; i++) Weapons[i] = WeaponStats.Registry[(WeaponType)updateInventory.Weapons[i]];

        EquippedWeaponStats = WeaponStats.Registry[(WeaponType)updateInventory.EquippedWeapon];
        GrabbedWeaponStats = WeaponStats.Registry[(WeaponType)updateInventory.EquippedWeapon];
    }
}