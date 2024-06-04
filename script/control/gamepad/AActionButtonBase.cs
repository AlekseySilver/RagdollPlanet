using Godot;

public abstract class AActionControl: Control
{
    public const float DEFAULT_OPACITY = 50.0f;
    public int TouchId => _touchId;

    protected int _touchId = -1;

    public virtual bool IsTouch => _touchId >= 0;

    public virtual bool Contains(in Vector2 point) => GetGlobalRect().HasPoint(point);

    public abstract void UpdateTouch(int touchId, in Vector2 touchPosition);

    public virtual void StopTouch() => _touchId = -1;

    public abstract void Load();

    public void ScaleSprite()
    {
        this.ForeachChild<Sprite>(true, s =>
        {
            s.ScaleKeepAspect();
            return false;
        });
    }

    public float Opacity
    {
        get
        {
            var s = this.FirstChild<Sprite>(true);
            if (s != null)
                return s.Modulate.a * 100f;
            return DEFAULT_OPACITY;
        }
        set
        {
            this.ForeachChild<Sprite>(true, s =>
            {
                var m = s.Modulate;
                m.a = value * .01f;
                s.Modulate = m;
                return false;
            });
        }
    } // Opacity

    public void SetSize(Vector3 size)
    {
        if (size == Vector3.Zero)
            SetSizeOverride(RectSize.x.XX()); // по умолчанию выравниваем ширину и высоту
        else
        {
            RectGlobalPosition = size.XY();
            SetSizeOverride(size.ZZ());
        }
    }
    public void SetSizeOverride(Vector2 size, bool canFullSize = false)
    {
        var UISize = A.UISize;
        size.x = Xts.Between(size.x, MinSize, canFullSize ? UISize.x : MaxSize);
        size.y = Xts.Between(size.y, MinSize, canFullSize ? UISize.y : MaxSize);
        RectSize = size;
        ScaleSprite();
        SetInsideScreen();
    }
    protected float MinSize => A.UISize.y * 0.1f;
    protected float MaxSize => A.UISize.y * 0.7f;

    public Vector3 GetSizeOverride()
    {
        var size = RectGlobalPosition.XY0();
        size.z = RectSize.x;
        return size;
    }

    public bool IsIntersect(AActionControl other)
    {
        return GetGlobalRect().Intersects(other.GetGlobalRect());
    }

    public bool IsInsideScreen()
    {
        var r = GetGlobalRect();
        if (r.Position.x < 0.0f || r.Position.y < 0.0f)
            return false;
        var s = A.UISize;
        if (r.End.x > s.x || r.End.y > s.y)
            return false;
        return true;
    }

    public void SetInsideScreen()
    {
        var r = GetGlobalRect();
        var pos = RectGlobalPosition;
        if (r.Position.x < 0.0f)
            pos.x -= r.Position.x;
        if (r.Position.y < 0.0f)
            pos.y -= r.Position.y;
        var s = A.UISize;
        if (r.End.x > s.x)
            pos.x -= r.End.x - s.x;
        if (r.End.y > s.y)
            pos.y -= r.End.y - s.y;
        RectGlobalPosition = pos;
    } // SetInsideScreen
} // AActionControl


public abstract class AActionButtonBase: AActionControl
{
    protected Sprite _actionSprite = null;

    Vector2 _positionLimitMin;
    Vector2 _positionLimitMax;

    public override void _Ready()
    {
        base._Ready();
        _actionSprite = this.FirstChild<Sprite>(true);
    }

    public override void Load()
    {
        _positionLimitMin = RectSize / 2;
        _positionLimitMax = A.UISize - _positionLimitMin;
    }

    public Vector2 ClampPosition(Vector2 value)
    {
        if (value.x < _positionLimitMin.x)
            value.x = _positionLimitMin.x;
        else if (value.x > _positionLimitMax.x)
            value.x = _positionLimitMax.x;
        if (value.y < _positionLimitMin.y)
            value.y = _positionLimitMin.y;
        else if (value.y > _positionLimitMax.y)
            value.y = _positionLimitMax.y;
        return value;
    }

    public override void UpdateTouch(int touchId, in Vector2 touchPosition)
    {
        if (_touchId == -1)
            _touchId = touchId;
    }
} // AActionButtonBase


