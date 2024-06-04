using Godot;
using System;

public class APlanetBase: Node
{
	public override void _Ready()
	{
		var cam = this.FirstChild<ACamera>(true);
		var cur = this.FirstChild<APlayerCursor>(true);

		cam.AssignTarget(cur);
	}
}
