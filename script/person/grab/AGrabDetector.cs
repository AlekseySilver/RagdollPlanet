using Godot;
using System.Collections.Generic;
using System.Linq;

public class AGrabDetector: Area
{
    Vector3 _direction;
    float _halfLength;
    float _slideDelayRest = 0.0f;

    AGrabJoint _Lgrab;
    AGrabJoint _Rgrab;
    Vector3 _Lgrab_point
    {
        get => _Lgrab.TargetGrabPoint;
        set => _Lgrab.TargetGrabPoint = value;
    }
    Vector3 _Rgrab_point
    {
        get => _Rgrab.TargetGrabPoint;
        set => _Rgrab.TargetGrabPoint = value;
    }

    bool _Lfirst;
    readonly HashSet<PhysicsBody> _bars = new HashSet<PhysicsBody>();

    APerson _behavior;

    /// <summary>
    /// Grab Target body
    /// </summary>
    public PhysicsBody BarRigidBody { get; private set; } = null;

    /// <summary>
    /// Grab Target bar Direction
    /// </summary>
    public Vector3 Direction { get => _direction; }

    float _shoulderLength = 0.7f;

    public void InitBehavior(APerson behaviorHuman)
    {
        _behavior = behaviorHuman;
        _shoulderLength = Xts.Max((_behavior.LUArm0.Position - _behavior.RUArm0.Position).Length(), .7f);

        _Lgrab = _behavior.LFArm.FirstChild<AGrabJoint>();
        _Rgrab = _behavior.RFArm.FirstChild<AGrabJoint>();
        _Lgrab.Init();
        _Rgrab.Init();
    }

    void body_entered(Node e)
    {
        var rb = (PhysicsBody)e;
        _bars.Add(rb);
        if (BarRigidBody == null)
            InitNewTarget(rb);
    }
    void body_exited(Node e)
    {
        var rb = (PhysicsBody)e;
        _bars.Remove(rb);
        if (BarRigidBody == rb)
        {
            if (_bars.Count > 0)
                InitNewTarget(_bars.First());
            else
                BarRigidBody = null;
        }
    }

    void InitNewTarget(PhysicsBody barRigidBody)
    {
        var cs = barRigidBody.FirstChild<CollisionShape>();
        if (cs != null && cs.Shape is CapsuleShape capsule)
        {
            _halfLength = capsule.Height * 0.5f * barRigidBody.Scale.z;
            _direction = barRigidBody.DirLocal2World(Vector3.Forward).Normalized();
            BarRigidBody = barRigidBody;
            UpdateGrabPoints();
        }
    }

    void UpdateGrabPoints()
    {
        var barPos = BarRigidBody.GlobalTransform.origin;

        // delta
        var d = _behavior.Head.Position - barPos;

        float minMax = _halfLength - _shoulderLength;
        float o = Xts.Between(d.Dot(_direction), -minMax, minMax);

        // on bar Center Grab point
        if (_direction.Dot(_Lgrab.JointPointWorld) < _direction.Dot(_Rgrab.JointPointWorld))
        {
            _Lgrab_point = _direction * (o - _shoulderLength) + barPos;
            _Rgrab_point = _direction * (o + _shoulderLength) + barPos;
        }
        else
        {
            _Lgrab_point = _direction * (o + _shoulderLength) + barPos;
            _Rgrab_point = _direction * (o - _shoulderLength) + barPos;
        }

        _Lfirst = (_Lgrab.JointPointWorld - _Lgrab_point).LengthSquared() < (_Rgrab.JointPointWorld - _Rgrab_point).LengthSquared();

        if (!(_Lgrab.IsGrabbed && _Rgrab.IsGrabbed))
            _slideDelayRest = 0.0f;
    } // UpdateGrabPoints

    /// <summary>
    /// grabbing
    /// true - when both hands grabbed
    /// </summary>
    public bool UpdateGrab(float timeStep)
    {
        if (!(_Lgrab.IsGrabbed || _Rgrab.IsGrabbed))
            UpdateGrabPoints(); // until the hands are grasped, shift the grip points

        if (_Lfirst)
            return UpdateGrab(_Lgrab) && SlideDelay(timeStep) && UpdateGrab(_Rgrab);
        return UpdateGrab(_Rgrab) && SlideDelay(timeStep) && UpdateGrab(_Lgrab);
    }

    bool SlideDelay(float timeStep)
    {
        if (_slideDelayRest > 0.0f)
        {
            _slideDelayRest -= timeStep;
            return false;
        }
        return true;
    }

    bool UpdateGrab(AGrabJoint joint)
    {
        if (joint.IsGrabbed)
            return true;

        joint.Move2Target();
        if (BarRigidBody != null
            &&
                (joint.JointPointWorld - joint.TargetGrabPoint).LengthSquared()
                <
                joint.Bone.Person.GRAB_DIST_SQ
        )
        {
            joint.Grab(BarRigidBody);
            return true;
        }
        return false;
    }

    /// <summary>
    /// move hand
    /// </summary>
    public void SetNewGrabPoint()
    {
        if (_behavior.ControlDirection == Vector3.Zero || BarRigidBody == null)
            return;

        float dir;
        if (Xts.IsBetween(_behavior.UpDirection.Dot(_direction), -Xts.SIN45, Xts.SIN45))
        {
            dir = _behavior.ControlDirection.Dot(_direction);
            if (Xts.IsBetween(dir, -Xts.SIN30, Xts.SIN30))
                return;
        }
        else
        {
            // the direction of the crossbar in screen coordinates
            dir = _behavior.ControlScreenDirection.y;
            if (Xts.IsBetween(dir, -Xts.SIN45, Xts.SIN45))
                return;
            dir *= _direction.y;
        }
        dir = dir > 0.0f ? 1.0f : -1.0f;

        // which arm to move
        AGrabJoint arm2move;
        var newp = _Rgrab.JointPointWorld - _Lgrab.JointPointWorld;
        if (newp.Dot(_direction) * dir > 0.0f)
        {
            arm2move = _Lgrab;
            newp = _Rgrab_point;
        }
        else
        {
            arm2move = _Rgrab;
            newp = _Lgrab_point;
        }

        // new Grab point
        newp += _direction * (dir * 2.0f * _shoulderLength);

        // check border
        var d = newp - BarRigidBody.GlobalTransform.origin; // vector from the Center of the horizontal bar to the new capture point
        if (d.Dot(_direction) * dir > _halfLength)
        {
            return; // don't move
        }

        _Lfirst = arm2move != _Lgrab;
        arm2move.TargetGrabPoint = newp;

        // release one arm
        arm2move.ReleaseGrab();

        _slideDelayRest = 0.5f;
    } // SetNewGrabPoint

    public bool IsOneHandGrabbed => _Lgrab.IsGrabbed != _Rgrab.IsGrabbed;

    public bool CanGrab => BarRigidBody != null;

    public bool IsGrabbed => _Lgrab.IsGrabbed || _Rgrab.IsGrabbed;
    public bool IsGrabbedTotal => _Lgrab.IsGrabbed && _Rgrab.IsGrabbed;

    public PhysicsBody GrabbedBody => _Lgrab.GrabbedBody ?? _Rgrab.GrabbedBody;

    public void ReleaseGrab()
    {
        _Lgrab.ReleaseGrab();
        _Rgrab.ReleaseGrab();
    }

    public Vector3 GetBarTangent()
    {
        return Direction.Cross(_Lgrab.JointPointWorld - _behavior.Body.Position).Normalized();
    }

    public Vector3 GetGrabPoint()
    {
        if (_Lgrab.IsGrabbed)
            return _Lgrab.JointPointWorld;
        return _Rgrab.JointPointWorld;
    }
} // AGrabDetector
