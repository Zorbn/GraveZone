using System.Collections.Generic;
using Common;

namespace BulletHell;

public class ParticleEffectStats
{
    public static readonly Dictionary<ParticleEffectType, ParticleEffectStats> Registry = new();

    static ParticleEffectStats()
    {
        Register(new ParticleEffectStats(ParticleEffectType.Hit, 1f, 2, Sprite.Hit));
    }

    public readonly ParticleEffectType ParticleEffectType;
    public readonly float LifeTime;
    public readonly int ParticleCount;
    public readonly Sprite Sprite;

    private ParticleEffectStats(ParticleEffectType particleEffectType, float lifeTime, int particleCount, Sprite sprite)
    {
        ParticleEffectType = particleEffectType;
        LifeTime = lifeTime;
        ParticleCount = particleCount;
        Sprite = sprite;
    }

    private static void Register(ParticleEffectStats particleEffectStats)
    {
        Registry.Add(particleEffectStats.ParticleEffectType, particleEffectStats);
    }
}