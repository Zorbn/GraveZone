using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BulletHell;

public class ParticlePool
{
    public IReadOnlyList<ParticleEffect> ParticleEffects => _particleEffects;

    private readonly List<ParticleEffect> _particleEffects = new();
    private readonly Stack<ParticleEffect> _unusedParticleEffects = new();

    public void SpawnParticle(ParticleEffectType particleEffectType, Vector3 position)
    {
        if (_unusedParticleEffects.TryPop(out var newParticleEffect))
        {
            newParticleEffect.Init(particleEffectType, position);
        }
        else
        {
            newParticleEffect = new ParticleEffect(particleEffectType, position);
        }
        
        _particleEffects.Add(newParticleEffect);
    }

    public void DespawnParticle(int i)
    {
        var particleEffect = _particleEffects[i];
        _particleEffects.RemoveAt(i);
        _unusedParticleEffects.Push(particleEffect);
    }
}