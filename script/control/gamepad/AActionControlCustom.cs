using Godot;
public sealed class AActionControlCustom: AActionControl
{
    public bool IsTwoTouch => _touch2Id >= 0;

    int _touch2Id = -1;
    Vector2 _touchStartPosition;
    Vector2 _touchLastPosition;
    Vector2 _touch2StartPosition;
    Vector2 _touch2LastPosition;

    public override void UpdateTouch(int touchId, in Vector2 touchPosition)
    {
        if (_touchId == -1)
        {
            _touchId = touchId;
            _touchStartPosition = _touchLastPosition = touchPosition;
        }
        else if (_touchId == touchId)
        {
            _touchLastPosition = touchPosition;
        }
        else if (_touch2Id == -1)
        {
            _touch2Id = touchId;
            _touch2StartPosition = _touch2LastPosition = touchPosition;
        }
        else if (_touch2Id == touchId)
        {
            _touch2LastPosition = touchPosition;
        }
    } // UpdateTouch

    public override void StopTouch()
    {
        base.StopTouch();
        _touch2Id = -1;
    }

    public float GetDeltaScale()
    {
        float start = (_touchStartPosition - _touch2StartPosition).Length();
        float last = (_touchLastPosition - _touch2LastPosition).Length();
        if (start < last)
        {
            return 1.0f;
        }
        else if (last < start)
        {
            return -1.0f;
        }
        return 0.0f;
    } // GetDeltaScale

    public override void Load() { }

    public void SetSize(Vector2 min, Vector2 max)
    {
        if (min == Vector2.Zero || max == Vector2.Zero)
            return;

        RectGlobalPosition = min;
        SetSizeOverride(max - min);
    }
    public void GetSize(out Vector2 min, out Vector2 max)
    {
        min = RectGlobalPosition;
        max = min + RectSize;
    }

    public Vector2 TouchDeltaPosition => _touchLastPosition - _touchStartPosition;
} // AActionControl


