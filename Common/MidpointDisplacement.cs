namespace Common;

public class MidpointDisplacement
{
    private struct Pass
    {
        public int X;
        public int Y;
        public int Size;
    }

    private const float Amplitude = 1f;
    private const float JitterAmplitude = 0.1f;

    public IReadOnlyList<float> Heightmap => _heightmap;

    private readonly int _mapSize;
    private readonly float[] _heightmap;
    private readonly Stack<Pass> _passes = new();

    public MidpointDisplacement(int exponent)
    {
        _mapSize = (int)Math.Pow(2, exponent) + 1;
        _heightmap = new float[_mapSize * _mapSize];
    }

    public void Generate(Random random)
    {
        Array.Fill(_heightmap, 0f);

        SetHeight(0, RandomHeight(random));
        SetHeight(_mapSize - 1, RandomHeight(random));
        SetHeight((_mapSize - 1) * _mapSize, RandomHeight(random));
        SetHeight(_mapSize * _mapSize - 1, RandomHeight(random));

        _passes.Clear();
        _passes.Push(new Pass { X = 0, Y = 0, Size = _mapSize });

        while (_passes.TryPop(out var nextPass)) DoPass(nextPass.X, nextPass.Y, nextPass.Size, random);
    }

    private void DoPass(int x, int y, int size, Random random)
    {
        var tl = x + y * _mapSize;
        var tr = tl + size - 1;
        var bl = tl + (size - 1) * _mapSize;
        var br = tl + (size - 1) * _mapSize + size - 1;

        var topMidpoint = MidpointIndex(tl, tr);
        SetHeight(topMidpoint, (_heightmap[tl] + _heightmap[tr]) * 0.5f + RandomJitter(random));
        var bottomMidpoint = MidpointIndex(bl, br);
        SetHeight(bottomMidpoint, (_heightmap[bl] + _heightmap[br]) * 0.5f + RandomJitter(random));
        var leftMidpoint = MidpointIndex(tl, bl);
        SetHeight(leftMidpoint, (_heightmap[tl] + _heightmap[bl]) * 0.5f + RandomJitter(random));
        var rightMidpoint = MidpointIndex(tr, br);
        SetHeight(rightMidpoint, (_heightmap[tr] + _heightmap[br]) * 0.5f + RandomJitter(random));

        var center = MidpointIndex(tl, br);
        SetHeight(center,
            (_heightmap[topMidpoint] + _heightmap[bottomMidpoint] + _heightmap[leftMidpoint] +
             _heightmap[rightMidpoint]) * 0.25f + RandomJitter(random));

        var nextSize = (int)Math.Ceiling(size * 0.5f);
        if (nextSize < 3) return;
        var offset = nextSize - 1;

        _passes.Push(new Pass { X = x, Y = y, Size = nextSize });
        _passes.Push(new Pass { X = x + offset, Y = y, Size = nextSize });
        _passes.Push(new Pass { X = x, Y = y + offset, Size = nextSize });
        _passes.Push(new Pass { X = x + offset, Y = y + offset, Size = nextSize });
    }

    private static float RandomHeight(Random random)
    {
        return random.NextSingle() * Amplitude;
    }

    private static float RandomJitter(Random random)
    {
        return random.NextSingle() * JitterAmplitude - JitterAmplitude * 0.5f;
    }

    private void SetHeight(int i, float value)
    {
        if (_heightmap[i] != 0f) return;

        _heightmap[i] = Math.Clamp(value, 0f, 1f);
    }

    private int MidpointIndex(int a, int b)
    {
        return a + (b - a) / 2;
    }
}