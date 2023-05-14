using System;
using Microsoft.Xna.Framework;

namespace BulletHell;

public class ParticleEffect
{
    private const float Gravity = 80f;
    private const float StartVelocityY = 15f;
    private const float SpawnRadius = 0.25f;
    private const float SpawnDiameter = 2f * SpawnRadius;
    private static readonly Vector3 Size = new(0.6f, 0.6f, 0.6f);
    
    private struct Particle
    {
        public Vector3 Position;
        public Vector3 Velocity;
    }

    private ParticleEffectStats? _particleEffectStats;
    private Vector3 _position;
    private Particle[]? _particles;
    private float _lifeTimer;

    public ParticleEffect(ParticleEffectType particleEffectType, Vector3 position)
    {
        Init(particleEffectType, position);
    }

    public void Init(ParticleEffectType particleEffectType, Vector3 position)
    {
        _particleEffectStats = ParticleEffectStats.Registry[particleEffectType];
        _position = position;
        _particles = new Particle[_particleEffectStats.ParticleCount];
        _lifeTimer = 0f;

        for (var i = 0; i < _particles.Length; i++)
        {
            // We can use the thread's shared random because particles don't
            // have to be synchronized with anything else (unlike the map).
            _particles[i].Position.X = Random.Shared.NextSingle() * SpawnDiameter - SpawnRadius;
            _particles[i].Position.Z = Random.Shared.NextSingle() * SpawnDiameter - SpawnRadius;
            _particles[i].Velocity = _particles[i].Position;
            _particles[i].Velocity.Normalize();
            _particles[i].Velocity.Y = StartVelocityY;
        }
    }

    public bool Update(ClientMap map, float deltaTime)
    {
        _lifeTimer += deltaTime;

        for (var i = 0; i < _particles!.Length; i++)
        {
            var newParticlePosition = _particles[i].Position;
            newParticlePosition.X += _particles[i].Velocity.X * deltaTime;

            if (map.IsCollidingWithBox(_position + newParticlePosition, Size))
            {
                newParticlePosition.X = _particles[i].Position.X;
                _particles[i].Velocity.X *= -1f;
            }

            newParticlePosition.Z += _particles[i].Velocity.Z * deltaTime;

            if (map.IsCollidingWithBox(_position + newParticlePosition, Size))
            {
                newParticlePosition.Z = _particles[i].Position.Z;
                _particles[i].Velocity.Z *= -1f;
            }

            _particles[i].Velocity.Y -= Gravity * deltaTime;
            newParticlePosition.Y += _particles[i].Velocity.Y * deltaTime;
            
            if (newParticlePosition.Y < 0f)
            {
                newParticlePosition.Y = _particles[i].Position.Y;
                _particles[i].Velocity.Y *= -0.8f;
            }

            _particles[i].Position = newParticlePosition;
        }

        return _lifeTimer > _particleEffectStats!.LifeTime;
    }

    public void AddSprites(SpriteRenderer spriteRenderer)
    {
        for (var i = 0; i < _particles!.Length; i++)
        {
            spriteRenderer.Add(_position + _particles[i].Position, _particleEffectStats!.Sprite, ShadowType.Small);
        }
    }
}