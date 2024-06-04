using Godot;

public abstract class APersonRigidBody: RigidBody
{
    public APerson Person = null;

    public override void _IntegrateForces(PhysicsDirectBodyState state)
    {
        if (Person == null)
            return;

        _getLinVel = state.LinearVelocity;
        _getAngVel = state.AngularVelocity;

        Custom(state);

        if (_needLinSet)
        {
            state.LinearVelocity = _setLinVel;
            _needLinSet = false;
        }
        if (_needAngSet)
        {
            state.AngularVelocity = _setAngVel;
            _needAngSet = false;
        }

        // Gravity
        state.AddCentralForce(Person.BonesCurrentGravityOverride); //  * Mass, it is assumed that the mass of every Bone = 1
    } // _IntegrateForces

    protected abstract void Custom(PhysicsDirectBodyState state);

    public Vector3 Position => GlobalTransform.origin;

    Vector3 _getLinVel;
    Vector3 _setLinVel;
    bool _needLinSet = false;
    Vector3 _getAngVel;
    Vector3 _setAngVel;
    bool _needAngSet = false;

    /// <summary>
    /// Linear velocity
    /// </summary>
    public Vector3 LinVel
    {
        get => _needLinSet ? _setLinVel : _getLinVel;
        set
        {
            _setLinVel = value;
            _needLinSet = true;
        }
    }

    /// <summary>
    /// Angular velocity
    /// </summary>
    public Vector3 AngVel
    {
        get => _needAngSet ? _setAngVel : _getAngVel;
        set
        {
            _setAngVel = value;
            _needAngSet = true;
        }
    }
}
