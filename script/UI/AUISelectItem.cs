using Godot;
using System;
using System.Collections.Generic;

public class AUISelectItem: ColorRect
{
    public Action<object> OnResult { get; set; }

    AUIItemScroll _itemScroll;

    readonly Dictionary<AUIActionButton, object> _index = new Dictionary<AUIActionButton, object>();

    public void Add(string key, object value)
    {
        var b = new AUIActionButton
        {
            Text = key,
            OnPressed = _on_btn_pressed
        };
        _itemScroll.Add(b);
        _index.Add(b, value);
    }

    public override void _Ready()
    {
        _itemScroll = this.FirstChild<AUIItemScroll>(true);
    }

    void _on_btn_pressed(AUIActionButton button)
    {
        if (_index.TryGetValue(button, out var value))
            OnResult?.Invoke(value);
    }

    void _on_btn_cancel_pressed()
    {
        OnResult?.Invoke(null);
    }
} // AUISelectItem
