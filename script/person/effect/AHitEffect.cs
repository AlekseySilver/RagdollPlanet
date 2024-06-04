using Godot;
using System;

public class AHitEffect: Spatial
{
    [Export] public float GRAVITY_SCALE = 0.5f;

    CPUParticles _particles;
    AudioStreamPlayer _sound;

    public override void _Ready()
    {
        _particles = this.FirstChild<CPUParticles>();
        _sound = this.FirstChild<AudioStreamPlayer>();
    }

    /// <summary>
    /// starting the effect
    /// </summary>
    public void Fire(ref SHitData data)
    {
        _sound.Play();

        FireEmit(data.WorldPosition + data.VictimBody.GlobalTranslation, data.Impulse);
    } // void Fire

    void FireEmit(Vector3 worldPosition, Vector3 worldDirection)
    {
        this.GlobalTranslation = worldPosition;

        _particles.Gravity = worldDirection * GRAVITY_SCALE;
        _particles.Emitting = true;
    } // void FireEmit
}
