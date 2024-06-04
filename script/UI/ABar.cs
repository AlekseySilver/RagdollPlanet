using Godot;

public class ABar: Control
{
    Tween _tween;
    Label _number;
    Label _name;
    TextureProgress _bar;

    float _animatedHealth = 0.0f;

    public float MaxHealth
    {
        get => (float)_bar.MaxValue;
        set => _bar.MaxValue = value;
    }
    public string NameText
    {
        get => _name.Text;
        set => _name.Text = value;
    }

    public override void _Ready()
    {
        _bar = GetNode<TextureProgress>("TextureProgress");
        _tween = GetNode<Tween>("Tween");
        _number = GetNode<Label>("Number");
        _name = GetNode<Label>("Name");
    }

    void _on_tween_tween_step(Godot.Object obj, NodePath key, float elapsed, Godot.Object value)
    {
        var rounded = Mathf.RoundToInt(_animatedHealth);
        _number.Text = rounded.ToString();
        _bar.Value = rounded;
    }

    public void ChangeHP(float hp)
    {
        _tween.InterpolateProperty(this, nameof(_animatedHealth), _animatedHealth, hp, 0.6f, Tween.TransitionType.Linear, Tween.EaseType.In);
        if (_tween.IsActive() == false)
        {
            _tween.Start();
        }
    }

    public void ZeroHP()
    {
        var startColor = new Color(1.0f, 1.0f, 1.0f);
        var endColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);

        _tween.InterpolateProperty(this, "modulate", startColor, endColor, 1.0f, Tween.TransitionType.Linear, Tween.EaseType.In);
    }
}
