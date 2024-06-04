using Godot;

public sealed class APersonApe: APerson
{
    protected override ABone1 GetKickBone(APerson rival, ABone1 activeBone, ref Vector3 kickDir)
    {
        if (activeBone == LFArm)
            activeBone = RFArm;
        else if (activeBone == RFArm)
            activeBone = LFArm;
        else
            activeBone = GD.Randf() > 0.5f ? LFArm : RFArm;

        // clarify the kick
        if (rival != null)
            kickDir = (rival.GetWeakPoint().GlobalTransform.origin - activeBone.Position).Normalized();

        return activeBone;
    } // GetKickBone

    protected override bool CheckBodyDirectionCantRun()
    {
        return false;
    }

    protected override void StandUp()
    {
        GravitySetStand();
        BonesGravity2Floor();
        Head.LimitNow(0.5f); // plain

        var dir = UpDirection;
        Body.SetTargetOffset(new Basis(new Vector3(55f * Xts.deg2rad, 0.0f, 0.0f)));
        Body.StartDirect(dir, STAND_RATE);

        LThigh0.StartLimit(0.1f);
        RThigh0.StartLimit(0.1f);

        LThigh1.StartLimit(0.0f);
        RThigh1.StartLimit(0.0f);

        LCalf.StartLimit(0.5f);
        RCalf.StartLimit(0.5f);


        LUArm0.StartLimit(0.5f);
        RUArm0.StartLimit(0.5f);

        LUArm1.StartLimit(0.0f);
        RUArm1.StartLimit(0.0f);

        LFArm.StartLimit(0.1f);
        RFArm.StartLimit(0.1f);
    } // StandUp

    protected override bool StandCheck()
    {
        Body.LinVel = Xts.Lerp(Body.LinVel, Vector3.Zero, 0.5f);
        return IsOnFloor == false;
    }

    protected override void CalcBodyDirection()
    {
        _runStepTime = 0.1f;
        RecalcRunDirection();

        float speed = Body.LinVel.Length();
        if (speed > WALK_RUN_STEP)
        {
            _runAnim.Overdrive = speed * RUN_OVERDRIVE_RATE;
            _currentAnim = _runAnim;
        }
        else
        {
            _walkAnim.Overdrive = Xts.Min(speed * WALK_OVERDRIVE_RATE, WALK_OVERDRIVE_MAX);
            _currentAnim = _walkAnim;
        }

        const float force = 1f;
        Body.StartDirect(ABone3.EAxis.MAIN, GroundContactDirection, force, ABone3.EAxis.FRONT, RunDirection3, force);
    } // CalcBodyDirection

    protected override void FlacStart()
    {
    }
    protected override bool FlacCheckFinish()
    {
        return true;
    }

    protected override bool GroundingRollCheckFinish()
    {
        return true;
    }
} // APersonApe
