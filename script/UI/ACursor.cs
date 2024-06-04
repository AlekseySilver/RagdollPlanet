using Godot;
using System.Collections.Generic;
using System.Linq;

public sealed class ACursor: Control
{
    class AButton
    {
        public Button Button = null;
        public Vector2 CursorOffset;
        public Vector2 Center => Button.GetGlobalRect().GetCenter();
    }

    Sprite _sprite;
    readonly List<AButton> _buttons = new List<AButton>();
    AButton _currentButton = null;
    bool _enabled = true;
    bool _innerEnabled = false;

    public override void _Ready()
    {
    } // _Ready

    public void Init()
    {
        RectSize = RectSize.y.XX();
        _sprite = this.FirstChild<Sprite>();
        _sprite.ScaleKeepAspect();
    }

    public bool IsEnabled
    {
        get => _enabled;
        private set
        {
            _enabled = value && _innerEnabled;
            TrySetVisible(_enabled);
            if (_enabled)
                Raise();
        }
    }

    void TrySetVisible(bool value)
    {
        if (Visible != value)
            Visible = value && _enabled && _buttons.Count > 0;
    }

    public void HideOverride()
    {
        IsEnabled = false;
        _buttons.Clear();
    }
    public void ShowOverride()
    {
        IsEnabled = true;
        InitButtons();
    }

    public void InitButtons()
    {
        var bak = SelectedButton;
        SetCurrentButton(null);
        var root = GetParent<Control>();
        var screen = new Rect2(Vector2.Zero, A.UISize);

        // fill button list
        _buttons.Clear();
        root.ForeachChild<Button>(true, b =>
        {
            var rect = b.GetGlobalRect();
            if (!b.Disabled //&& b.FocusMode == FocusModeEnum.All
                             && b.IsVisibleInTree() && screen.Intersects(rect))
            {
                int c = (int)(rect.Size.y * .45f);
                _buttons.Add(new AButton
                {
                    Button = b,
                    CursorOffset = new Vector2(c, c)
                });
                ;
            }
            return false;
        });

        // cursor visibility
        TrySetVisible(_buttons.Count > 0);
        if (Visible)
            if (!TrySelect(bak))
                SetCurrentButton(_buttons[0]);
    } // InitButtons

    bool GoToNextButton(Vector2 direction)
    {
        if (_currentButton != null)
        {
            var center = _currentButton.Center;
            direction = direction.Normalized();
            AButton button = null;
            for (int i = 0; i < 2; ++i)
            {
                float maxDot = Xts.SIN15;
                float minLen = float.MaxValue;
                foreach (var o in _buttons)
                {
                    if (o == _currentButton)
                        continue;
                    var delta = o.Center - center;
                    var len = delta.LengthSquared();

                    var _debug = o.Button.Text;

                    delta = delta.Normalized();
                    float dot = direction.Dot(delta);
                    if (dot < maxDot - Xts.SIN05 || len > minLen)
                        continue;
                    minLen = len;
                    maxDot = dot;
                    button = o;
                }
                if (button != null)
                    break;
                center -= direction * A.UISize;
            }
            if (button == null)
                return false;
            SetCurrentButton(button);
        }
        else if (_buttons.Count > 0)
            SetCurrentButton(_buttons[0]);
        return true;
    }

    void SetCurrentButton(AButton value)
    {
        _currentButton = value;
        if (_currentButton == null)
            TrySetVisible(false);
        else
        {
            TrySetVisible(true);
            var half = A.UISize * .5f;
            var center = _currentButton.Button.GetRectGlobalCenter();
            var mag = new Vector2(half.x - center.x > 0 ? 1 : -1, half.y - center.y > 0 ? 1 : -1);

            //mag *= -1;

            float rot;
            // 0 == +1;+1
            // 90 == -1;+1
            // 180 == -1;-1
            // 270 == +1;-1
            if (mag.x > 0)
                rot = mag.y > 0 ? 0.0f : -90f;
            else
                rot = mag.y > 0 ? 90f : 180f;

            // offset by sprite width
            var pos = RectSize * -.5f;
            pos += mag * RectSize * .5f;
            // offset by the width of the button
            pos += _currentButton.CursorOffset * mag;
            // from the center of the button
            pos += center;

            _sprite.Rotation = rot;
            RectGlobalPosition = pos;

            _currentButton.Button.FocusMode = FocusModeEnum.All;
            _currentButton.Button.EnabledFocusMode = FocusModeEnum.All;
            _currentButton.Button.GrabFocus();
        }
    }// current_button

    public Button SelectedButton => _currentButton?.Button;

    public bool TrySelect(Button button)
    {
        var b = _buttons.FirstOrDefault(x => x.Button == button);
        if (b == null)
            return false;
        SetCurrentButton(b);
        return true;
    }

    public override void _Process(float delta)
    {
        var v = Vector2.Zero;
        if (Input.IsActionJustPressed("ui_up"))
            v.y -= 1.0f;
        if (Input.IsActionJustPressed("ui_down"))
            v.y += 1.0f;
        if (Input.IsActionJustPressed("ui_left"))
            v.x -= 1.0f;
        if (Input.IsActionJustPressed("ui_right"))
            v.x += 1.0f;
        if (v != Vector2.Zero)
        {
            if (!_innerEnabled)
            {
                _innerEnabled = true;
                IsEnabled = true;
            }
            if (Visible)
                GoToNextButton(v);
        }

        if (Input.IsActionJustPressed("ui_select") && Visible)
        {
            // click();
            var a = new InputEventAction
            {
                Action = "ui_accept",
                Pressed = true
            };
            Input.ParseInputEvent(a);
        }

        if (Input.IsActionJustReleased("ui_select") && Visible)
        {
            // click();
            var a = new InputEventAction
            {
                Action = "ui_accept",
                Pressed = false
            };
            Input.ParseInputEvent(a);
        }
    } // _Process
} // ACursor
