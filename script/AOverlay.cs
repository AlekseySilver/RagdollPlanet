using Godot;
using System;

public class AOverlay: ColorRect
{
	[Export] public float[] offset_ = new float[] { -3.5f, 2.7f, 0.7f, 3.4f, 5.0f };
	[Export] public float total_offset_ = 1.0f;
	[Export] public float start_dot_ = 0.25f;

	public override void _Process(float delta)
	{
		var scene = A.App.SceneManager as AScenePlay;
		var camXf = scene.Camera.Transform;
		var basis = camXf.basis;
		var camZ = basis.Column2;
		var sunDir = scene.SunDir;

		var dot = camZ.Dot(sunDir);
		// check sun visibility
		if (dot > start_dot_)
		{
			const float OCEAN_RADIUS = 1000f; // TODO Get from Scene
			// cos = sq(1 - sin2)
			var cos = -OCEAN_RADIUS * OCEAN_RADIUS / camXf.origin.LengthSquared() + 1f;
			if (cos < 0.0f)
				cos = 0.0f;
			else
				cos = Mathf.Sqrt(cos);
			var cos2 = sunDir.Dot(scene.Camera.GlobalUp);
			// проверка, что солнце не перекрыто землей
			if (cos2 < cos)
			{
				var c = new Color(1.0f, 1f, 1f, (dot - start_dot_) / (1.0f - start_dot_));

				var v = camZ * dot - sunDir;
				var offset = new Vector2(-basis.Column0.Dot(v), basis.Column1.Dot(v));
				var center = RectSize * 0.5f;

				var n = GetChildCount();
				for (int i = 0; i < n; i++)
				{
					var sp = GetChildOrNull<Sprite>(i);
					var os = offset * (offset_[i] * total_offset_);
					var d = os.LengthSquared();
					//d *= d;
					os *= d;
					sp.Position = center + os;
					sp.SelfModulate = c;
				}
				dot -= 0.2f;
				c.a = dot * dot * dot * dot * dot;
				Color = c;
				Visible |= true; // TODO smooth visible
				return;
			}
		}
		Visible &= false; // TODO smooth invisible
	}
}