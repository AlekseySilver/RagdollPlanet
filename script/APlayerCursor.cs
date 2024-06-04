using Godot;
using System;

public class APlayerCursor: Spatial
{
	[Export]
	public float SPEED = 10.0f;


	public override void _Process(float delta)
	{
		var move = Vector3.Zero;
		if (Input.IsActionPressed("ui_left"))
			move.x -= SPEED * delta;
		if (Input.IsActionPressed("ui_right"))
			move.x += SPEED * delta;
		if (Input.IsActionPressed("ui_up"))
			move.z += SPEED * delta;
		if (Input.IsActionPressed("ui_down"))
			move.z -= SPEED * delta;
		if (Input.IsActionPressed("ui_page_up"))
			move.y += SPEED * delta;
		if (Input.IsActionPressed("ui_page_down"))
			move.y -= SPEED * delta;
		if (move != Vector3.Zero)
			Translate(move);
	}
}