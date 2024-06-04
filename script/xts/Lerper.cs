using Godot;

/// <summary>
/// base for a linear interpolator
/// </summary>
public class ALerperBase
{
    public float Speed { get; set; } = 1.0f;

    /// <summary>
    /// Source (current position) from 0 to 1
    /// </summary>
    public float Source { get; set; } = 0.0f;

    /// <summary>
    /// Target from 0 to 1
    /// </summary>
    public float Target { get; set; } = 1.0f;

    public virtual bool Update(float time)
    {
        if (IsDone)
            return false; // target achieved

        if (Source < Target)
        {
            Source += Speed * time;
            if (Source > Target)
                Source = Target;
        }
        else
        {
            Source -= Speed * time;
            if (Source < Target)
                Source = Target;
        }

        return true; // continue
    } // UpdateOverride

    /// <summary>
    /// target achieved
    /// </summary>
    public bool IsDone
    {
        get => Source == Target;
        set
        {
            if (value)
                Source = Target;
        }
    }
} // ALerperBase


/// <summary>
/// Vector2 interpolator
/// </summary>
public class ALerperV2: ALerperBase
{
    public Vector2 A { get; set; } = Vector2.Zero;
    public Vector2 B { get; set; } = Vector2.Zero;
    public Vector2 C { get; private set; } = Vector2.Zero;

    public override bool Update(float time)
    {
        if (base.Update(time))
        {
            C = Xts.Lerp(A, B, Source);
            return true;
        }
        return false;
    }
} // ALerperV2

/// <summary>
/// Vector3 interpolator
/// </summary>
public class ALerperVector3: ALerperBase
{
    public Vector3 A { get; set; } = Vector3.Zero;
    public Vector3 B { get; set; } = Vector3.Zero;
    public Vector3 C { get; private set; } = Vector3.Zero;

    public override bool Update(float time)
    {
        if (base.Update(time))
        {
            C = Xts.Lerp(A, B, Source);
            return true;
        }
        return false;
    }
} // ALerperVector3

/// <summary>
/// Quaternion interpolator (spherical)
/// </summary>
public class ALerperQ: ALerperBase
{
    public Quat A { get; set; } = Quat.Identity;
    public Quat B { get; set; } = Quat.Identity;
    public Quat C { get; private set; } = Quat.Identity;

    public override bool Update(float time)
    {
        if (base.Update(time))
        {
            C = A.Slerp(B, Source);
            return true;
        }
        return false;
    }
} // ALerperQ
