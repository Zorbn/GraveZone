using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BulletHell;

public class ParticlePool
{
    public List<ParticleEffect> ParticleEffects { get; } = new();

    private readonly Stack<ParticleEffect> _unusedParticleEffects = new();

    public void SpawnParticle(ParticleEffectType particleEffectType, Vector3 position)
    {
        if (_unusedParticleEffects.TryPop(out var newParticleEffect))
            newParticleEffect.Init(particleEffectType, position);
        else
            newParticleEffect = new ParticleEffect(particleEffectType, position);

        ParticleEffects.Add(newParticleEffect);
    }

    public void DespawnParticle(int i)
    {
        var particleEffect = ParticleEffects[i];
        ParticleEffects.RemoveAt(i);
        _unusedParticleEffects.Push(particleEffect);
    }
}