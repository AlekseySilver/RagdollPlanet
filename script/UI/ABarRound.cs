using Godot;

public sealed class ABarRound: Panel
{
    [Export] public float STATE_SPEED = 200.0f;

    [Export] public float SHOW_TIME_100_PERCENT = 1.0f;

    float _percent;
    bool _needUpdate;
    float _targetPercent;

    ShaderMaterial _mat;

    float _showRest = 0.0f;

    public override void _Ready()
    {
        _mat = Material as ShaderMaterial;
        Percent = 0.0f;
    }

    public void UpdateOverride()
    {
        if (_needUpdate)
        {
            float percent = Percent;
            if (percent < TargetPercent)
            {
                percent += A.TimeStep * STATE_SPEED;
                if (percent >= TargetPercent)
                {
                    _needUpdate = false;
                    percent = TargetPercent;
                }
            }
            else
            {
                percent -= A.TimeStep * STATE_SPEED;
                if (percent <= TargetPercent)
                {
                    _needUpdate = false;
                    percent = TargetPercent;
                }
            }
            Percent = percent;

            if (percent >= 100f)
                _showRest = SHOW_TIME_100_PERCENT;
        }
        else if (_showRest > 0.0f)
        {
            _showRest -= A.TimeStep;
            if (_showRest <= 0.0f)
                Visible = false;
        }
    } // public void UpdateOverride

    public float Percent
    {
        get => _percent;
        set
        {
            _percent = value;
            _mat.SetShaderParam("value", value * 0.01f);
        }
    } // Percent

    public float TargetPercent
    {
        get => _targetPercent;
        set
        {
            _targetPercent = value;
            _needUpdate = true;
            if (_targetPercent < 100.0f)
                Visible |= true;
        }
    }
} // ABarRound
