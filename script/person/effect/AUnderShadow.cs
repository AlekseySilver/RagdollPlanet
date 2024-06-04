using Godot;

public class AUnderShadow: MeshInstance
{
    [Export]
    float MIN_LEN
    {
        get => _minLen;
        set
        {
            _minLen = value;
            _minLenSq = _minLen * _minLen;
            _deltaLen = _maxLen - _minLen;
        }
    }
    [Export]
    float MAX_LEN
    {
        get => _maxLen;
        set
        {
            _maxLen = value;
            _maxLenSq = _maxLen * _maxLen;
            _deltaLen = _maxLen - _minLen;
        }
    }

    float _minLen = 4.0f;
    float _minLenSq = 4.0f * 4.0f;
    float _maxLen = 30.0f;
    float _maxLenSq = 30.0f * 30.0f;
    float _deltaLen = 26.0f;

    ShaderMaterial _shader = null;

    public override void _Ready()
    {
        _shader = GetSurfaceMaterial(0) as ShaderMaterial;
        HideShadow();
    }

    public void HideShadow()
    {
        Visible &= false;
    }

    public void Orient(in Vector3 bodyPos, in Vector3 floorPos, in Vector3 floorDir, float moveSpeed)
    {
        var diff = floorPos - bodyPos;
        float t = diff.LengthSquared();
        if (Xts.IsNotBetween(t, _minLenSq, _maxLenSq))
        {
            HideShadow();
            return;
        }
        var a = Mathf.Sqrt(t) / MAX_LEN;
        _shader.SetShaderParam("albedo", new Color(0.0f, 0.0f, 0.0f, Xts.Clamp(a, 0.0f, 1.0f)));
        _shader.SetShaderParam("size", Xts.Clamp(1.0f - a, 0.0f, 1.0f));

        var xf = Transform;
        if (Visible == false)
        {
            xf.origin = floorPos;
            Visible = true;
        }
        else
        {
            diff = floorPos - xf.origin;
            t = diff.LengthSquared();
            float q = moveSpeed * A.TimeStep;
            // in order for the spot not to bounce, its Speed cannot be greater than MOVE_SPEED
            if (Xts.IsNotBetween(t, q * q, 2.0f))
            {
                xf.origin = floorPos;
            }
            else
            {
                xf.origin += diff * (q / Mathf.Sqrt(t));
            }
        }
        xf.basis = Xts.UpAlign(xf.basis, floorDir);
        Transform = xf;
    } // Orient
}
