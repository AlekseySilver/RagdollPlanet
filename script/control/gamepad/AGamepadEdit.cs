using Godot;

namespace Core
{
    public class AGamepadEdit: AGamepad
	{
		AActionControlCustom _joystick;
		AActionControlCustom _jumpButton;
		AActionControlCustom _kickButton;
		AActionControlCustom _startButton;
		AActionControlCustom _selectButton;
		AActionControlCustom _moveUpButton;
		AActionControlCustom _MoveDownButton;
		AActionControlCustom _cameraControl;

		AActionControlCustom _control = null;

		HSlider _cameraSpeed = null;

		Vector2 _touchStart;
		Vector2 _controlStart;

		int _touch2Id = -1;
		Vector2 _touch2StartDelta;
		Vector2 _touch2ControlSize;

		public override void _Ready()
		{
			_joystick = GetNode<AActionControlCustom>(joystick_);
			_jumpButton = GetNode<AActionControlCustom>(jump_button_);
			_kickButton = GetNode<AActionControlCustom>(kick_button_);
			_startButton = GetNode<AActionControlCustom>(start_button_);
			_selectButton = GetNode<AActionControlCustom>(select_button_);
			_moveUpButton = GetNode<AActionControlCustom>(MU_button_);
			_MoveDownButton = GetNode<AActionControlCustom>(MD_button_);
			_cameraControl = GetNode<AActionControlCustom>(cam_control_);
			_cameraSpeed = GetNode<HSlider>("cam_speed/h_slider");

			_joystick.SetSize(A.Config.SizeJoystick);
			_jumpButton.SetSize(A.Config.SizeButtonA);
			_kickButton.SetSize(A.Config.SizeButtonB);
			_startButton.SetSize(A.Config.SizeButtonStart);
			_selectButton.SetSize(A.Config.SizeButtonSelect);
			_moveUpButton.SetSize(A.Config.SizeButtonMoveUp);
			_MoveDownButton.SetSize(A.Config.SizeButtonMoveDown);
			_cameraControl.SetSize(A.Config.SizeCameraMin, A.Config.SizeCameraMax);
			_cameraSpeed.Value = A.Config.CameraSpeed;

			float opacity = A.Config.Opacity;
			_joystick.Opacity = opacity;
			_jumpButton.Opacity = opacity;
			_kickButton.Opacity = opacity;
			_startButton.Opacity = opacity;
			_selectButton.Opacity = opacity;
			_moveUpButton.Opacity = opacity;
			_MoveDownButton.Opacity = opacity;
		} // Init

		public override void _Input(InputEvent e)
		{
			if (e is InputEventScreenTouch te)
			{
				if (te.Pressed) // новое касание
					DoTouch(te.Index, te.Position);
				else if (_control != null && te.Index == _control.TouchId)
				{
					_control = null;
					_touch2Id = -1;
				}
				else if (te.Index == _touch2Id)
					_touch2Id = -1;
			}
			else if (e is InputEventScreenDrag td)
			{
				DoTouch(td.Index, td.Position);
			}
		} // _Input

		void DoTouch(int touchId, in Vector2 pos)
		{
			if (_control == null) // move Start
				_ = CheckContains(_joystick, touchId, pos)
					|| CheckContains(_jumpButton, touchId, pos)
					|| CheckContains(_kickButton, touchId, pos)
					|| CheckContains(_startButton, touchId, pos)
					|| CheckContains(_selectButton, touchId, pos)
					|| CheckContains(_moveUpButton, touchId, pos)
					|| CheckContains(_MoveDownButton, touchId, pos)
					|| CheckContains(_cameraControl, touchId, pos);
			else if (_control.TouchId == touchId)
			{ // move do
				_control.RectGlobalPosition = _controlStart
					+ pos
					- _touchStart;
				_control.SetInsideScreen();
			}
			else if (_touch2Id == -1)
			{ // scale Start
				_touch2Id = touchId;
				if (_control != _cameraControl)
					_touch2StartDelta.x = (pos - _touchStart).Length();
				else
					_touch2StartDelta = (pos - _touchStart).Abs();
				_touch2ControlSize = _control.RectSize;
			}
			else if (_touch2Id == touchId)
			{ // scale do
				var delta = pos - _touchStart;
				if (_control != _cameraControl)
				{
					delta = (delta.Length() - _touch2StartDelta.x).XX();
				}
				else
				{
					delta = delta.Abs() - _touch2StartDelta;
				}

				var center = _control.GetRectGlobalCenter();
				delta += _touch2ControlSize;
				_control.SetSizeOverride(delta, _control == _cameraControl);
				_control.SetRectGlobalCenter(center);
				_control.SetInsideScreen();
			}
		} // DoTouch

		bool CheckContains(AActionControlCustom control, int touchId, in Vector2 pos)
		{
			if (control.Contains(pos))
			{
				_control = control;
				control.UpdateTouch(touchId, pos);
				_controlStart = _control.RectGlobalPosition;
				_touchStart = pos;
				return true;
			}
			return false;
		} // CheckContains

		public void _on_btn_back_pressed()
		{
			// Save all
			A.Config.SizeJoystick = _joystick.GetSizeOverride();
			A.Config.SizeButtonA = _jumpButton.GetSizeOverride();
			A.Config.SizeButtonB = _kickButton.GetSizeOverride();
			A.Config.SizeButtonStart = _startButton.GetSizeOverride();
			A.Config.SizeButtonSelect = _selectButton.GetSizeOverride();
			A.Config.SizeButtonMoveUp = _moveUpButton.GetSizeOverride();
			A.Config.SizeButtonMoveDown = _MoveDownButton.GetSizeOverride();

			_cameraControl.GetSize(out var min, out var max);
			A.Config.SizeCameraMin = min;
			A.Config.SizeCameraMax = max;

			A.Config.Opacity = _jumpButton.Opacity;
			A.Config.CameraSpeed = (float)_cameraSpeed.Value;

			A.Config.Save();

			(A.App.UI.Gameplay as AUIGameplayART)?.ShowMenu_Pause();
		} // _on_btn_back_pressed

		public void _on_btn_default_pressed()
		{
			// Save default
			A.Config.SizeJoystick = Vector3.Zero;
			A.Config.SizeButtonA = Vector3.Zero;
			A.Config.SizeButtonB = Vector3.Zero;
			A.Config.SizeButtonStart = Vector3.Zero;
			A.Config.SizeButtonSelect = Vector3.Zero;
			A.Config.SizeButtonMoveUp = Vector3.Zero;
			A.Config.SizeButtonMoveDown = Vector3.Zero;
			A.Config.SizeCameraMin = Vector2.Zero;
			A.Config.SizeCameraMax = Vector2.Zero;

			A.Config.Opacity = AActionControl.DEFAULT_OPACITY;
			A.Config.CameraSpeed = 50.0f;

			A.Config.Save();

			(A.App.UI.Gameplay as AUIGameplayART)?.ShowMenu_EditControl();
		} // _on_btn_back_pressed

		public void _on_h_slider_value_changed(float value)
		{
			float opacity = 100.0f - value;
			_joystick.Opacity = opacity;
			_jumpButton.Opacity = opacity;
			_kickButton.Opacity = opacity;
			_startButton.Opacity = opacity;
			_selectButton.Opacity = opacity;
			_moveUpButton.Opacity = opacity;
			_MoveDownButton.Opacity = opacity;
		} // _on_h_slider_value_changed

		public void _on_h_slider_cam_speed_value_changed(float value)
		{
		}
	}
}