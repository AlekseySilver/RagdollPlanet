using Godot;

public class ANPCControl: IPersonControl
{
    public bool IsJumpPressed => false;

    public bool IsKickPressed => false;

    public bool Enabled
    {
        get => true;
        set
        {
        }
    }

    public float JumpPressedTime => 0.0f;

    public float KickPressedTime => 0.0f;

    public Vector2 GetAxis1()
    {
        return Vector2.Zero;
    }
    public Vector3 GetAxis3D1() => Vector3.Zero;

    public Vector2 GetAxis2()
    {
        return Vector2.Zero;
    }

    public float GetVertical()
    {
        return 0.0f;
    }

    public float GetZoom()
    {
        return 0.0f;
    }

    public void UpdateOverride()
    {

    }
}
