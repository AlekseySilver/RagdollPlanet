using Godot;
using System;

public class AUIActionTextEdit: Button
{
    public Action<AUIActionTextEdit, string> OnTextChanged;

    public override void _Ready()
    {
        Connect("text_changed", this, "text_changed");
    }

    public void text_changed(AUIActionTextEdit textEdit, string newText)
    {
        OnTextChanged?.Invoke(this, newText);
    }
} // AUIActionTextEdit
