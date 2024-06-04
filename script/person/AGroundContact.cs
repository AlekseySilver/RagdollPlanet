using Godot;

public class AGroundContact
{
    Vector3 _norm = Vector3.Down;
    Vector3 _pos = Vector3.Zero;

    Vector3 _normSum;
    Vector3 _posSum;
    bool _needUpdate = false;

    public void Add(Vector3 norm, Vector3 pos)
    {
        _normSum += norm;
        _posSum += pos;
        ++Count;
        _needUpdate |= true;
    }
    public void Remove(Vector3 norm, Vector3 pos)
    {
        --Count;
        if (Count == 0)
            _normSum = _posSum = Vector3.Zero;
        else if (Count > 0)
        {
            _normSum -= norm;
            _posSum -= pos;
        }
        else
        {
            Count = 0;
            return;
        }
        _needUpdate |= true;
    }
    public void Update()
    {
        if (_needUpdate)
        {
            if (Count == 1)
            {
                _norm = _normSum;
                _pos = _posSum;
            }
            else if (Count > 1)
            {
                _norm = _normSum.Normalized();
                _pos = _posSum * (1.0f / Count);
            }
            _needUpdate = false;
            HasChange |= true;
        }
        else
            HasChange &= false;
    }

    public void ContactOverride(Vector3 dir, Vector3 pos)
    {
        _norm = dir;
        _pos = pos;
    }

    /// <summary>
    /// last Norm
    /// </summary>
    public Vector3 Norm => _norm;

    /// <summary>
    /// last Pos
    /// </summary>
    public Vector3 Pos => _pos;

    public int Count { get; private set; } = 0;

    public bool HasContact => Count > 0;

    public bool HasChange { get; private set; } = false;
} // AGroundContact
