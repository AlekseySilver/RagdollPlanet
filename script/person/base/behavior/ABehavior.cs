using Godot;
using System.Runtime.CompilerServices;

public partial class APerson {
    public Vector3 RunDirection3 { get; protected set; }
    protected Vector3 _runSide3;

    public ABone1[] Bones { get; protected set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void SetBodyLinVelOnFloor() {
        Body.LinVel = RunDirection3 * (MOVE_SPEED * SumControlLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetBodyLinVelOnFloor(float speed) {
        Body.LinVel = RunDirection3 * speed;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetBodyLinVel() {
        Body.LinVel = SumControlDirection * (MOVE_SPEED * SumControlLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void SetBodyLinVel(float speed) {
        Body.LinVel = SumControlDirection * speed;
    }

    /// <summary>
    /// calculate the Direction of running along the plane
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RecalcRunDirection() {
        RecalcRunDirection(GroundContactDirection);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RecalcRunDirection(in Vector3 groundDirection) {
        RecalcRunDirection(SumControlDirection, groundDirection);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void RecalcRunDirection(in Vector3 initDirection, in Vector3 groundDirection) {
        RunDirection3 = groundDirection.Cross(initDirection);
        _runSide3 = RunDirection3.Normalized();
        RunDirection3 = _runSide3.Cross(groundDirection);
    }
}
