using Godot;
using System.Runtime.CompilerServices;

public sealed class ABone3: ABone
{
    public enum EAxis { NONE, MAIN, FRONT, SIDE }

    public bool IsDirecting { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _directAxisCount > 0; }

    int _directAxisCount = 0;
    EAxis _dir0Axis;
    EAxis _dir1Axis;
    float _dir0Speed;
    float _dir1Speed;

    // Target direction 1
    public Vector3 TargetAxisWorld1
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set;
    }

    // axis offset
    public Basis TargetOffsetWorld
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _targetOffsetWorld;
    }
    Basis _targetOffsetWorld = Basis.Identity;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetTargetOffset(in Basis offset)
    {
        _targetOffsetWorld = offset;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ResetTargetOffset()
    {
        SetTargetOffset(Basis.Identity);
    }

    /// <summary>
    /// the unit vector of the direction of the anterior side of the Bone in local coordinates
    /// </summary>
    public Vector3 FrontAxis { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; } = Vector3.Back;
    /// <summary>
    /// the unit vector of the direction of the right side of the Bone in local coordinates
    /// </summary>
    public Vector3 SideAxis { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; } = Vector3.Right;

    /// <summary>
    /// the unit vector of the direction in world coordinates
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    Vector3 AxisWorld(EAxis a)
    {
        var res = a == EAxis.MAIN ? MainAxis : a == EAxis.FRONT ? FrontAxis : SideAxis;
        res = _targetOffsetWorld.XformInv(res);
        res = this.DirLocal2World(res);
        return res;
    }

    public Vector3 AxisWorldFront { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => this.DirLocal2World(FrontAxis); }

    public Vector3 AxisWorldSide { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => this.DirLocal2World(SideAxis); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetDirectTarget(in Vector3 worldDir, float rate) => SetDirectTarget(EAxis.MAIN, worldDir, rate);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirectTarget(EAxis axis, Vector3 worldDir) => SetDirectTarget(axis, worldDir, 1.0f);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirectTarget(EAxis axis, Vector3 worldDir, float rate)
    {
        _dir0Axis = axis;
        _dir0Speed = DefaultDirectionSpeed * rate;
        TargetAxisWorld0 = worldDir;

        _dir1Axis = EAxis.NONE;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirectTarget(EAxis axis1, in Vector3 worldDir1, EAxis axis2, in Vector3 worldDir2)
    {
        SetDirectTarget(axis1, worldDir1, 1.0f, axis2, worldDir2, 1.0f);
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirectTarget(EAxis axis1, in Vector3 worldDir1, float rate1, EAxis axis2, in Vector3 worldDir2, float rate2)
    {
        _dir0Axis = axis1;
        _dir0Speed = DefaultDirectionSpeed * rate1;
        TargetAxisWorld0 = worldDir1;

        _dir1Axis = axis2;
        _dir1Speed = DefaultDirectionSpeed * rate2;
        TargetAxisWorld1 = worldDir2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirectRate(float rate)
    {
        _dir0Speed = DefaultDirectionSpeed * rate;
    }

    /// <summary>
    /// Start the direction of the Bone in one world regardless of the current position of the Bone
    /// you can direct two axes (it doesn't make sense to direct more)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartDirect(EAxis axis1, in Vector3 worldDir1, float rate1, EAxis axis2, Vector3 worldDir2, float rate2)
    {
        SetDirectTarget(axis1, worldDir1, rate1, axis2, worldDir2, rate2);
        StartDirect();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartDirect(EAxis axis1, in Vector3 worldDir1, EAxis axis2, Vector3 worldDir2)
    {
        SetDirectTarget(axis1, worldDir1, axis2, worldDir2);
        StartDirect();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartDirect(EAxis axis, in Vector3 worldDir, float rate)
    {
        SetDirectTarget(axis, worldDir, rate);
        StartDirect();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartDirect(EAxis axis, in Vector3 worldDir)
    {
        SetDirectTarget(axis, worldDir);
        StartDirect();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void StartDirect()
    {
        _directAxisCount = _dir0Axis == EAxis.NONE ? 0 : _dir1Axis == EAxis.NONE ? 1 : 2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void StopDirect() => _directAxisCount = 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateDirectTarget(Vector3 worldDir0, Vector3 worldDir1)
    {
        TargetAxisWorld0 = worldDir0;
        TargetAxisWorld1 = worldDir1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void UpdateAxis(float step)
    {
        switch (_directAxisCount)
        {
            case 1:
                UpdateDirectSingleAxis();
                break;
            case 2:
                UpdateDirectDoubleAxis();
                break;
        }
    }

    /// <summary>
    /// updating the direction with one axis
    /// The current velocity along the axis is preserved
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void UpdateDirectSingleAxis()
    {
        // the original direction of the axis in world coordinates
        var ax = AxisWorld(_dir0Axis);

        // new velocity
        var total = Xts.AngularVelocity2Direction(ax, TargetAxisWorld0) * _dir0Speed;

        // + current velocity along the axis
        total += AngVel.ProjectionOn(ax); // ax * AngVel.Dot(ax);

        // total velocity
        AngVel = total;
    } // void UpdateDirectSingleAxis

    /// <summary>
    /// updating the direction with two axes
    /// the current Speed is completely overwritten
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void UpdateDirectDoubleAxis()
    {
        // new velocity on the first axis
        var total = Xts.AngularVelocity2Direction(AxisWorld(_dir0Axis), TargetAxisWorld0) * _dir0Speed;

        //  new velocity on the second axis
        total += Xts.AngularVelocity2Direction(AxisWorld(_dir1Axis), TargetAxisWorld1) * _dir1Speed;

        // итого скорость
        AngVel = total;
    } // void UpdateDirectDoubleAxis

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void CheckCollision2(PhysicsDirectBodyState state, PhysicsBody ob, int contactId) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Relax()
    {
        StopDirect();
        ResetTargetOffset();
    }

} // ABone3
