namespace Common;

public class Inventory
{
    public const int Width = 10;
    public const int Height = 2;
    public const int SlotCount = Width * Height;
    
    public Weapon EquippedWeapon { get; private set; }
    public readonly Weapon[] Weapons = new Weapon[Width * Height];
    public Weapon GrabbedWeapon { get; private set; }

    public bool IsFull()
    {
        for (var i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i] is null)
            {
                return false;
            }
        }

        return true;
    }

    // Returns true if the weapon was successfully added, false otherwise (ie: the inventory is full).
    public bool AddWeapon(WeaponType weaponType)
    {
        var weapon = Weapon.Registry[weaponType];
        
        for (var i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i] is not null) continue;

            Weapons[i] = weapon;

            return true;
        }

        return false;
    }

    public Weapon RemoveWeapon(int i)
    {
        var weapon = Weapons[i];
        Weapons[i] = null;

        return weapon;
    }
    
    public void GrabSlot(int i)
    {
        if (GrabbedWeapon is null)
        {
            var removedWeapon = RemoveWeapon(i);
            GrabbedWeapon = removedWeapon;
            return;
        }
        
        (Weapons[i], GrabbedWeapon) = (GrabbedWeapon, Weapons[i]);
    }
    
    public void GrabEquippedSlot()
    {
        if (GrabbedWeapon is null)
        {
            GrabbedWeapon = EquippedWeapon;
            EquippedWeapon = null;
            return;
        }
        
        (EquippedWeapon, GrabbedWeapon) = (GrabbedWeapon, EquippedWeapon);
    }

    public void DropGrabbed(Map map, float x, float z, int id)
    {
        if (GrabbedWeapon is null) return;

        map.DropWeapon(GrabbedWeapon.WeaponType, x, z, id);
        GrabbedWeapon = null;
    }

    public void UpdateInventory(UpdateInventory updateInventory)
    {
        for (var i = 0; i < SlotCount; i++)
        {
            Weapons[i] = Weapon.Registry[(WeaponType)updateInventory.Weapons[i]];
        }

        EquippedWeapon = Weapon.Registry[(WeaponType)updateInventory.EquippedWeapon];
        GrabbedWeapon = Weapon.Registry[(WeaponType)updateInventory.EquippedWeapon];
    }
}