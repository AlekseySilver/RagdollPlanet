using Godot;

public partial class APerson
{
	[Export] public float START_IN_AIR_TIME = 0.3f;
	[Export] public float LOOSE_WALL_AIR_TIME = 0.5f;
	[Export] public float MOVE_SPEED { get; set; } = 20.0f;

	[Export] public float CTRL_DIR_ACC { get; set; } = 2.0f;

	[Export] public float WALK_RUN_STEP { get; set; } = 15.0f;

	[Export] public float WALK_OVERDRIVE_RATE { get; set; } = 0.3f;

	[Export] public float WALK_OVERDRIVE_MAX { get; set; } = 2.5f;

	[Export] public float RUN_OVERDRIVE_RATE { get; set; } = 0.25f;


	[Export] public float HORIZ_WALL_MOVE_RATE { get; set; } = 0.25f;

	[Export] public float VERT_WALL_MOVE_SPEED { get; set; } = 15.0f;

	[Export] public float VERT_WALL_RUN_OVERDRIVE { get; set; } = 5.5f;


	[Export] public float WALL_CHECK_LEN { get; set; } = 9.0f;

	[Export] public float JUMP_UP_VEL { get; set; } = 20.0f;

	[Export] public float JUMP_GROUP_SPEED { get; set; } = 14.0f;

	[Export] public float GROUD_ROLL_SPEED { get; set; } = 20.0f;



	[Export] public float GRAB_MOVE2POINT_ACC { get; set; } = 500.0f;
	[Export] public float GRAB_MOVE2POINT_VEL { get; set; } = 20.0f;

	[Export]
	public float GRAB_DIST
	{
		get => _grab_dist;
		set
		{
			_grab_dist = value;
			GRAB_DIST_SQ = _grab_dist * _grab_dist;
		}
	}
	float _grab_dist = 1.5f; public float GRAB_DIST_SQ { get; private set; } = 1.5f * 1.5f;

	[Export] public float GRAB_SWING_RATE { get; set; } = 300.0f;
	[Export] public float GRAB_SWING_MAX_SPEED { get; set; } = 30.0f;

	[Export] public float WALL_SLIDE_SPEED { get; set; } = 5.0f;

	[Export] public float WALL_RUN_START_MIN_VEL { get; set; } = -10.0f;

	[Export] public float STAND_RATE { get; set; } = 0.5f;

	[Export] public float GROUND_CHECK_HEIGHT { get; set; } = 2.0f;



	[Export] public float GRAVITY_HEIGHT { get; set; } = 4.0f;
	[Export] public float ON_FLOOR_HEIGHT { get; set; } = 4.0f;


	[Export] public float GRAVITY_RUN { get; set; } = -5.0f;
	[Export] public float GRAVITY_STAND { get; set; } = -12.0f;
	[Export] public float GRAVITY_AIR { get; set; } = -20.0f;
	[Export] public float GRAVITY_GRAB { get; set; } = -10.0f;

	[Export] public float KICK_SPEED { get; set; } = 30.0f;

	[Export(PropertyHint.File)] public string anim_run_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_walk_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_kick_air_L_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_kick_air_R_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_kick_ground_L_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_kick_ground_R_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_punch_air_L_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_punch_air_R_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_punch_ground_L_resource { get; set; }
	[Export(PropertyHint.File)] public string anim_punch_ground_R_resource { get; set; }

	[Export(PropertyHint.File)] public string hit_effect_resource { get; set; }
}
