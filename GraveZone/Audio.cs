using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

namespace GraveZone;

public class Audio
{
    public const float PositionalAudioScale = 2f;

    private const float MasterVolume = 0.2f;
    private const float PitchVariance = 0.33f;

    private readonly Dictionary<Sound, SoundEffect> _sounds = new();
    private readonly Random _random = new();
    private readonly AudioEmitter _audioEmitter = new();

    private readonly Dictionary<Sound, List<SoundEffectInstance>> _soundEffectInstancesPerSound = new();
    private readonly Dictionary<Sound, Stack<SoundEffectInstance>> _unusedSoundEffectInstancesPerSound = new();

    public void AddSound(ContentManager contentManager, Sound sound, string contentName)
    {
        var soundEffect = contentManager.Load<SoundEffect>(contentName);
        _soundEffectInstancesPerSound[sound] = new List<SoundEffectInstance>();
        _unusedSoundEffectInstancesPerSound[sound] = new Stack<SoundEffectInstance>();
        _sounds.Add(sound, soundEffect);
    }

    public void PlaySoundPositionalWithPitch(AudioListener listener, Sound sound, Vector3 position, float volume = 1f, float pan = 0f)
    {
        PlaySoundPositional(listener, sound, position, volume, PitchVariance * _random.NextSingle(), pan);
    }

    private void PlaySoundPositional(AudioListener listener, Sound sound, Vector3 position, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        _audioEmitter.Position = position * Audio.PositionalAudioScale;
        var soundEffectInstance = GetSoundEffectInstance(sound, volume, pitch, pan);
        soundEffectInstance.Play();
        soundEffectInstance.Apply3D(listener, _audioEmitter);
    }

    public void PlaySoundWithPitch(Sound sound, float volume = 1f, float pan = 0f)
    {
        PlaySound(sound, volume, PitchVariance * _random.NextSingle(), pan);
    }

    private void PlaySound(Sound sound, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        var soundEffectInstance = GetSoundEffectInstance(sound, volume, pitch, pan);
        soundEffectInstance.Play();
    }

    private SoundEffectInstance GetSoundEffectInstance(Sound sound, float volume = 1f, float pitch = 0f, float pan = 0f)
    {
        var soundEffect = _sounds[sound];

        if (!_unusedSoundEffectInstancesPerSound[sound].TryPop(out var soundEffectInstance))
            soundEffectInstance = soundEffect.CreateInstance();

        _soundEffectInstancesPerSound[sound].Add(soundEffectInstance);

        soundEffectInstance.Volume = volume * MasterVolume;
        soundEffectInstance.Pitch = pitch;
        soundEffectInstance.Pan = pan;

        return soundEffectInstance;
    }

    public void Update()
    {
        foreach (var (sound, soundEffectInstances) in _soundEffectInstancesPerSound)
        {
            var unusedSoundEffectInstances = _unusedSoundEffectInstancesPerSound[sound];

            for (var i = soundEffectInstances.Count - 1; i >= 0; i--)
            {
                if (soundEffectInstances[i].State == SoundState.Playing) continue;

                var particleEffect = soundEffectInstances[i];
                soundEffectInstances.RemoveAt(i);
                unusedSoundEffectInstances.Push(particleEffect);
            }
        }
    }
}