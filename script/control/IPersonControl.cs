using Godot;

public interface IPersonControl
{
    /// <summary>
    /// Control Direction in world space XZ
    /// </summary>
    Vector2 GetAxis1();
    Vector3 GetAxis3D1();

    Vector2 GetAxis2();

    float GetZoom();

    float GetVertical();

    void UpdateOverride();

    bool IsJumpPressed { get; }
    bool IsKickPressed { get; }

    float JumpPressedTime { get; }
    float KickPressedTime { get; }

    bool Enabled { get; set; }
}