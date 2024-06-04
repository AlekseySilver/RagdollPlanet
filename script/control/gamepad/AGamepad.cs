using Godot;

public abstract class AGamepad: Control
{
    [Export] public NodePath joystick_;
    [Export] public NodePath jump_button_;
    [Export] public NodePath kick_button_;
    [Export] public NodePath start_button_;
    [Export] public NodePath select_button_;
    [Export] public NodePath MU_button_;
    [Export] public NodePath MD_button_;
    [Export] public NodePath cam_control_;

    protected A App => A.App;
}
