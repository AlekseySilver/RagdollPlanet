using Godot;
using System.Runtime.CompilerServices;

public partial class APerson
{
    void InitFSMJumps()
    {
        // ------------------------------------------------
        // JUMPS // ------------------------------------------------
        // STAND

        _FSM.AddJumpReason(EGMAction.RELAX, EGMAction.GROUNDING_ROLL, NeedStandUp);
        _FSM.AddJumpReason(EGMAction.GROUNDING_ROLL, EGMAction.STAND, _FSM.IsActiveActionFinished);

        // RUN
        _FSM.AddJumpReason(EGMAction.STAND, EGMAction.RUN_GROUND, NeedStartRunCommon);
        _FSM.AddJumpReason(EGMAction.JUMP, EGMAction.RUN_FLAC, NeedStartFlac);

        // RELAX
        _FSM.AddJumpReason(EGMAction.RUN_GROUND, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.RUN_FLAC, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.JUMP, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.JUMP_GROUP, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.JUMP_ON_WALL_VERT, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.JUMP_ON_WALL_HORIZ, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.KNOCK_DOWN, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.KICK_PUNCH, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.WALL_SLIDE, EGMAction.RELAX, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.CONTROLLED_FALL, EGMAction.RELAX, _FSM.IsActiveActionFinished);

        // JUMP
        _FSM.AddJumpReason(EGMAction.JUMP_OFF_GROUND, EGMAction.JUMP, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.JUMP_OFF_BAR, EGMAction.JUMP, _FSM.IsActiveActionFinished);

        _FSM.AddJumpReason(EGMAction.RUN_GROUND, EGMAction.JUMP_OFF_GROUND, NeedJump);
        _FSM.AddJumpReason(EGMAction.STAND, EGMAction.JUMP_OFF_GROUND, NeedJump);
        _FSM.AddJumpReason(EGMAction.GROUNDING_ROLL, EGMAction.JUMP_OFF_GROUND, NeedJump);
        _FSM.AddJumpReason(EGMAction.WALL_SLIDE, EGMAction.JUMP_OFF_GROUND, NeedJumpOffWall);
        _FSM.AddJumpReason(EGMAction.WALL_RUN_VERT, EGMAction.JUMP_OFF_GROUND, NeedJumpOffWall);
        _FSM.AddJumpReason(EGMAction.WALL_RUN_HORIZ, EGMAction.JUMP_OFF_GROUND, NeedJumpOffWall);
        _FSM.AddJumpReason(EGMAction.RELAX, EGMAction.JUMP_GROUP, NeedJumpGroup);

        // B action
        _FSM.AddJumpReason(EGMAction.STAND, EGMAction.B_ACTION, NeedBAction);
        _FSM.AddJumpReason(EGMAction.B_ACTION, EGMAction.STAND, _FSM.IsActiveActionFinished);

        // KICK
        _FSM.AddJumpReason(EGMAction.STAND, EGMAction.KICK_PUNCH, NeedKickPunchStand);
        _FSM.AddJumpReason(EGMAction.RUN_GROUND, EGMAction.KICK_PUNCH, NeedKickPunch);
        _FSM.AddJumpReason(EGMAction.RUN_FLAC, EGMAction.KICK_PUNCH, NeedKickPunch);
        _FSM.AddJumpReason(EGMAction.RELAX, EGMAction.KICK_PUNCH, NeedKickPunch);
        _FSM.AddJumpReason(EGMAction.WALL_RUN_HORIZ, EGMAction.KICK_PUNCH, NeedKickPunch);
        _FSM.AddJumpReason(EGMAction.GROUNDING_ROLL, EGMAction.KICK_PUNCH, NeedKickPunch);

        // WALL
        _FSM.AddJumpReason(EGMAction.JUMP_ON_WALL_VERT, EGMAction.WALL_RUN_VERT, NeedWallRunStart);
        _FSM.AddJumpReason(EGMAction.JUMP_ON_WALL_HORIZ, EGMAction.WALL_RUN_HORIZ, NeedWallRunStart);

        // WALL_SLIDE
        _FSM.AddJumpReason(EGMAction.WALL_RUN_VERT, EGMAction.WALL_SLIDE, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.WALL_RUN_HORIZ, EGMAction.WALL_SLIDE, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.RELAX, EGMAction.WALL_SLIDE, NeedWallSlide);
        _FSM.AddJumpReason(EGMAction.CONTROLLED_FALL, EGMAction.WALL_SLIDE, NeedWallSlide);

        // GRAB
        _FSM.AddJumpReason(EGMAction.RELAX, EGMAction.GRAB_TRY, NeedTryGrab);
        _FSM.AddJumpReason(EGMAction.CONTROLLED_FALL, EGMAction.GRAB_TRY, NeedTryGrab);

        _FSM.AddJumpReason(EGMAction.GRAB_TRY, EGMAction.GRABBED_RELAX, _FSM.IsActiveActionFinished);

        _FSM.AddJumpReason(EGMAction.GRAB_TRY, EGMAction.RELAX, CanNotGrab);

        _FSM.AddJumpReason(EGMAction.GRABBED_SWING_GROUP, EGMAction.GRABBED_RELAX, NeedStopKickPunch);
        _FSM.AddJumpReason(EGMAction.GRABBED_SWING_STRAINGTH, EGMAction.GRABBED_RELAX, NeedStopKickPunch);

        _FSM.AddJumpReason(EGMAction.GRABBED_RELAX, EGMAction.GRAB_TRY, Grab_NeedGrabTry);
        _FSM.AddJumpReason(EGMAction.GRABBED_RELAX, EGMAction.GRABBED_SWING_STRAINGTH, NeedKickPunch);

        // GRAB SWING
        _FSM.AddJumpReason(EGMAction.GRABBED_SWING_GROUP, EGMAction.GRABBED_SWING_STRAINGTH, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EGMAction.GRABBED_SWING_STRAINGTH, EGMAction.GRABBED_SWING_GROUP, _FSM.IsActiveActionFinished);

        // GRAB JUMP
        _FSM.AddJumpReason(EGMAction.GRAB_TRY, EGMAction.JUMP_OFF_BAR, () => NeedJumpOffBar() && Grab_IsGrabbed);
        _FSM.AddJumpReason(EGMAction.GRABBED_RELAX, EGMAction.JUMP_OFF_BAR, NeedJumpOffBar);
        _FSM.AddJumpReason(EGMAction.GRABBED_SWING_GROUP, EGMAction.JUMP_OFF_BAR, NeedJumpOffBar);
        _FSM.AddJumpReason(EGMAction.GRABBED_SWING_STRAINGTH, EGMAction.JUMP_OFF_BAR, NeedJumpOffBar);

        // CONTROLLED_FLY
        _FSM.AddJumpReason(EGMAction.RELAX, EGMAction.CONTROLLED_FALL, NeedControlledFly);

        // ------------------------------------------------
        // DEFAULT // ------------------------------------------------
        _FSM.SetAsDefault(EGMAction.RELAX);
        _FSM.SetAsActive(EGMAction.RELAX);
    } // void InitFSM

    bool NeedStandUp() => IsOnFloor && UpDirection.Dot(GroundContactDirection) > Xts.SIN30;

    bool NeedStartRunCommon() => SumControlDirection != Vector3.Zero
        && (CheckGroundExact() || _airTime < 1.0f)
        ;
    bool NeedStartFlac() => !Control.IsJumpPressed && _airTime < 0.1f
        && ControlDirection != Vector3.Zero
        && UpDirection.Dot(GroundContactDirection) > Xts.SIN30
        ;

    bool NeedJump()
    {
        return NeedJumpOffBar() && CheckGroundExact();
    }
    bool NeedJumpGroup()
    {
        return Control.IsJumpPressed && IsInAir;
    }
    bool NeedJumpOffWall()
    {
        return NeedJumpOffBar();
    }

    bool IsJustJump => Control.JumpPressedTime > 0.0f && Control.JumpPressedTime < JUMP_MAX_TIME;

    bool NeedJumpOffBar()
    {
        return IsJustJump;
    }

    bool NeedKickPunchStand()
    {
        return NeedKickPunch() && ActiveTrigger == null;
    }
    bool NeedBAction()
    {
        return NeedKickPunch() && ActiveTrigger != null;
    }

    bool NeedKickPunch()
    {
        return Control.KickPressedTime > 0.0f && Control.KickPressedTime < KICK_MAX_TIME;
    }
    bool NeedStopKickPunch()
    {
        return !Control.IsKickPressed;
    }

    bool NeedWallSlide()
    {
        return CheckGroundExact()
         && IsGroundForSlide;
    }

    bool NeedWallRunStart()
    {
        return HasGroundContact && Focus > MAX_FOCUS_TIME * 0.99f && !IsJustJump
            && IsGroundForSlide
            ;
    }

    bool NeedTryGrab()
    {
        return Grab_CanGrab && !IsJump && !IsKick;
    }

    bool CanNotGrab() => !Grab_CanGrab || _FSMActionRestTime < 0.0f;

    bool NeedControlledFly() => IsInAir && !IsJump && !IsKick;

    bool IsGroundForSlide => CheckForSlide(GroundContactDirection);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    bool CheckForSlide(in Vector3 norm)
    {
        return Xts.IsBetween(UpDirection.Dot(norm), -0.1f, Xts.SIN45);
    }
}
