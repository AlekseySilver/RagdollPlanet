using Godot;
using System;

public class AUIObjectPanel: ColorRect
{
    public Action<AEditor.EMode> OnPressed;

    public void _on_btn_cursor_pressed()
    {
        OnPressed?.Invoke(AEditor.EMode.CURSOR);
    }
    public void _on_btn_select_pressed()
    {
        OnPressed?.Invoke(AEditor.EMode.SELECT);
    }
    public void _on_btn_move_pressed()
    {
        OnPressed?.Invoke(AEditor.EMode.MOVE);
    }
    public void _on_btn_rotate_pressed()
    {
        OnPressed?.Invoke(AEditor.EMode.ROTATE);
    }
    public void _on_btn_scale_pressed()
    {
        OnPressed?.Invoke(AEditor.EMode.SCALE);
    }
} // AUIObjectPanel
