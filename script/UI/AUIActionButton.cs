using Godot;
using System;

public class AUIActionButton: Button
{
    public Action<AUIActionButton> OnPressed;

    public override void _Ready()
    {
        Connect("pressed", this, "OnButtonPressed");
    }

    void OnButtonPressed()
    {
        OnPressed?.Invoke(this);
    }
}
