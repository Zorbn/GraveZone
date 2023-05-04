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