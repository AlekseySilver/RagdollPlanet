using Godot;

public class AUIItemScroll: Control
{
    [Export]
    public string Label
    {
        get => this.FirstChild<Label>()?.Text ?? string.Empty;
        set
        {
            var lbl = this.FirstChild<Label>();
            if (lbl != null)
                lbl.Text = value;
        }
    }

    VBoxContainer _vBox;

    public override void _Ready()
    {
        _vBox = this.FirstChild<VBoxContainer>(true);
    }

    public void Add(Control control)
    {
        _vBox.AddChild(control);

    } // Add

    public void Remove(Control control)
    {
        _vBox.RemoveChild(control);
    }

    public void Clear()
    {
        _vBox.ClearChildren();
    }
} // AUIItemScroll
