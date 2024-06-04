using Godot;
using System;

public static class Asset
{
    /// <summary>
    /// instantiate asset by Name
    /// </summary>
    public static T Instantiate<T>(string resource) where T : Node
    {
        T i = GD.Load<PackedScene>(resource).Instance<T>();
        return i;
    }

    public static T Instantiate<T>(string resource, Node parent) where T : Node
    {
        T i = Instantiate<T>(resource);
        parent.AddChild(i);
        parent.MoveChild(i, 0);
        return i;
    }
    public static T Instantiate2Scene<T>(string resource) where T : Node
    {
        return Instantiate<T>(resource, A.App.SceneManager);
    }
    public static T Instantiate2UI<T>(string resource) where T : Node
    {
        return Instantiate<T>(resource, A.App.UI);
    }




    public const string RibbonTrail = "res://scene/person/effect/RibbonTrail.tscn";
    public const string RibbonTrail2 = "res://scene/person/effect/RibbonTrail2.tscn";
    public const string UnderShadow = "res://scene/person/effect/UnderShadow.tscn";
    public const string DashCursor = "res://scene/person/effect/DashCursor.tscn";


    public const string touch_gamepad = "res://scene/UI/touch_gamepad.tscn";
    public const string edit_touch_gamepad = "res://scene/UI/edit_touch_gamepad.tscn";
    public const string select_item_menu = "res://scene/UI/select_item_menu.tscn";

    public const string pause_menu = "res://scene/UI/pause_menu.tscn";
    public const string select_menu = "res://scene/UI/select_menu.tscn";

    public const string anim_edit_menu = "res://scene/UI/anim_edit.tscn";

    public const string key_edit_element = "res://scene/UI/key_edit_element.tscn";

    public const string capsule_shape = "res://geometry/capsule_shape.shape";

    public const string debug_arrow = "res://scene/editor/debug_arrow.tscn";

    public const string file_dialog = "res://scene/UI/file_dialog.tscn";

    public const string trigger_dialog = "res://scene/trigger/trigger_dialog.tscn";

    public const string material_outline = "res://material/outline.material";

}