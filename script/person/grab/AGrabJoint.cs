using Godot;

public class AGrabJoint: Spatial
{
    public ABone Bone { get; private set; }

    public PhysicsBody GrabbedBody { get; private set; } = null;

    RID _pinJoint = null;

    public void Init()
    {
        Bone = GetParentOrNull<ABone>();
    }

    public void Grab(PhysicsBody body)
    {
        ReleaseGrab();
        GrabbedBody = body;

        PhysicsServer.BodyAddCollisionException(Bone.GetRid(), body.GetRid());

        Vector3 localA = Translation;
        Vector3 localB = body.GlobalTransform.AffineInverse().Xform(TargetGrabPoint);

        _pinJoint = PhysicsServer.JointCreatePin(Bone.GetRid(), localA, body.GetRid(), localB);
        PhysicsServer.JointSetSolverPriority(_pinJoint, 1000);
    }

    public void ReleaseGrab()
    {
        if (IsGrabbed)
        {
            PhysicsServer.BodyRemoveCollisionException(Bone.GetRid(), GrabbedBody.GetRid());
            PhysicsServer.FreeRid(_pinJoint);
            _pinJoint = null;
            GrabbedBody = null;
        }
    }

    public Vector3 TargetGrabPoint { get; set; }

    public void Move2Target()
    {
        Bone.LinVel = Xts.ApproachLinear(Bone.LinVel
            , (TargetGrabPoint - JointPointWorld).Normalized() * Bone.Person.GRAB_MOVE2POINT_VEL
            , A.FixedStep * Bone.Person.GRAB_MOVE2POINT_ACC);
    }

    public bool IsGrabbed => GrabbedBody != null;

    /// <summary>
    /// the current location of the grabbing joint
    /// </summary>
    public Vector3 JointPointWorld => GlobalTransform.origin;
}
