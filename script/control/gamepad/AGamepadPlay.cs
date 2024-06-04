using Godot;
using System.Collections.Generic;

public sealed class AGamepadPlay: AGamepad, IPersonControl
{
    AActionJoystick _joystick;
    AActionButton _jumpButton;
    AActionButton _kickButton;
    AActionButton _startButton;
    AActionButton _selectButton;
    AActionButton _moveUpButton;
    AActionButton _moveDownButton;
    AActionControlCustom _cameraControl;

    readonly Dictionary<int, AActionControl> _touches = new Dictionary<int, AActionControl>(5);

    bool _enabled = true;

    Vector2 _axis1 = Vector2.Zero;
    Vector2 _axis2 = Vector2.Zero;
    Vector2 _cameraRotateSpeed;
    Vector2 _invertedCameraRate;

    float _vertical = 0.0f;

    public Vector2 Rotate => _axis2;
    float _zoom = 0.0f;

    public override void _Ready()
    {
        SetProcessInput(true);
    }

    public void Init()
    {
        _joystick = GetNode<AActionJoystick>(joystick_);
        _jumpButton = GetNode<AActionButton>(jump_button_);
        _kickButton = GetNode<AActionButton>(kick_button_);
        _startButton = GetNode<AActionButton>(start_button_);
        _selectButton = GetNode<AActionButton>(select_button_);
        _moveUpButton = GetNode<AActionButton>(MU_button_);
        _moveDownButton = GetNode<AActionButton>(MD_button_);
        _cameraControl = GetNode<AActionControlCustom>(cam_control_);

        _joystick.SetSize(A.Config.SizeJoystick);
        _jumpButton.SetSize(A.Config.SizeButtonA);
        _kickButton.SetSize(A.Config.SizeButtonB);
        _startButton.SetSize(A.Config.SizeButtonStart);
        _selectButton.SetSize(A.Config.SizeButtonSelect);
        _moveUpButton.SetSize(A.Config.SizeButtonMoveUp);
        _moveDownButton.SetSize(A.Config.SizeButtonMoveDown);
        _cameraControl.SetSize(A.Config.SizeCameraMin, A.Config.SizeCameraMax);

        float opacity = A.Config.Opacity;
        _joystick.Opacity = opacity;
        _jumpButton.Opacity = opacity;
        _kickButton.Opacity = opacity;
        _startButton.Opacity = opacity;
        _selectButton.Opacity = opacity;
        _moveUpButton.Opacity = opacity;
        _moveDownButton.Opacity = opacity;

        _joystick.Load();
        _jumpButton.Load();
        _kickButton.Load();
        _startButton.Load();
        _selectButton.Load();
        _moveUpButton.Load();
        _moveDownButton.Load();
        _cameraControl.Load();

        _cameraRotateSpeed = Vector2.One * 10.1f;

        _invertedCameraRate = _cameraRotateSpeed * (4.0f / A.UISize.y);
        _invertedCameraRate.y *= -1.0f; // для сенсорного экрана Y увеличивается вниз
    } // Init

    public override void _Input(InputEvent e)
    {
        if (e is InputEventScreenTouch te)
        {
            if (te.Pressed) // новое касание
                _ = CheckContains(_startButton, te.Index, te.Position)
                    || CheckContains(_joystick, te.Index, te.Position)
                    || CheckContains(_jumpButton, te.Index, te.Position)
                    || CheckContains(_kickButton, te.Index, te.Position)
                    || CheckContains(_selectButton, te.Index, te.Position)
                    || CheckContains(_moveUpButton, te.Index, te.Position)
                    || CheckContains(_moveDownButton, te.Index, te.Position)
                    || CheckContains(_cameraControl, te.Index, te.Position);
            else if (_touches.TryGetValue(te.Index, out AActionControl ctrl))
            {
                ctrl.StopTouch();
                _touches.Remove(te.Index);
            }
        }
        else if (e is InputEventScreenDrag td)
        {
            if (_touches.TryGetValue(td.Index, out AActionControl ctrl)) // продолжается касание
                ctrl.UpdateTouch(td.Index, td.Position);
        }
    } // _Input

    bool CheckContains(AActionControl control, int touchId, in Vector2 position)
    {
        if (control.Contains(position))
        {
            _touches.Add(touchId, control);
            control.UpdateTouch(touchId, position);
            return true;
        }
        return false;
    } // CheckContains

    public void UpdateOverride()
    {
        if (_enabled == false)
            return;

        _axis2 = Vector2.Zero;
        _zoom = 0.0f;
        _vertical = 0.0f;

        _axis1 = Vector2.Zero;
        _axis1.y += Input.GetActionRawStrength("move_forward");
        _axis1.y -= Input.GetActionRawStrength("move_back");
        _axis1.x -= Input.GetActionRawStrength("move_left");
        _axis1.x += Input.GetActionRawStrength("move_right");
        float len = _axis1.LengthSquared();
        if (len < AActionJoystick.START_RATE * AActionJoystick.START_RATE)
            _axis1 = Vector2.Zero;
        else if (len > 1.0f)
        {
            len = 1.0f / Mathf.Sqrt(len);
            _axis1 *= len;
        }

        if (Input.IsActionPressed("move_up") || _moveUpButton.IsTouch)
            _vertical += 1f;
        if (Input.IsActionPressed("move_down") || _moveDownButton.IsTouch)
            _vertical -= 1f;

        if (Input.IsActionPressed("jump") || _jumpButton.IsTouch)
            JumpPressedTime += A.TimeStep;
        else
            JumpPressedTime = 0.0f;

        if (Input.IsActionPressed("kick") || _kickButton.IsTouch)
            KickPressedTime += A.TimeStep;
        else
            KickPressedTime = 0.0f;

        if (Input.IsActionPressed("start") || _startButton.IsTouch)
        {
            if (StartPressedTime > 0.0f)
                StartPressedTime += A.TimeStep;
            else
                StartPressedTime = Xts.SMALL_FLOAT;
        }
        else
            StartPressedTime = 0.0f;

        if (Input.IsActionPressed("select") || _selectButton.IsTouch)
        {
            if (SelectPressedTime > 0.0f)
                SelectPressedTime += A.TimeStep;
            else
                SelectPressedTime = Xts.SMALL_FLOAT;
        }
        else
            SelectPressedTime = 0.0f;

        if (Input.IsActionPressed("cam_left"))
            _axis2.x -= 1.0f;
        if (Input.IsActionPressed("cam_right"))
            _axis2.x += 1.0f;
        if (Input.IsActionPressed("cam_up"))
            _axis2.y += 1.0f;
        if (Input.IsActionPressed("cam_down"))
            _axis2.y -= 1.0f;

        if (Input.IsActionPressed("cam_forward"))
            _zoom -= 1.0f;
        if (Input.IsActionPressed("cam_back"))
            _zoom += 1.0f;

        if (_cameraControl.IsTouch)
        {
            if (_cameraControl.IsTwoTouch)
                _zoom = _cameraControl.GetDeltaScale();
            else
            {
                _axis2 = _cameraControl.TouchDeltaPosition * _invertedCameraRate;
                float d = _axis2.LengthSquared();
                if (d > 1.0f)
                    _axis2 *= 1.0f / Mathf.Sqrt(d);
            }

        }
    } // UpdateOverride

    /// <summary>
    /// Control Direction in screen space
    /// </summary>
    public Vector2 GetAxis1()
    {
        if (_axis1 == Vector2.Zero)
            _axis1 = _joystick.GetDirection();
        return _axis1;
    }
    public Vector3 GetAxis3D1() => Vector3.Zero;

    public Vector2 GetAxis2() => _axis2;

    public float GetZoom() => _zoom;

    public Vector3 GetAxis2AndZoom() => new Vector3(_axis2.x, _axis2.y, _zoom);

    public float GetVertical() => _vertical;


    public bool IsJumpPressed => JumpPressedTime > 0.0f;
    public bool IsKickPressed => KickPressedTime > 0.0f;

    public bool IsStartPressed => StartPressedTime > 0.0f;
    public bool IsStartJustPressed => StartPressedTime == Xts.SMALL_FLOAT;
    public bool IsSelectPressed => SelectPressedTime > 0.0f;
    public bool IsSelectJustPressed => SelectPressedTime == Xts.SMALL_FLOAT;


    public float JumpPressedTime { get; private set; }
    public float KickPressedTime { get; private set; }

    public float SelectPressedTime { get; private set; }
    public float StartPressedTime { get; private set; }

    public bool Enabled
    {
        get => _enabled;
        set
        {
            if (_enabled == value)
                return;
            _enabled = value;
            if (!_enabled)
            {
                _touches.Clear();
                _joystick.StopTouch();
                _jumpButton.StopTouch();
                _kickButton.StopTouch();
                KickPressedTime = 0.0f;
                JumpPressedTime = 0.0f;
                _axis1 = Vector2.Zero;
                _axis2 = Vector2.Zero;
            }
            Visible = value;
        }
    }
} // AGamepadPlay
