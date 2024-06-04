using Godot;

public partial class APerson
{
    public enum EGMAction
    {
        RELAX,
        STAND,
        RUN_COMMON, // common action of running, does nothing just to switch to other runs
        RUN_GROUND,
        RUN_FLAC,

        JUMP,
        JUMP_OFF_GROUND,
        JUMP_OFF_BAR,

        JUMP_GROUP,
        JUMP_ON_WALL_VERT,
        JUMP_ON_WALL_HORIZ,

        WALL_RUN_VERT,
        WALL_RUN_HORIZ,
        WALL_SLIDE,

        KICK_PUNCH,

        KNOCK_DOWN,
        KNOCK_OUT,

        GROUNDING_ROLL,

        GRAB_TRY,
        GRABBED_RELAX,
        GRABBED_SWING_GROUP,
        GRABBED_SWING_STRAINGTH,

        CONTROLLED_FALL,

        B_ACTION,

        ANIM_EDIT,
    }

    /// <summary>
    /// FSM for basic Actions
    /// </summary>
    protected readonly FSM<EGMAction> _FSM = new FSM<EGMAction>();

    void InitFSM()
    {
        SidesSwap(); // fill in the initial values
        // ------------------------------------------------
        // ACTIONS // ------------------------------------------------
        // расслабиться
        var a = _FSM.AddAction(EGMAction.RELAX);
        a.OnStart = () =>
        {
            Grab_ReleaseGrab();
            Relax();
            BonesGravityDefault();
            GravitySetInAir();
        };

        a = _FSM.AddAction(EGMAction.ANIM_EDIT);
        a.OnStart = () =>
        {
            GravitySetRun();
        };

        // STAND
        a = _FSM.AddAction(EGMAction.STAND);
        a.OnStart = StandUp;
        a.CheckFinish = StandCheck;
        a.OnJumpOff = BonesGravityDefault;

        // RUN
        a = _FSM.AddAction(EGMAction.RUN_COMMON);
        a.OnStart = () => _airControlDirection = Vector3.Zero;

        // on GROUND
        a = _FSM.AddAction(EGMAction.RUN_GROUND);
        a.OnStart = RunStart;
        a.CheckFinish = RunCheckFinish;
        a.OnJumpOff = () =>
        {
            Relax();
            BonesGravityDefault();
        };

        // FLAC
        a = _FSM.AddAction(EGMAction.RUN_FLAC);
        a.OnStart = FlacStart;
        a.CheckFinish = FlacCheckFinish;
        a.OnJumpOff = () =>
        {
            Relax();
            BonesGravityDefault();
        };

        // JUMP
        a = _FSM.AddAction(EGMAction.JUMP);
        a.CheckFinish = CheckJumpFinish;

        // JUMP OFF GROUND
        a = _FSM.AddAction(EGMAction.JUMP_OFF_GROUND);
        a.OnStart = () =>
        {
            // jump in the direction of the wall Normal
            if (IsGroundForSlide)
            {
                _airControlDirection = GroundContactDirection;
                if (_FSM.PrevActionKey == EGMAction.WALL_RUN_HORIZ)
                {
                    _airControlDirection += RunDirection3;
                }
                else if (_FSM.PrevActionKey == EGMAction.WALL_SLIDE)
                {
                    _airControlDirection += _FSMActionVector;
                }
                _airControlDirection = _airControlDirection.InPlane(UpDirection).Normalized();
                LastControlDirection = _airControlDirection;
                SumControlDirection = _airControlDirection;
            }
            else
                _airControlDirection = SumControlDirection;

            StartJump(JUMP_UP_VEL);
        };
        a.CheckFinish = Xts.AlwaysTrue;

        // JUMP OFF BAR
        a = _FSM.AddAction(EGMAction.JUMP_OFF_BAR);
        a.OnStart = () =>
        {
            Grab_ReleaseGrab();

            float dotUp = Body.AxisWorldMain.Dot(UpDirection);
            if (dotUp > 0.0f)
            {
                _airControlDirection = Vector3.Zero;
                Relax();
            }
            else
            {
                if (dotUp < -Xts.SIN60)
                    _airControlDirection = Vector3.Zero;
                else
                {
                    _airControlDirection = Grab_BarDirection().Cross(UpDirection);
                    if (_airControlDirection.Dot(Body.AxisWorldMain) > 0.0f)
                        _airControlDirection = -_airControlDirection;
                }
                StartJump(JUMP_UP_VEL);
            }
        };
        a.CheckFinish = Xts.AlwaysTrue;

        // JUMP_ON_WALL
        a = _FSM.AddAction(EGMAction.JUMP_ON_WALL_VERT);
        a.OnStart = VertRunStart;
        a.CheckFinish = CheckFinishJumpOnWall;

        a = _FSM.AddAction(EGMAction.JUMP_ON_WALL_HORIZ);
        a.OnStart = HorizRunStart;
        a.CheckFinish = CheckFinishJumpOnWall;

        // JUMP GROUP
        a = _FSM.AddAction(EGMAction.JUMP_GROUP);
        a.OnStart = () =>
        {
            Group();
            _jumpVel = Body.AxisWorldSide.Cross(UpDirection).Cross(UpDirection).Normalized();
        };
        a.CheckFinish = () =>
        {
            if (!Control.IsJumpPressed || HasGroundContact)
                return true;

            _FSMActionVector = Body.AngVel;
            _FSMActionVector = _FSMActionVector.ProjectionOn(_jumpVel);
            _FSMActionVector = _FSMActionVector.Normalized();
            _FSMActionVector *= JUMP_GROUP_SPEED;
            Body.AngVel = _FSMActionVector;

            SetBodyVelInAir();
            return CheckStartJumpOnWall();
        };

        // WALL RUN VERTical
        a = _FSM.AddAction(EGMAction.WALL_RUN_VERT);
        a.CheckFinish = VertRunCheck;

        // WALL RUN HORIZontal
        a = _FSM.AddAction(EGMAction.WALL_RUN_HORIZ);
        a.CheckFinish = HorizRunCheck;
        a.OnJumpOff = () =>
        {
            _FSMActionVector = RunDirection3;
        };

        // WALL SLIDE down
        a = _FSM.AddAction(EGMAction.WALL_SLIDE);
        a.OnStart = () =>
        {
            if (_FSM.PrevActionKey != EGMAction.WALL_RUN_HORIZ)
                _FSMActionVector = Vector3.Zero;
            WallSlideStart();
            BonesGravity2Wall();
            _FSMActionVector2 = GroundContactDirection.Cross(UpDirection).Cross(GroundContactDirection).Normalized() * -3.0f;
            _FSMActionVector2 += GroundContactDirection * -2.0f;
            _FSMActionVector2 += _FSMActionVector * 3.0f;
            _FSMActionVector2 *= WALL_SLIDE_SPEED;
        };
        a.CheckFinish = () =>
        {
            if (NeedWallSlide() == false)
            {
                return true;
            }
            Body.LinVel = ControlDirection * WALL_SLIDE_SPEED + _FSMActionVector2;
            return false;
        };
        a.OnJumpOff = BonesGravityDefault;

        // B action
        a = _FSM.AddAction(EGMAction.B_ACTION);
        a.OnStart = () =>
        {
            if (ActiveTrigger != null)
            {
                ActiveTrigger.DoAction();

                var dir = (ActiveTrigger.GetGlobalPos() - Body.Position).Normalized();
                var tan = dir.Cross(UpDirection);

                // Person direct
                Body.StartDirect(ABone3.EAxis.FRONT, dir, 0.5f, ABone3.EAxis.MAIN, UpDirection, 1.0f);
                // NPC direct
                ActiveTrigger.GetParentOrNull<ABone3>()?.StartDirect(ABone3.EAxis.FRONT, -dir, 0.5f, ABone3.EAxis.MAIN, UpDirection, 1.0f);

                var cam = SceneManager.Camera;
                if (cam.CurrentCameraDirection.Dot(tan) < 0.0f)
                    tan = -tan;
                cam.LookAt((tan * 3f - UpDirection).Normalized());
            }
            SceneManager.UI.SetBAction(null);
            EnableControl(false);
        };
        a.CheckFinish = () =>
        {
            if (ActiveTrigger == null)
                return true;

            if (ActiveTrigger.IsActionFinished())
            {
                var t = ActiveTrigger;
                // Reset b action, after Finish
                SetBAction(t, false);
                if (t.Player == this)
                    SetBAction(t, true);
                return true;
            }

            StandCheck();

            return false;
        };
        a.OnJumpOff = () =>
        {
            EnableControl(true);
        };

        // KICK or PUNCH
        a = _FSM.AddAction(EGMAction.KICK_PUNCH);
        a.OnStart = () =>
        {
            if (HasGroundContact)
                KickPunchGroundStart();
            else
                KickPunchAirStart();
        };
        a.CheckFinish = () =>
        {
            if (KickPunchCheckFinish())
                return true;
            if (!HasGroundContact)
                BodyDirCheck4Limb();
            return false;
        };
        a.OnJumpOff = KickOnJumpOff;

        // KNOCK DOWN
        a = _FSM.AddAction(EGMAction.KNOCK_DOWN);
        a.OnStart = () =>
        {
            _FSMActionRestTime = KNOCK_DOWN_TIME;
            Relax();
            BonesGravityDefault();
            Grab_ReleaseGrab();
        };
        a.CheckFinish = () =>
        {
            _FSMActionRestTime -= A.FixedStep;
            return _FSMActionRestTime < 0.0f;
        };

        // KNOCK OUT
        a = _FSM.AddAction(EGMAction.KNOCK_OUT);
        a.OnStart = () =>
        {
            Relax();
            BonesGravityDefault();
            Grab_ReleaseGrab();
        };
        a.CheckFinish = Xts.AlwaysFalse;

        // roll on landing
        a = _FSM.AddAction(EGMAction.GROUNDING_ROLL);
        a.OnStart = GroundingRollStart;
        a.CheckFinish = GroundingRollCheckFinish;
        a.OnJumpOff = () =>
        {
            BonesGravityDefault();
            GravitySetStand();
        };

        // grabbing the bar
        a = _FSM.AddAction(EGMAction.GRAB_TRY);
        a.OnStart = Grab_GrabTryStart;
        a.CheckFinish = Grab_GrabTryCheckFinish;
        a.OnJumpOff = Grab_GrabTryJumpOff;

        // hanging on a bar
        a = _FSM.AddAction(EGMAction.GRABBED_RELAX);
        a.OnStart = () =>
        {
            _FSMActionRestTime = 0.5f;
            Straight();
            RestoreFocus();
        };
        a.CheckFinish = () =>
        {
            // check regrab arm
            Grab_Regrab();

            if (_FSMActionRestTime > 0.0f)
            {
                _FSMActionRestTime -= A.FixedStep;
                if (_FSMActionRestTime <= 0.0f)
                {
                    Relax();
                }
            }

            return NeedJumpOffBar();
        };

        // swing on bar - bottom
        a = _FSM.AddAction(EGMAction.GRABBED_SWING_GROUP);
        a.OnStart = GrabbedSwingGroupStart;
        a.CheckFinish = GrabbedSwingGroup;

        // swing on bar - top
        a = _FSM.AddAction(EGMAction.GRABBED_SWING_STRAINGTH);
        a.OnStart = GrabbedSwingStraingthStart;
        a.CheckFinish = GrabbedSwingStraingth;

        // Controlled Fall
        a = _FSM.AddAction(EGMAction.CONTROLLED_FALL);
        a.OnStart = ControlledFallStart;
        a.CheckFinish = ControlledFallCheckFinish;

        // ------------------------------------------------
        // JUMPS // ------------------------------------------------
        InitFSMJumps();

        // ------------------------------------------------
        // DEFAULT // ------------------------------------------------
        _FSM.SetAsDefault(EGMAction.RELAX);
        _FSM.SetAsActive(EGMAction.RELAX);
    } // void InitFSM

    public bool NoMoreFocus(float rate = 1.0f)
    {
        Focus -= A.FixedStep * rate;
        if (Focus > 0.0f)
            return false;
        Focus = 0.0f;
        return true;
    }

    public void FSMActiveActionRestart() => _FSM.ActiveAction.Start();

    bool RunCheckFinish()
    {
        if (SumControlDirection == Vector3.Zero)
            return true;
        return RunCheck();
    }

    void SetBodyVelInAir()
    {
        if (SumControlDirection != Vector3.Zero)
        {
            Body.LinVel = (ControlDirection * USER_CTRL_RATE + _airControlDirection * AIR_CTRL_RATE) * MOVE_SPEED
                + Body.LinVel.ProjectionOn(UpDirection);
        }
    }
    void KickPunchStart()
    {
        var rival = GetRival();
        if (rival != null)  // has rival
            _kickDirection = rival.Body.Position - Body.Position;
        else // kick in air
            _kickDirection = LastControlDirection;
        _kickDirection = _kickDirection.Normalized();

        // the part that suits the Speed the most
        ActiveBone = GetKickBone(rival, ActiveBone, ref _kickDirection);

        // hit trail
        _kickTrail.Emitter = (Spatial)ActiveBone.LastChild();
        _FSMActionRestTime = KICK_MAX_TIME;

        // reset the fly direction
        _airControlDirection = Vector3.Zero;
    }
    void KickPunchGroundStart()
    {
        KickPunchStart();
        StartGroundKick(ActiveBone);
        BonesGravityZero();
    }
    void KickPunchAirStart()
    {
        KickPunchStart();
        StartAirKick(ActiveBone, _kickDirection);
        BonesGravityZero();
    }

    bool KickPunchCheckFinish()
    {
        _FSMActionRestTime -= A.FixedStep;
        if (_FSMActionRestTime < 0.0f)
            return true;

        if (NoMoreFocus(1.5f))
            return true;

        // set the Speed of the Bone, if there is still a Focus
        _FSMActionVector = _kickDirection * KICK_SPEED;
        ActiveBone.LinVel = _FSMActionVector;
        Body.LinVel = _FSMActionVector * -0.15f; // stop body

        return false;
    }

    void KickOnJumpOff()
    {
        ActiveBone = null;
        _kickTrail.Emitter = null;
        BonesGravityDefault();
    }

    bool CheckStartJumpOnWall()
    {
        if (Body.LinVel.Dot(UpDirection) > 0.0f)
        {
            if (CheckWall2FSMActionVector())
            { // is wall
                if (_airControlDirection.InPlane(UpDirection).Normalized().Dot(_FSMActionVector) < -Xts.SIN50)
                    DoAction(EGMAction.JUMP_ON_WALL_VERT);
                else
                    DoAction(EGMAction.JUMP_ON_WALL_HORIZ);

                _FSMActionRestTime2 = 0.5f;

                return true; // jump on wall
            }
        }
        return false;
    } // CheckStartJumpOnWall

    bool IntersectRay2FSMActionVector(in Vector3 direction)
    {
        return IntersectRay2FSMActionVector(Body.Position, direction);
    }

    bool IntersectRay2FSMActionVector(in Vector3 from, in Vector3 direction)
    {
        return this.IntersectRay(from, direction, Xts.GROUND_LAYER_VALUE, ref _FSMActionVector);
    }

    bool CheckWall2FSMActionVector()
    {
        var dir = Body.LinVel.InPlane(UpDirection).Normalized() * WALL_CHECK_LEN;
        if (IntersectRay2FSMActionVector(dir))
        {
            if (CheckForSlide(_FSMActionVector))
            { // is wall
                return true; // jump on wall
            }
        }
        return false;
    }

    bool CheckFinishJumpOnWall()
    {
        if (!Control.IsJumpPressed)
            return true;

        _FSMActionRestTime2 -= A.FixedStep;
        if (_FSMActionRestTime2 < 0.0f)
        {
            if (!CheckWall2FSMActionVector())
                return true;
            _FSMActionRestTime2 = 0.5f;
        }

        if (_FSMActionRestTime > 0.0f) // continue jump
        {
            _FSMActionRestTime -= A.FixedStep;
            Body.LinVel = _jumpVel;
        }
        else // fall
        {
            SetBodyVelInAir();
        }

        return false;
    } // CheckFinishJumpOnWall


    #region GRAB

    bool GrabbedSwingGroup()
    {
        _FSMActionVector = Grab_BarTangent();
        if (_FSMActionVector.Dot(Body.LinVel) < 0.0f)
            Xts.Reverse(ref _FSMActionVector);
        Body.LinVel = Xts.ApproachLinear(Body.LinVel
                                        , _FSMActionVector * GRAB_SWING_MAX_SPEED
                                        , GRAB_SWING_RATE * A.FixedStep
                                        );
        return Body.DirectDot(UpDirection) < 0.0f;
    }

    bool GrabbedSwingStraingth()
    {
        return Body.DirectDot(UpDirection) > Xts.SIN65;
    }

    #endregion

    #region MISC

    void StartJump(float upVelocity)
    {
        GravitySetInAir();
        Straight();
        Body.Relax();
        _FSMActionRestTime = JUMP_MAX_TIME;

        _jumpVel = _airControlDirection * MOVE_SPEED + UpDirection * upVelocity;
    }

    bool CheckJumpFinish()
    {
        if (!Control.IsJumpPressed)
            return true;

        _FSMActionRestTime -= A.FixedStep;
        if (_FSMActionRestTime < 0.0f)
            return true;

        Body.LinVel = _jumpVel;

        return CheckStartJumpOnWall();
    } // CheckJumpFinish

    public void DoAction(EGMAction action) => _FSM.SetAsActive(action);
    public void DoAction(EGMAction action, bool restart)
    {
        if (restart && _FSM.ActiveActionKey == action)
            return;
        DoAction(action);
    }

    public EGMAction ActiveAction => _FSM.ActiveActionKey;
    public string ActiveActionName => ActiveAction.ToString();
    public string PrevActionName => _FSM.PrevActionKey.ToString();

    #endregion
}
