using Godot;
using System.Runtime.CompilerServices;

public abstract partial class APerson
{
    public AGrabDetector _grabDetector;

    RigidBody[] _harmBodies;

    #region bones
    public ABone1 Head { get; private set; }
    public ABone1 LUArm0 { get; private set; }
    public ABone1 RUArm0 { get; private set; }
    public ABone1 LUArm1 { get; private set; }
    public ABone1 RUArm1 { get; private set; }
    public ABone1 LFArm { get; private set; }
    public ABone1 RFArm { get; private set; }
    public ABone1 LThigh0 { get; private set; }
    public ABone1 RThigh0 { get; private set; }
    public ABone1 LThigh1 { get; private set; }
    public ABone1 RThigh1 { get; private set; }
    public ABone1 LCalf { get; private set; }
    public ABone1 RCalf { get; private set; }

    public ABone1 AThigh0, BThigh0, AThigh1, BThigh1, ACalf, BCalf, AUArm0, BUArm0, AUArm1, BUArm1, AFArm, BFArm;
    #endregion

    void InitBehavior()
    {
        #region bones
        Head = (ABone1)_bones[ABone.EPart.Head];
        LUArm0 = (ABone1)_bones[ABone.EPart.LUArm0];
        RUArm0 = (ABone1)_bones[ABone.EPart.RUArm0];
        LUArm1 = (ABone1)_bones[ABone.EPart.LUArm1];
        RUArm1 = (ABone1)_bones[ABone.EPart.RUArm1];
        LFArm = (ABone1)_bones[ABone.EPart.LFArm];
        RFArm = (ABone1)_bones[ABone.EPart.RFArm];
        LThigh0 = (ABone1)_bones[ABone.EPart.LThigh0];
        RThigh0 = (ABone1)_bones[ABone.EPart.RThigh0];
        LThigh1 = (ABone1)_bones[ABone.EPart.LThigh1];
        RThigh1 = (ABone1)_bones[ABone.EPart.RThigh1];
        LCalf = (ABone1)_bones[ABone.EPart.LCalf];
        RCalf = (ABone1)_bones[ABone.EPart.RCalf];
        #endregion

        var bodyRID = Body.GetRid();
        PhysicsServer.BodyAddCollisionException(bodyRID, LUArm1.GetRid());
        PhysicsServer.BodyAddCollisionException(bodyRID, RUArm1.GetRid());
        PhysicsServer.BodyAddCollisionException(bodyRID, LThigh1.GetRid());
        PhysicsServer.BodyAddCollisionException(bodyRID, RThigh1.GetRid());



        LCalf.HitForce = RCalf.HitForce = LFArm.HitForce = RFArm.HitForce = MAX_HIT_FORCE;

        LFArm.MainAxis = Vector3.Back;
        RFArm.MainAxis = Vector3.Forward;
        LUArm1.MainAxis = Vector3.Back;
        RUArm1.MainAxis = Vector3.Forward;
        LThigh1.MainAxis = Vector3.Back;
        RThigh1.MainAxis = Vector3.Back;
        LCalf.MainAxis = Vector3.Back;
        RCalf.MainAxis = Vector3.Back;

        Bones = new ABone1[] { Head, LUArm1, RUArm1, LFArm, RFArm, LThigh1, RThigh1, LCalf, RCalf };
        _harmBodies = new RigidBody[] { Body, Head };

        _grabDetector = Body.FirstChild<AGrabDetector>(true);
        _grabDetector.InitBehavior(this);

        // anim
        _runAnim = AAnimation.Create(anim_run_resource, this);
        _walkAnim = AAnimation.Create(anim_walk_resource, this);
        _animKickAirL = AAnimation.Create(anim_kick_air_L_resource, this);
        _animKickAirR = AAnimation.Create(anim_kick_air_R_resource, this);
        _animKickGroundL = AAnimation.Create(anim_kick_ground_L_resource, this);
        _animKickGroundR = AAnimation.Create(anim_kick_ground_R_resource, this);
        _animPunchAirL = AAnimation.Create(anim_punch_air_L_resource, this);
        _animPunchAirR = AAnimation.Create(anim_punch_air_R_resource, this);
        _animPunchGroundL = AAnimation.Create(anim_punch_ground_L_resource, this);
        _animPunchGroundR = AAnimation.Create(anim_punch_ground_R_resource, this);
    } // InitBehavior

    public void Relax()
    {
        Body.Relax();
        Head.Relax();
        RThigh0.Relax();
        LThigh0.Relax();
        RThigh1.Relax();
        LThigh1.Relax();
        RCalf.Relax();
        LCalf.Relax();
        RUArm0.Relax();
        LUArm0.Relax();
        RUArm1.Relax();
        LUArm1.Relax();
        RFArm.Relax();
        LFArm.Relax();
    }

    public void SidesSwap()
    {
        if (AThigh0 == LThigh0)
        {
            AThigh0 = RThigh0;
            BThigh0 = LThigh0;
            AThigh1 = RThigh1;
            BThigh1 = LThigh1;
            ACalf = RCalf;
            BCalf = LCalf;
            AUArm0 = RUArm0;
            BUArm0 = LUArm0;
            AUArm1 = RUArm1;
            BUArm1 = LUArm1;
            AFArm = RFArm;
            BFArm = LFArm;
        }
        else
        {
            AThigh0 = LThigh0;
            BThigh0 = RThigh0;
            AThigh1 = LThigh1;
            BThigh1 = RThigh1;
            ACalf = LCalf;
            BCalf = RCalf;
            AUArm0 = LUArm0;
            BUArm0 = RUArm0;
            AUArm1 = LUArm1;
            BUArm1 = RUArm1;
            AFArm = LFArm;
            BFArm = RFArm;
        }
    } // SidesSwap
    public void SidesA2Right(bool right)
    {
        if (right != (AThigh1 == RThigh1))
            SidesSwap();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public RigidBody GetWeakPoint()
    {
        return Xts.RndNextFloat() > 0.7f ? Head : (RigidBody)Body;
    }

    public void GroundKickDirect()
    {
        var bone = ActiveBone;
        if (bone == null)
            return;

        var axisSide = bone.TargetAxisWorld0;
        if (bone == RCalf)
            axisSide = -axisSide;
        else if (bone != LCalf)
            return;
        var axMain = -bone.TargetAxisWorld0;
        axMain += UpDirection;
        axMain = axMain.Normalized();
        Body.StartDirect(ABone3.EAxis.MAIN, axMain, 0.2f, ABone3.EAxis.SIDE, axisSide, 0.7f);
    } // GroundKickDirect


    public void GroundingRollStart()
    {
        GravitySetInAir();
        BonesGravity2Floor();
        Group();
        _FSMActionRestTime = 0.0f;
    } // GroundingRollStart

    public void Grab_GrabTryStart()
    {
        _FSMActionRestTime = 3.0f;
        GravitySetGrab();
        LUArm0.StartLimit(0.5f, 1.0f);
        RUArm0.StartLimit(0.5f, 1.0f);
    } // Grab_GrabTryStart

    public void Grab_GrabTryJumpOff()
    {
        LUArm0.StopLimit();
        RUArm0.StopLimit();
    } // Grab_GrabTryJumpOff

    public void GrabbedSwingGroupStart()
    {
        LThigh0.StartLimit(0.0f, .5f); // bended
        RThigh0.StartLimit(0.0f, .5f); // bended

        LCalf.StartLimit(1.0f, 0.5f); // bended
        RCalf.StartLimit(1.0f, 0.5f); // bended

        LUArm0.StartLimit(0.5f, 0.5f); // almost along the body
        RUArm0.StartLimit(0.5f, 0.5f); // almost along the body

        LFArm.StartLimit(1.0f, 0.5f); // bended
        RFArm.StartLimit(1.0f, 0.5f); // bended
    } // GrabbedSwingGroupStart

    public void GrabbedSwingStraingthStart()
    {
        Head.LimitNow(0.5f);

        LThigh0.StartLimit(0.7f, 0.5f);
        RThigh0.StartLimit(0.7f, 0.5f);

        LThigh1.StartLimit(0.0f, 0.5f);
        RThigh1.StartLimit(0.0f, 0.5f);

        LCalf.StartLimit(0.0f, 0.5f);
        RCalf.StartLimit(0.0f, 0.5f);

        LUArm0.StartLimit(1.0f, 0.5f);
        RUArm0.StartLimit(1.0f, 0.5f);

        LUArm1.StartLimit(0.0f, 0.5f);
        RUArm1.StartLimit(0.0f, 0.5f);

        LFArm.StartLimit(0.0f, 0.5f);
        RFArm.StartLimit(0.0f, 0.5f);
    } // GrabbedSwingStraingthStart

    public void ControlledFallStart()
    {
        if (_airControlDirection == Vector3.Zero)
        {
            // in case you went into a fall after running or some other non-jumping movement
            _airControlDirection = SumControlDirection.InPlane(UpDirection);
        }

        Straight();
    } // ControlledFallStart

    public bool ControlledFallCheckFinish()
    {
        SetBodyVelInAir();
        return !NeedControlledFly();
    } // ControlledFallCheckFinish

    public void WallSlideStart()
    {
        var ax1 = UpDirection;
        bool head = true;
        if (!Xts.IsIn(_FSM.PrevActionKey, EGMAction.WALL_RUN_HORIZ, EGMAction.JUMP_ON_WALL_VERT))
        {
            head = ax1.Dot(Body.AxisWorldMain) > 0.0f; // Head up
            if (!head)
                ax1 = -ax1;
        }
        var ax2 = GroundContactDirection;
        Body.StartDirect(ABone3.EAxis.MAIN, ax1, ABone3.EAxis.FRONT, ax2);

        Head.StartLimit(0.0f);

        SidesA2Right(Body.AxisWorldSide.Dot(GroundContactDirection) > 0.0f);

        if (head)
        {
            AThigh1.StartLimit(0.0f);
            BThigh1.StartLimit(0.0f);
            AThigh0.StartLimit(0.4f); // bended 90
            BThigh0.StartLimit(0.7f); // along the body

            ACalf.StartLimit(1.0f); // bended
            BCalf.StartLimit(0.0f); // straight

            AUArm1.StartLimit(0.5f);
            BUArm1.StartLimit(0.5f);
            AUArm0.StartLimit(0.4f); // almost straight
            BUArm0.StartLimit(0.4f); // straight

            AFArm.StartLimit(1.0f); // bended
            BFArm.StartLimit(0.0f); // straight
        }
        else
        {
            AThigh1.StartLimit(0.0f);
            BThigh1.StartLimit(0.0f);
            AThigh0.StartLimit(0.4f); // bended 90
            BThigh0.StartLimit(0.4f); // bended 90

            ACalf.StartLimit(1.0f); // bended
            BCalf.StartLimit(1.0f); // bended

            AUArm1.StartLimit(0.0f);
            BUArm1.StartLimit(0.0f);
            AUArm0.StartLimit(0.9f); // along the body up
            BUArm0.StartLimit(0.4f); // straight              

            AFArm.StartLimit(0.0f); // straight
            BFArm.StartLimit(1.0f); // bended
        }
    } // WallSlideStart

    public void Group()
    {
        Head.StartLimit(0.0f); // bended
        Body.Relax();

        LThigh0.StartLimit(0.0f); // bended
        RThigh0.StartLimit(0.0f); // bended

        LThigh1.StartLimit(0.2f); // straight
        RThigh1.StartLimit(0.2f); // straight

        LCalf.StartLimit(0.8f); // bended
        RCalf.StartLimit(0.8f); // bended

        LUArm0.StartLimit(0.5f); // almost along the body
        RUArm0.StartLimit(0.5f); // almost along the body

        LUArm1.StartLimit(0.0f); // straight
        RUArm1.StartLimit(0.0f); // straight

        LFArm.StartLimit(0.8f); // bended
        RFArm.StartLimit(0.8f); // bended
    } // Group

    /// <summary>
    /// all straigth, hands up
    /// </summary>
    public void Straight()
    {
        Head.StartLimit(0.5f);

        LThigh0.StartLimit(0.7f);
        RThigh0.StartLimit(0.7f);

        LThigh1.StartLimit(0.0f);
        RThigh1.StartLimit(0.0f);

        LCalf.StartLimit(0.0f);
        RCalf.StartLimit(0.0f);

        LUArm0.StartLimit(1.0f);
        RUArm0.StartLimit(1.0f);

        LUArm1.StartLimit(0.0f);
        RUArm1.StartLimit(0.0f);

        LFArm.StartLimit(0.0f);
        RFArm.StartLimit(0.0f);
    } // Straight

    public void BodyDirCheck4Limb()
    {
        var dir = LastControlDirection;
        if (Body.DirectDot(dir) < 0.0f)
        {
            if (!Xts.IsIn(ActiveBone, LFArm, RFArm))
                Xts.Reverse(ref dir);
            Body.StartDirect(dir);
        }
        else if (Body.IsDirecting)
            Body.StopDirect();
    }

    public bool IsHeadUp => Body.DirectDot(UpDirection) > 0.0f;

    public bool Grab_IsGrabbed => _grabDetector.IsGrabbed;
    public bool Grab_IsGrabbedTotal => _grabDetector.IsGrabbedTotal;
    public bool Grab_CanGrab => _grabDetector.CanGrab;
    public void Grab_ReleaseGrab()
    {
        _grabDetector.ReleaseGrab();
        _type.GrabFinish(this);
    } // Grab_ReleaseGrab

    public Vector3 Grab_BarDirection()
    {
        return _grabDetector.Direction;
    }
    public Vector3 Grab_BarTangent()
    {
        return _grabDetector.GetBarTangent();
    }

    public Vector3 GetGrabPoint()
    {
        return _grabDetector.GetGrabPoint();
    }

    public bool Grab_NeedGrabTry() => _grabDetector.IsOneHandGrabbed;
    public bool Grab_GrabTryCheckFinish()
    {
        _FSMActionRestTime -= A.FixedStep;

        if (_grabDetector.UpdateGrab(A.FixedStep))
        {
            _type.GrabStart(this);
            return true;
        }

        // body to the horizontal bar
        Body.StartDirect((_grabDetector.BarRigidBody.GetGlobalPos() - Body.Position).Normalized());

        return false;
    }

    public void Grab_Regrab() => _grabDetector.SetNewGrabPoint();
    public PhysicsBody Grab_GetGrabbedBody() => _grabDetector.GrabbedBody;

    public RigidBody[] HarmBodies => _harmBodies;

    public void SetLinVel(in Vector3 vel)
    {
        Body.LinVel = vel;
        LThigh1.LinVel = vel;
        RThigh1.LinVel = vel;
        LCalf.LinVel = vel;
        RCalf.LinVel = vel;
        LUArm0.LinVel = vel;
        RUArm0.LinVel = vel;
        LFArm.LinVel = vel;
        RFArm.LinVel = vel;
    }
}
