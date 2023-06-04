namespace GraveZone;

public class ArrayList<T>
{
    public int Count { get; private set; }
    public T[] Array => _array;
    private readonly int _chunkSize;

    private T[] _array;

    public ArrayList(int chunkSize = 1024)
    {
        _chunkSize = chunkSize;
        _array = new T[chunkSize];
    }

    public T this[int i]
    {
        get => _array[i];
        set => _array[i] = value;
    }

    public void Add(T element)
    {
        while (Count >= _array.Length) Expand();

        _array[Count] = element;
        ++Count;
    }

    private void Expand()
    {
        System.Array.Resize(ref _array, _array.Length + _chunkSize);
    }

    public void Clear()
    {
        Count = 0;
    }
}