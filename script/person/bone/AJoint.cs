using Godot;

public sealed class AJoint: HingeJoint
{
    public bool IsLimiting { get; private set; }

    /// <summary>
    /// the Target angle of the required rotation and fixation of the joint
    /// </summary>
    public float TargetAngle { get; private set; }

    float _defaultLowLimit;
    float _defaultHighLimit;
    float _limitSpeed;

    public override void _Ready()
    {
        _defaultLowLimit = AngularLimit__lower;
        _defaultHighLimit = AngularLimit__upper;
    }

    /// <summary>
    /// the angle Between the limits
    /// </summary>
    /// <param Name="range">from 0 to 1</param>
    public float GetAngle(float range)
    {
        return Xts.Lerp(_defaultLowLimit, _defaultHighLimit, range);
    }

    void UpdateLimitAngle(float angle)
    {
        StopLimit();
        AngularLimit__lower = angle - BIAS;
        AngularLimit__upper = angle + BIAS;
    }

    public void LimitNow(float range) => UpdateLimitAngle(GetAngle(range));

    public void LimitNow(float range_low, float range_high)
    {
        StopLimit();
        AngularLimit__lower = GetAngle(range_low) - BIAS;
        AngularLimit__upper = GetAngle(range_high) + BIAS;
    }

    /// <summary>
    /// restoring the default limits
    /// </summary>
    public void Limit2Default()
    {
        StopLimit();
        AngularLimit__lower = _defaultLowLimit;
        AngularLimit__upper = _defaultHighLimit;
    }

    /// <summary>
    /// To begin limiting the position of the Bone, the Bone retains one angle with the parent
    /// </summary>
    /// <param Name="range">from 0 to 1 (0 - low, 1 - high)</param>
    /// <param Name="Speed">Speed in degrees per second</param>
    public void StartLimit(float range, float speed)
    {
        Limit2Default();
        TargetAngle = GetAngle(range);
        IsLimiting = true;
        _limitSpeed = speed;
    }
    void StopLimit() => IsLimiting &= false;

    const float BIAS = 1.5f;

    public void FixedUpdate(float delta)
    {
        if (IsLimiting)
        {
            float h = delta * _limitSpeed;
            float l = AngularLimit__lower + h;
            h = AngularLimit__upper - h;

            if (l > TargetAngle - BIAS)
            {
                if (h < TargetAngle + BIAS)
                    UpdateLimitAngle(TargetAngle);
                else
                {
                    AngularLimit__lower = TargetAngle - BIAS;
                    AngularLimit__upper = h;
                }
            }
            else
            {
                AngularLimit__lower = l;
                AngularLimit__upper = h < TargetAngle + BIAS ? TargetAngle + BIAS : h;
            }
        }
    } // void FixedUpdate
}
