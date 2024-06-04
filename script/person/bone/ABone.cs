using Godot;
using System.Runtime.CompilerServices;


public abstract class ABone: APersonRigidBody
{
    public enum EPart
    {
        Body = 0, Head,
        LUArm0, RUArm0,
        LUArm1, RUArm1,
        LFArm, RFArm,
        LThigh0, RThigh0,
        LThigh1, RThigh1,
        LCalf, RCalf,
        LUMid0, RUMid0,
        LUMid1, RUMid1,
        LFMid, RFMid,
    }

    #region INIT

    protected ABone _parent = null;

    // link to Parent Bone
    public virtual ABone Parent { get => _parent; set => _parent = value; }
    public CollisionShape Shape { get; protected set; }

    // skeleton
    public int SkBoneIdx = 0; // Bone id in skeleton

    public int SkBoneParentIdx { get; set; } = -1; // Parent Bone id in skeleton (-1 if no Parent)
    public Vector3 SkBoneLocalOrigin { get; set; } // skeleton Bone Position
    public Basis SkBoneGlobalBasis { get; set; } // skeleton Bone global rotaion
    public Basis RBody2SkBoneOffset { get; set; } // rotation from rigidbody 2 skeleton Bone

    /// <summary>
    /// 0 for body
    /// </summary>
    public int LimbLevel => Parent == null ? 0 : Parent.LimbLevel + 1;

    public Basis RBodyBasis => Transform.basis;

    public bool HasGroundContact { get; private set; } = false;
    public Vector3 LastGroundContactNorm { get; private set; }
    public Vector3 LastGroundContactPos { get; private set; }

    public override void _Ready()
    {
        Shape = this.FirstChild<CollisionShape>();

        DefaultDirectionSpeed = 20.0f;
    }

    #endregion

    #region DIRECTING
    public float DefaultDirectionSpeed { get; set; }

    public virtual Vector3 TargetAxisWorld0
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected set;
    } // Target direction 0

    /// <summary>
    /// the unit vector of the main direction of the Bone in local coordinates
    /// </summary>
    public Vector3 MainAxis { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; } = Vector3.Up;

    public Vector3 AxisWorldMain { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => this.DirLocal2World(MainAxis); }

    /// <summary>
    /// how far has the direction Target been achieved (1 - achieved, -1 - opposite, 0 - perpendicular
    /// </summary>
    public float DirectDone { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => DirectDot(TargetAxisWorld0); }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float DirectDot(in Vector3 targetWorld) => AxisWorldMain.Dot(targetWorld);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateDirectTarget(in Vector3 worldDirection) => TargetAxisWorld0 = worldDirection;

    public abstract void StartDirect();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartDirect(in Vector3 worldDirection, float rate)
    {
        SetDirectTarget(worldDirection, rate);
        StartDirect();
    }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void StartDirect(in Vector3 worldDirection) => StartDirect(worldDirection, 1.0f);

    public abstract void StopDirect();

    public abstract void SetDirectTarget(in Vector3 worldDirection, float rate);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetDirectTarget(in Vector3 worldDirection) => SetDirectTarget(worldDirection, 1.0f);

    protected abstract void UpdateAxis(float step);

    /// <summary>
    /// Bone UpdateOverride
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override void Custom(PhysicsDirectBodyState state)
    {
        UpdateAxis(state.Step);
        CheckCollisions(state);
    }

    /// <summary>
    /// Bone direct
    /// </summary>
    /// <param Name="localAxis">the local axis to be directed in the direction of force</param>
    /// <param Name="worldForceDirection">the direction of force in world coordinates</param>
    protected void ApplyDirect(in Vector3 localAxis, in Vector3 worldForceDirection, float force)
    {
        var dir = this.DirLocal2World(localAxis);
        var torque = dir.Cross(worldForceDirection); // |torque| == |force|, when dir is perpendicular to force, because |dir| == 1
        float dot = dir.Dot(worldForceDirection);

        if (dot < 0.0f) // if dir and force are directed in different directions
        {
            torque = torque.Normalized();
        }

        AngVel += torque * force;
        //RBody.ApplyTorqueImpulse(torque * force);
        //RBody.ApplyTorque(torque * force);
    }

    #endregion

    #region COLLISION

    protected abstract void CheckCollision2(PhysicsDirectBodyState state, PhysicsBody ob, int contactId);

    void CheckCollisions(PhysicsDirectBodyState state)
    {
        HasGroundContact = false;
        int cc = state.GetContactCount();
        for (int i = 0; i < cc; ++i)
        {
            if (state.GetContactColliderObject(i) is PhysicsBody ob)
            {
                if (ob.CollisionLayer != Xts.GROUND_LAYER_VALUE)
                {
                    CheckCollision2(state, ob, i);
                }
                else
                {
                    HasGroundContact = true;
                    LastGroundContactNorm = state.GetContactLocalNormal(i); // already in world space
                    LastGroundContactPos = state.GetContactLocalPosition(i); // in world space - origin
                    LastGroundContactPos += ob.GlobalTranslation;
                }
            } // if
        } // for
    } // void NodeCollisionStart

    #endregion

    public abstract void Relax();
}
