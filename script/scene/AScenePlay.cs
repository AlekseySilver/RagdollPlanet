using Godot;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

public partial class AScenePlay: AScene
{
	[Export] public float GRAVITY_SCALE = -9.8f;
	[Export] public Color SUN_DAY_COLOR;
	[Export] public Color SUN_DAWN_COLOR;

	[Export(PropertyHint.File)] public string ui_resource;
	[Export(PropertyHint.File)] public string cursor_resource;
	[Export(PropertyHint.File)] public string player_resource;

	[Export] public float FIGHT_CHECK_LEN_SQ = 10000.0f;

	[Export] public NodePath pause_music;


	public AFightManager Fight { get; } = new AFightManager();
	public ACamera Camera { get; protected set; } = null;

	public AEditor Editor { get; } = new AEditor();

	public APerson Player => Fight.Player;

	public AUIGameplayART UI { get; private set; } = null;

	MeshInstance _ocean;
	DirectionalLight _sun;

	ShaderMaterial _backgroundShader = null;

	readonly ALazyUpdate _lazy = new ALazyUpdate
	{
		TimeBetweenUpdate = 1.0f / 25.0f
	};

	public override async Task InitAsync()
	{
		await base.InitAsync();

		A.App.UI.StartFade(false);

		await A.App.UI.SetSceneUIAsync(ui_resource);
		UI = A.App.UI.Gameplay as AUIGameplayART;

		Camera = this.FirstChild<ACamera>(true);
		Camera.SetPhysicsProcess(false);

		_ocean = this.FirstChild<MeshInstance>(true, "SphereCap");
		_sun = this.FirstChild<DirectionalLight>(true);

		//var env = GetWorld().Environment;
		//env.FogEnabled = true;
		//env.DofBlurFarEnabled = true;

		//_backgroundShader = this.FirstChild<WorldEnvironment>(false)?.FirstChild<Panel>(true)?.Material as ShaderMaterial;
		_backgroundShader = this.FirstChild<MeshInstance>(false, "sky")?.GetSurfaceMaterial(0) as ShaderMaterial;

		Editor.Enabled = true;

		Resume();

		Editor.Cursor.SetGlobalPos(Vector3.Up * 1030.0f);
		Camera.AssignTarget(Editor.Cursor);

		/*if (OS.HasFeature("demo_build"))
		{*/
			SetEditor(false);
			Camera.LookAt(Vector3.Forward);
		/*}
		else
		{
			var dfp = A.Config.DetailsFilePath;
			if (!string.IsNullOrWhiteSpace(dfp) && !HasNode(AEditor.EDIT_ROOT))
				Editor.Load(dfp);
		}*/
	} // InitAsync

	public void PersonKnockOut(APerson person)
	{
		if (person.TYPE == APerson.EType.ENEMY)
		{
			Fight.FindNewEnemy();
		}
	}
	public void PersonLifeChanged(APerson person)
	{
		if (person == Fight.Player)
			UI.PlayerChangeHP(person.Life);
		else if (person == Fight.Enemy)
			UI.EnemyChangeHP(person.Life);
	}
	public void PersonFocusChanged(APerson person)
	{
		if (person == Fight.Player)
			UI.Focus = person.FocusPercent;
	}
	public void SetPlayer(APerson person)
	{
		Fight.SetPlayer(person);
		UI.PlayerName = Tr(person.Name);
		UI.PlayerChangeHP(person.Life);
	}
	public void SetEnemy(APerson person)
	{
		if (person == null)
		{
			UI.EnemyName = null;
		}
		else
		{
			UI.EnemyName = Tr(person.Name);
			UI.EnemyChangeHP(person.Life);
		}
	}

	public virtual void Close()
	{
	}

	public override void UpdateOverride()
	{
		base.UpdateOverride();

		if (IsPaused)
			return;

		if (Editor.Enabled)
		{
			Camera.RotateAndZoom(UI.Gamepad.GetAxis2AndZoom() * new Vector3(1.0f, 1.0f, UI.Gamepad.IsJumpPressed ? 10.0f : 1.0f));
		}
		else
		{
			Camera.RotateAndZoom(UI.Gamepad.GetAxis2AndZoom());
			if (UI.Gamepad.GetVertical() < 0.0f)
				Camera.LookAt(Player.LastControlDirection);
		}

		_ocean.GlobalTransform = Xts.UpAlign(_ocean.GlobalTransform, Camera.GlobalUp);

		_sun.RotateX(0.2f * A.TimeStep);

		Fight.Update();

		// SELECT
		if (UI.Gamepad.IsSelectJustPressed)
		{
			PauseSelect();
		} // switch Editor

		// START
		if (UI.Gamepad.IsStartJustPressed)
		{
			Pause();
		} // Pause

		Editor.Update();

		if (_backgroundShader != null)
		{
			//_backgroundShader.SetShaderParam("camera_xform", Camera.Transform);
			//_backgroundShader.SetShaderParam("SunDir", -_sun.Transform.basis.z);

			var sunDir = -_sun.Transform.basis.z;
			float day = Camera.GlobalUp.Dot(sunDir);
			_sun.LightColor = Xts.Lerp(SUN_DAWN_COLOR, SUN_DAY_COLOR, Xts.Abs(day));

			day = day * -0.5f + 0.5f;
			_sun.LightEnergy = Xts.Min(day * 2.0f, 1.0f);

			var par1 = new Transform(
				sunDir, // SunDir
				new Vector3(day, 0.0f, 0.0f), // day + TODO
				Vector3.Zero, // TODO
				Vector3.Zero // TODO
				);

			_backgroundShader.SetShaderParam("par1", par1);
		}

		_lazy.Update();
	} // UpdateOverride

	public override void FixedUpdate()
	{
		base.FixedUpdate();

		//UI.SetDebug0(Camera.current_cam_up.ToString());
		//A.App.UI.SetDebug0(Engine.GetFramesPerSecond().ToString());
		//A.App.UI.SetDebug0(Fight?.Player != null ? Fight.Player.Debug : string.Empty);
		//A.App.UI.SetDebug0(Fight?.Player != null ? ToStringPercent(Fight.Player.BonesCurrentGravityOverride) : string.Empty);
		A.App.UI.SetDebug0(debug0);

		//UI.SetDebug1(ToStringPercent2(Fight.Player.GroundContactDirection, 5));
		//UI.SetDebug1(ToStringPercent2(Fight.Player.Body.AngVel, 9));

		A.App.UI.SetDebug1(Fight.Player?.PrevActionName);
		A.App.UI.SetDebug2(Fight.Player?.ActiveActionName);

		if (IsPaused)
			return;

		Fight.FixedUpdate();
	} // public override void FixedUpdate

	public string debug0;

	string ToStringPercent(Vector3 v)
	{
		v *= 100.0f;
		return $"{(int)v.x}, {(int)v.y}, {(int)v.z}";
	}

	string ToStringPercent2(Vector3 v, int len)
	{
		var a = v.Abs();
		if (a.x > a.y)
		{
			if (a.z < a.x)
				return $"{(v.x < 0.0f ? '-' : '+')}X {ToString3((int)a.x, len)}";
		}
		else
		{
			if (a.z < a.y)
				return $"{(v.y < 0.0f ? '-' : '+')}Y {ToString3((int)a.y, len)}";
		}
		return $"{(v.z < 0.0f ? '-' : '+')}Z {ToString3((int)a.z, len)}";
	}
	string ToString3(int value, int len)
	{
		var res = new string('0', len);
		res = $"{res}{value}";
		res = res.Substring(res.Length - len, len);
		return res;
	}

	public void SetEditor(bool enabled)
	{
		if (enabled == Editor.Enabled)
			return;

		if (enabled)
		{
			Editor.Enabled = true;
			Editor.Control = UI.Gamepad;
			Camera.AssignTarget(Editor.Cursor);
			Camera.SetPhysicsProcess(false);
			Fight.QueueFreePlayer();

			UI.PlayerName = null;
			UI.EnemyName = null;
		}
		else
		{
			// Add player
			var p = Asset.Instantiate2Scene<APerson>(player_resource);
			p.SetGlobalPos(Editor.Cursor.GetGlobalPos());
			p.TYPE = APerson.EType.PLAYER;
			p.Init();

			Editor.Enabled = false;
			Camera.SetPhysicsProcess(true);

			A.App.UI.FirstChild<AUIEditDebug>(true)?.SetObject(p);
		}

		//A.App.UI.FirstChild<AUIEditDebug>(true)?.SetObject(Camera);
	} // SetEditor

	void PausePrerare()
	{
		IsPaused = true;
		Editor.Control = null;
		if (Fight.Player != null)
			Fight.Player.Control = null;
		GetTree().Paused = true;
        GetNodeOrNull<AudioStreamPlayer>(pause_music)?.Play();
    }
	public virtual void PauseSelect()
	{
		PausePrerare();
		UI.ShowMenu_PauseSelect();
	}
	public override void Pause()
	{
		PausePrerare();
		UI.ShowMenu_Pause();
	}
	public virtual void Resume()
	{
		IsPaused = false;
		UI.ShowMenu_Play();
		Editor.Control = UI.Gamepad;
		if (Fight.Player != null)
			Fight.Player.Control = UI.Gamepad;

		GetTree().Paused = false;

		Camera.CAM_ROT_SPEED = A.Config.CameraSpeed * 0.1f;

		GetNodeOrNull<AudioStreamPlayer>(pause_music)?.Stop();
    }

	public async Task DoHiddenTaskAsync(Func<Task> task)
	{
		await A.App.UI.FadeAsync(true);
		await task();
		await A.App.UI.FadeAsync(false);
	}

	public async Task<T> DoHiddenTaskAsync<T>(Func<Task<T>> task)
	{
		await A.App.UI.FadeAsync(true);
		var res = await task();
		await A.App.UI.FadeAsync(false);
		return res;
	}

	public Vector3 SunDir
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => -_sun.Transform.basis.z;
	}

	public void DialogTimelineStart()
	{

	} // DialogTimelineStart

	public void DialogTimelineEnd()
	{

	} // DialogTimelineEnd

	#region dialog events
	public void SwapPlayer()
	{
		var oldPlayer = Player;
		var newPlayer = (oldPlayer.ActiveTrigger as ATriggerDialog)?.OwnerPerson;
		if (newPlayer != null)
		{
			oldPlayer.TYPE = APerson.EType.NPC;
			oldPlayer.NPCDialogName = newPlayer.NPCDialogName;
			newPlayer.TYPE = APerson.EType.PLAYER;
			player_resource = newPlayer.Filename;
		}
	} // SwapPlayer

	public void StartNPCFight()
	{
		var npc = (Player.ActiveTrigger as ATriggerDialog)?.OwnerPerson;
		if (npc != null)
		{
			npc.TYPE = APerson.EType.ENEMY;
		}
	} // StartNPCFight

	#endregion

	public override ACamera GetCamera()
	{
		return Camera;
	}

	public void Add4Lazy(IUpdateable obj)
	{
		_lazy.Add(obj);
	}
	public void Remove4Lazy(IUpdateable obj)
	{
		_lazy.Remove(obj);
	}
} // public class AScenePlay
