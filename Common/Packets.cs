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