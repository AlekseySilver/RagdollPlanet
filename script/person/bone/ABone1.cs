using Godot;
using System.Runtime.CompilerServices;

public sealed class ABone1: ABone
{
    int _directState = 0;
    float _directSpeed;

    public AJoint Joint { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; }

    public bool IsDirecting { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _directState != 0; }

    /// <summary>
    /// impact force on dynamic objects
    /// </summary>
    public float HitForce { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; }

    public override ABone Parent
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => base.Parent;
        set
        {
            base.Parent = value;
            Parent1 = value as ABone1;
        }
    }

    public ABone1 Parent1 { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
    public ABone DirectParent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    Vector3 _directParentTargetLocal = Vector3.Zero;

    public Vector3 DirectParentTargetWorld
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            return DirectParent.DirLocal2World(_directParentTargetLocal);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FindDirectParent()
    {
        if (Parent != null)
            DirectParent = Parent.CollisionLayer == 0u ? Parent.Parent : Parent;
    }

    public override void _Ready()
    {
        base._Ready();
        HitForce = -1.0f; // off by default

        DefaultLimitSpeed = 500.0f;
    }

    protected override void UpdateAxis(float step)
    {
        if (_directState > 0)
        {
            if (_directState == 2)
            {
                TargetAxisWorld0 = DirectParentTargetWorld;
            }

            // the original direction of the axis in world coordinates
            var ax = AxisWorldMain;

            // new velocity
            var total = Xts.AngularVelocity2Direction(ax, TargetAxisWorld0) * _directSpeed;

            // + current velocity along the axis
            total += AngVel.ProjectionOn(ax);

            // total velocity
            AngVel = total;
        }
    } // UpdateAxis

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartDirectByParentSpace(Vector3 directParentSpace, float rate = 1.0f)
    {
        _directParentTargetLocal = directParentSpace;
        _directState = 2;
        _directSpeed = DefaultDirectionSpeed * rate;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartDirectByParentWorld(Vector3 directWorldSpace, float rate = 1.0f)
    {
        _directParentTargetLocal = DirectParent.DirWorld2Local(directWorldSpace);
        _directState = 2;
        _directSpeed = DefaultDirectionSpeed * rate;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 Euler2ParentDirectTarget(Vector3 euler)
    {
        return new Basis(euler * Xts.deg2rad).Xform(DirectParent.MainAxis);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 ParentDirectTarget2Euler(Vector3 target)
    {
        return Xts.FromRotationTo(DirectParent.MainAxis, target).GetEuler() * Xts.rad2deg;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void StartDirect()
    {
        StopLimit();
        _directState = 1;
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void StopDirect()
    {
        _directState = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void SetDirectTarget(in Vector3 worldDirection, float rate)
    {
        _directSpeed = DefaultDirectionSpeed * rate;
        TargetAxisWorld0 = worldDirection;
    }

    #region LIMITING

    /// <summary>
    /// the Rate of application of joint fixation (degrees per second)
    /// </summary>
    public float DefaultLimitSpeed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StopLimit() => Joint.Limit2Default();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartLimit(float range, float rate = 1.0f)
    {
        Joint.LimitNow(range);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LimitNow(float range) => Joint.LimitNow(range);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void LimitNow(float rangeLow, float rangeHigh) => Joint.LimitNow(rangeLow, rangeHigh);

    #endregion

    protected override void CheckCollision2(PhysicsDirectBodyState state, PhysicsBody ob, int contactId)
    {
        if (HitForce > 0.0f // Bone can do hit
                && ((ob.CollisionLayer & Xts.PERSON_LAYER_VALUE) != 0u)
                && ob is RigidBody rb
                && Person.CanHit(rb)
                && Person.ActiveBone == this
                )
        {
            var pos = state.GetContactLocalPosition(contactId); // in world space - origin
            var norm = state.GetContactLocalNormal(contactId); // already in world space
            DoHit(norm, pos, rb);
        }
    } // CheckCollision2

    void DoHit(Vector3 normal, Vector3 pos, RigidBody otherBody)
    {
        var vel = normal.Normalized();
        vel *= -HitForce;

        var data = new SHitData()
        {
            HitterBody = this,
            VictimBody = otherBody,
            Impulse = vel,
            WorldPosition = pos,
        };
        Person.TryHit(ref data);
    } // DoHit

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override void Relax()
    {
        StopDirect();
        StopLimit();
    }
} // ABone1
