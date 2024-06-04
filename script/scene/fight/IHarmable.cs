using Godot;

/// <summary>
/// an object that can receive damage
/// </summary>
public interface IHarmable
{
    void ReceiveHit(ref SHitData data);

    bool CanReceiveHit();

    void KnockOut();

    RigidBody RBody { get; }

    void OutOfBound();
}
