using Godot;
using System.Runtime.CompilerServices;

public abstract partial class APerson
{
    const float MAX_RUN_STEP_TIME = 0.25f;
    const float HALF_MAX_RUN_STEP_TIME = MAX_RUN_STEP_TIME * 0.5f;
    const float HOVER_HELP_RATE_MIN = 0.9f;
    const float HOVER_HELP_RATE_MAX = 1.2f;

    const float MAX_FLAC_SUB_STEP = 0.4f;

    protected float _runStepTime;
    int _subStep = 0;

    protected AAnimation _currentAnim;
    protected AAnimation _runAnim;
    protected AAnimation _walkAnim;
    protected AAnimation _animKickAirL;
    protected AAnimation _animKickAirR;
    protected AAnimation _animKickGroundL;
    protected AAnimation _animKickGroundR;
    protected AAnimation _animPunchAirL;
    protected AAnimation _animPunchAirR;
    protected AAnimation _animPunchGroundL;
    protected AAnimation _animPunchGroundR;

    protected virtual ABone1 GetKickBone(APerson rival, ABone1 activeBone, ref Vector3 kickDirection)
    {
        if (GD.Randf() > 0.5f)
        {
            if (activeBone == LFArm)
                activeBone = RFArm;
            else if (activeBone == RFArm)
                activeBone = LFArm;
            else
                activeBone = GD.Randf() > 0.5f ? LFArm : RFArm;
        }
        else
        {
            if (activeBone == LCalf)
                activeBone = RCalf;
            else if (activeBone == RCalf)
                activeBone = LCalf;
            else
                activeBone = GD.Randf() > 0.5f ? LCalf : RCalf;

            // hi kick
            if (rival == null && IsOnFloor)
                kickDirection = (kickDirection * 4.0f + UpDirection).Normalized();
        }

        // to clarify the kick
        if (rival != null)
            kickDirection = (rival.GetWeakPoint().GlobalTransform.origin - activeBone.Position).Normalized();

        return activeBone;
    } // GetKickBone

    void StartGroundKick(ABone1 kickBone)
    {
        Body.Relax();
        if (kickBone == LFArm)
            _animPunchGroundL.SingleFrame();
        else if (kickBone == RFArm)
            _animPunchGroundR.SingleFrame();
        else if (kickBone == LCalf)
            _animKickGroundL.SingleFrame();
        else if (kickBone == RCalf)
            _animKickGroundR.SingleFrame();
    }
    void StartAirKick(ABone1 kickBone, in Vector3 kickDirection)
    {
        if (kickBone == LFArm)
        {
            _animPunchAirL.SingleFrame();
            Body.StartDirect(kickDirection);
        }
        else if (kickBone == RFArm)
        {
            _animPunchAirR.SingleFrame();
            Body.StartDirect(kickDirection);
        }
        else if (kickBone == LCalf)
        {
            _animKickAirL.SingleFrame();
            Body.StartDirect(-kickDirection);
        }
        else if (kickBone == RCalf)
        {
            _animKickAirR.SingleFrame();
            Body.StartDirect(-kickDirection);
        }
    }

    #region STAND
    protected virtual void StandUp()
    {
        GravitySetStand();
        BonesGravity2Floor();
        Head.LimitNow(0.5f); // plain

        var dir = UpDirection;
        Body.StartDirect(dir, STAND_RATE);

        LThigh0.Relax();
        RThigh0.Relax();

        dir = -dir;

        LThigh1.StartDirect(dir, STAND_RATE);
        RThigh1.StartDirect(dir, STAND_RATE);

        LCalf.StartDirect(dir, STAND_RATE);
        RCalf.StartDirect(dir, STAND_RATE);


        LUArm0.LimitNow(0.0f, 0.4f);
        RUArm0.LimitNow(0.0f, 0.4f);

        LUArm1.Relax();
        RUArm1.Relax();

        LFArm.Relax();
        RFArm.Relax();

        _runStepTime = 0.5f;
    } // StandUp

    protected virtual bool StandCheck()
    {
        _runStepTime -= A.FixedStep;
        if (_runStepTime < 0.0f)
        {
            _runStepTime = 1.0f;
            var dir = -(BThigh1.AxisWorldMain + BCalf.AxisWorldMain).Normalized().Reflection(UpDirection);

            SidesSwap();

            AThigh1.StartDirect(dir, STAND_RATE);
            if (dir.Dot(UpDirection) > -Xts.SIN45)
                ACalf.Relax();
            else
                ACalf.StartDirect(-UpDirection, STAND_RATE);

            dir = -UpDirection;
            BThigh1.StartDirect(dir, STAND_RATE);
            BCalf.StartDirect(dir, STAND_RATE);
        }

        Body.LinVel = Xts.Lerp(Body.LinVel , Vector3.Zero, 0.5f);

        return IsOnFloor == false;
    } // StandCheck

    #endregion

    #region RUN
    protected virtual void CalcBodyDirection()
    {
        _runStepTime = 0.1f;
        RecalcRunDirection();

        float angle;
        var groundDirection = GroundContactDirection;

        float speed = Body.LinVel.Length();
        if (speed > WALK_RUN_STEP)
        {
            _runAnim.Overdrive = speed * RUN_OVERDRIVE_RATE;
            _currentAnim = _runAnim;

            // viewing what is the slope of the wall in front
            var dir = (RunDirection3 - GroundContactDirection) * WALL_CHECK_LEN;
            if (IntersectRay2FSMActionVector(Head.Position, dir))
            {
                if (_FSMActionVector.Dot(groundDirection) < Xts.SIN85)
                {
                    groundDirection = _FSMActionVector;
                    RecalcRunDirection(groundDirection);
                }
            }
            angle = Xts.LerpClamped(90.0f, 60.0f, (speed - WALK_RUN_STEP) / (MOVE_SPEED - WALK_RUN_STEP));
        }
        else
        {
            _walkAnim.Overdrive = Xts.Min(speed * WALK_OVERDRIVE_RATE, WALK_OVERDRIVE_MAX);
            _currentAnim = _walkAnim;
            angle = 90.0f;
        }

        angle *= Xts.deg2rad;
        var sin = Mathf.Sin(angle);
        var cos = Mathf.Cos(angle);

        var bodyMain = groundDirection * sin + RunDirection3 * cos;
        var bodyFront = RunDirection3 * sin - groundDirection * cos;

        const float force = 1.0f;
        Body.StartDirect(ABone3.EAxis.MAIN, bodyMain, force, ABone3.EAxis.FRONT, bodyFront, force);
    } // CalcBodyDirection

    public void RunStart()
    {
        GravitySetRun();
        BonesGravity2Floor();

        CalcBodyDirection();

        Head.StartLimit(0.5f); // ровно
        //_walkAnim.Reset();
        //_runAnim.Reset();
    } // RunStart

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual bool CheckBodyDirectionCantRun()
    {
        return Body.AxisWorldMain.Dot(GroundContactDirection) < Xts.SIN15;
    }

    public bool RunCheck()
    {
        if (IsOnFloor == false
            || CheckBodyDirectionCantRun()
            )
            return true;

        _currentAnim.Update(A.FixedStep);

        _runStepTime -= A.FixedStep;
        if (_runStepTime < 0.0f)
        {
            CalcBodyDirection();
        }

        SetBodyLinVelOnFloor();
        return false;
    } // RunCheck

    #endregion

    bool CheckWallRunFinished()
    {
        if (IsGroundForSlide)
        {
            if (IsOnWall == false)
                return true;

            if (ControlDirection != Vector3.Zero && ControlDirection.Dot(GroundContactDirection) > 0.0f)
                return true;
        }
        else
            return true;
        return false;
    } // CheckWallRunFinished

    #region VERT_RUN

    public void VertRunStart()
    {
        RecalcRunDirection(UpDirection, _FSMActionVector); // vertical running

        Relax();
        Body.StartDirect(ABone3.EAxis.MAIN, _FSMActionVector, 1.5f
                        , ABone3.EAxis.SIDE, _runSide3, 1.5f);

        Head.StartLimit(0.5f); // plain

        _runAnim.Overdrive = VERT_WALL_RUN_OVERDRIVE;
        _runAnim.GotoTimePart(Xts.RndNextBool() ? 0.0f : 0.5f);
        _airTime = 0.0f;
    } // VertRunStart

    public bool VertRunCheck()
    {
        if (CheckWallRunFinished())
            return true;

        _runAnim.Update(A.FixedStep);

        Body.LinVel = (UpDirection - GroundContactDirection * 0.1f) * VERT_WALL_MOVE_SPEED;
        return NoMoreFocus();
    } // VertRunCheck

    #endregion

    #region HORIZ_RUN

    public void HorizRunStart()
    {
        RecalcRunDirection(_airControlDirection, _FSMActionVector);

        Relax();
        Body.StartDirect(ABone3.EAxis.FRONT, RunDirection3, 1.5f
                        , ABone3.EAxis.MAIN, _FSMActionVector, 1.5f);
        Head.StartLimit(0.5f); // plain

        _runAnim.Overdrive = MOVE_SPEED * RUN_OVERDRIVE_RATE;
        _runAnim.GotoTimePart(Xts.RndNextBool() ? 0.0f : 0.5f);
        _airTime = 0.0f;
    } // HorizRunStart

    public bool HorizRunCheck()
    {
        if (CheckWallRunFinished())
            return true;

        if (GroundContactDirection.Dot(RunDirection3) > Xts.SIN45) // выпуклая стенка
            return true;

        _runAnim.Update(A.FixedStep);

        Body.LinVel = (RunDirection3 - GroundContactDirection * 0.1f + UpDirection * HORIZ_WALL_MOVE_RATE) * MOVE_SPEED;
        return NoMoreFocus();
    } // HorizRunCheck

    #endregion

    #region FLAC

    protected virtual void FlacStart()
    {
        GravitySetRun();
        BonesGravity2Floor();

        RecalcRunDirection();

        _subStep = 0;
        _runStepTime = float.MaxValue;

        Head.StartLimit(0.5f);
    } // FlacStart
    protected virtual bool FlacCheckFinish()
    {
        _runStepTime -= A.FixedStep;
        _FSMActionVector = GroundContactDirection;

        switch (_subStep)
        {
            case -1: // squat
                if (_runStepTime > 1000.0f)
                {
                    _runStepTime = 0.0f;

                    Straight();
                    Body.StartDirect(ABone3.EAxis.MAIN, _FSMActionVector, 0.5f, ABone3.EAxis.FRONT, -RunDirection3, 0.5f);
                }

                if (RunDirection3.Dot(LastControlDirection) < Xts.SIN45)
                    return true; // stop flac if a different Direction of movement is selected
                RecalcRunDirection();

                break;
            case 0: // Face up
                if (_runStepTime > 1000.0f)
                {
                    GravitySetInAir();
                    _runStepTime = 0.3f;

                    LThigh0.StartLimit(1.0f);
                    RThigh0.StartLimit(1.0f);

                    LThigh1.StartLimit(0.0f);
                    RThigh1.StartLimit(0.0f);

                    LCalf.StartLimit(0.5f);
                    RCalf.StartLimit(0.5f);

                    LUArm0.StartLimit(1.0f);
                    RUArm0.StartLimit(1.0f);

                    LUArm1.StartDirectByParentSpace(Vector3.Up);
                    RUArm1.StartDirectByParentSpace(Vector3.Up);

                    LFArm.StartLimit(0.5f);
                    RFArm.StartLimit(0.5f);

                    Body.StartDirect(RunDirection3);
                }
                else if (Body.AxisWorldMain.Dot(RunDirection3) > Xts.SIN75)
                { // Head in the Direction of running
                    _runStepTime = -1.0f; // Start the next step
                }
                break;
            case 1: // On hands
                if (_runStepTime > 1000.0f)
                {
                    _runStepTime = MAX_FLAC_SUB_STEP;

                    LFArm.StartLimit(0.0f);
                    RFArm.StartLimit(0.0f);

                    LThigh0.Relax();
                    RThigh0.Relax();
                    LThigh1.StartDirect(_FSMActionVector);
                    RThigh1.StartDirect(_FSMActionVector);
                    LCalf.Relax();
                    RCalf.Relax();

                    _FSMActionVector = -_FSMActionVector;

                    Body.StartDirect(ABone3.EAxis.MAIN, _FSMActionVector, 0.5f, ABone3.EAxis.FRONT, RunDirection3, 0.5f);
                }
                break;
            case 2: // Face down
                if (_runStepTime > 1000.0f)
                {
                    GravitySetRun();
                    _runStepTime = 0.2f;

                    Body.StartDirect(ABone3.EAxis.FRONT, _FSMActionVector, 0.5f, ABone3.EAxis.MAIN, -RunDirection3, 0.5f);

                    LThigh0.Relax();
                    RThigh0.Relax();
                    LThigh1.Relax();
                    RThigh1.Relax();
                    LCalf.Relax();
                    RCalf.Relax();
                }
                break;
            case 3: // On feet
                if (_runStepTime > 1000.0f)
                {
                    _runStepTime = 0.5f;

                    Body.StartDirect(ABone3.EAxis.MAIN, _FSMActionVector, 0.5f, ABone3.EAxis.FRONT, RunDirection3, 0.5f);

                    _FSMActionVector = -_FSMActionVector;
                    // coming on one leg, on the one below
                    if (LCalf.Position.Dot(UpDirection) < RCalf.Position.Dot(UpDirection))
                        LThigh1.StartDirect(_FSMActionVector);
                    else
                        RThigh1.StartDirect(_FSMActionVector);
                }
                if (RunDirection3.Dot(LastControlDirection) < Xts.SIN45)
                    return true; // stop flac if a different Direction of movement is selected
                RecalcRunDirection();
                break;
            default:
                return true;
        } // switch

        if (_runStepTime < 0.0f)
        {
            ++_subStep;
            _runStepTime = float.MaxValue;
        }

        SetBodyLinVelOnFloor(MOVE_SPEED);

        return false;
    } // FlacCheckFinish

    #endregion

    protected virtual bool GroundingRollCheckFinish()
    {
        var up = _floorDirection;
        var bodyMain = Body.AxisWorldMain;
        var bodyFront = Body.AxisWorldMain;

        if (bodyMain.Dot(up) > Xts.SIN45)
        {
            if (
                LThigh1.AxisWorldMain.Dot(up) < -Xts.SIN65
                || RThigh1.AxisWorldMain.Dot(up) < -Xts.SIN65
                || LThigh1.MainAxis.Dot(LCalf.MainAxis) < 0.0f // bended
                || RThigh1.AxisWorldMain.Dot(LCalf.MainAxis) < 0.0f
            )
            {
                return true;
            }
        }

        if (Body.IsDirecting == false)
        {
            var v = bodyMain.Cross(up);
            if (v.Dot(Body.AngVel) >= 0.0f)
            {
                Body.StartDirect(ABone3.EAxis.MAIN, up * 2.0f + RunDirection3);
            }
            else
            {
                Body.AngVel = Body.AngVel.Normalized() * GROUD_ROLL_SPEED;
            }
        }
        else if (_FSMActionRestTime > 0.75f)
            return true;

        _FSMActionRestTime += A.FixedStep;
        if (_FSMActionRestTime < 0.5f)
            SetBodyLinVel(SumControlLength * MOVE_SPEED * 0.5f);

        return false;
    } // GroundingRollCheckFinish
}
