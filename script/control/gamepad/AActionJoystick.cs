using Godot;

public sealed class AActionJoystick: AActionButtonBase
{
    public Sprite RoundSprite { get; private set; } = null;

    public override void _Ready()
    {
        base._Ready();
        RoundSprite = this.FirstChild<Sprite>(true, "spr_action_round");
    }

    public const float START_RATE = 0.3f;
    const float END_RATE = 1.0f - START_RATE;

    Vector2 _initPosition;
    Vector2 _touchStartPosition;
    Vector2 _touchLastPosition;
    float _roundOffset; // dead zone
    float _size;
    float _roundOffsetRescale = 1.0f;

    public Vector2 GetDirection()
    {
        if (TouchId == -1)
            return Vector2.Zero;

        var d = _touchLastPosition - _touchStartPosition;
        var len = d.Length();
        if (len < _roundOffset)
            return Vector2.Zero;
        d *= 1.0f / len; // d = d.Normalized();
        if (len > _size)
            len = _size;

        RoundSprite.Offset = d * (len * _roundOffsetRescale);
        d.y = -d.y;
        len /= _size;
        len -= START_RATE;
        len /= END_RATE;
        return d * len;
    } // GetDirection

    public override void Load()
    {
        base.Load();
        _initPosition = RectPosition;
        _size = GetRect().Size.x * 0.5f;
        _roundOffset = _size * START_RATE;
        _roundOffsetRescale = 1f / RoundSprite.Transform.Scale.x;
    }

    public override void StopTouch()
    {
        base.StopTouch();
        RoundSprite.Offset = Vector2.Zero;
        RectPosition = _initPosition;
    }

    public override void UpdateTouch(int touchId, in Vector2 touchPosition)
    {
        if (_touchId == -1)
        {
            _touchId = touchId;
            _touchLastPosition = _touchStartPosition = ClampPosition(touchPosition);
            RectGlobalPosition = _touchStartPosition - new Vector2(_size, _size);
        }
        else if (_touchId == touchId)
            _touchLastPosition = touchPosition;
    } // UpdateTouch
} // AActionJoystick
