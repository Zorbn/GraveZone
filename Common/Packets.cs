namespace Common;

public class SetLocalId
{
    public int Id { get; set; }
}

public class PlayerSpawn
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
}

public class PlayerDespawn
{
    public int Id { get; set; }
}

public class PlayerMove
{
    public int Id { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
}

// Sent from the client to the server when a player attacks.
public class PlayerAttack
{
    public NetVector3 Direction { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
}

// Sent from the server to the client when a projectile is spawned.
public class ProjectileSpawn
{
    public NetVector3 Direction { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
}

public class MapGenerate
{
    public int Seed { get; set; }
}

public class DroppedWeaponSpawn
{
    public WeaponType WeaponType { get; set; }
    public float X { get; set; }
    public float Z { get; set; }
    public int Id { get; set; }
}

public class RequestPickupWeapon
{
    public int DroppedWeaponId { get; set; }
}

public class PickupWeapon
{
    public int PlayerId { get; set; }
    public int DroppedWeaponId { get; set; }
    public WeaponType WeaponType { get; set; }
}

public class RequestGrabSlot
{
    public int SlotIndex { get; set; }
}

public class GrabSlot
{
    public int PlayerId { get; set; }
    public int SlotIndex { get; set; }
}
public class RequestGrabEquippedSlot
{
}

public class GrabEquippedSlot
{
    public int PlayerId { get; set; }
}

public class RequestDropGrabbed
{
}

public class DropGrabbed
{
    public float X { get; set; }
    public float Z { get; set; }
    public int DroppedWeaponId { get; set; }
    public int PlayerId { get; set; }
}

public class UpdateInventory
{
    public int PlayerId { get; set; }
    public int[] Weapons { get; set; }
    public int EquippedWeapon { get; set; }
    public int GrabbedWeapon { get; set; }
}