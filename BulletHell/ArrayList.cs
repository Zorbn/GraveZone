namespace BulletHell;

public class ArrayList<T>
{
    public int Count { get; private set; }
    public T[] Array { get; private set; }
    public readonly int ChunkSize;

    public ArrayList(int chunkSize = 1024)
    {
        ChunkSize = chunkSize;
        Array = new T[chunkSize];
    }

    public T this[int i]
    {
        get => Array[i];
        set => Array[i] = value;
    }

    public void Add(T element)
    {
        while (Count >= Array.Length)
        {
            Expand();
        }

        Array[Count] = element;
        ++Count;
    }

    private void Expand()
    {
        var newArray = new T[Array.Length + ChunkSize];
        System.Array.Copy(Array, newArray, Count);
        Array = newArray;
    }

    public void Clear()
    {
        Count = 0;
    }
}