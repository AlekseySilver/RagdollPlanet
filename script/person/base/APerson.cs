using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class APerson: Spatial, IControlable, IHarmable
{
    protected enum EBonesGravityState { ZERO = 0, DEFAULT, CUSTOM, FLOOR }

    // - Character parameters:
    public float HIT_STREINGTH { get; set; } = 10.0f; // -- impact force (the amount of Life that the impact takes away)
    public float MAX_FOCUS_TIME { get; set; } = 1.5f; // -- Focus duration
    public float MAX_LIFE { get; set; } = 100.0f; // -- endurance
    public float CLOSE_COMBAT_RATE { get; set; } = 0.75f; // -- melee (the minimum Speed at which a hit occurs)

    protected const float MAX_HIT_FORCE = 150.0f;
    protected const float KNOCK_DOWN_TIME = 1.0f;

    const float USER_CTRL_RATE = 0.333f;
    const float AIR_CTRL_RATE = 1.0f - USER_CTRL_RATE;

    const float JUMP_MAX_TIME = 0.65f;
    const float KICK_MAX_TIME = 0.45f;

    public Vector3 UpDirection { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }

    public ABone3 Body { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
    public RigidBody RBody { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Body; }

    public AScenePlay SceneManager { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; } = null;

    public Spatial TeleportNode { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; }

    float _airTime = 1.0f;
    float _focus = 0.0f;
    float _life = 0.0f;

    float _FSMActionRestTime2 = 0.0f;
    float _FSMActionRestTime = 0.0f;
    Vector3 _FSMActionVector = Vector3.Zero;
    Vector3 _FSMActionVector2 = Vector3.Zero;

    Vector3 _focusLastRestoreNorm = Vector3.Zero;

    public Vector3 BonesCurrentGravityOverride { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; }
    protected EBonesGravityState _bonesGravityState = EBonesGravityState.DEFAULT;

    float _gravityScale = -20.0f;

    public Vector3 ControlDirection { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; protected set; } = Vector3.Zero;

    public Vector3 SumControlDirection { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; protected set; } = Vector3.Zero;
    public float SumControlLength { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; protected set; } = 0.0f;

    public Vector3 LastControlDirection { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; protected set; } = Vector3.Right;

    /// <summary>
    /// The user'S direction when the character rose into the air
    /// </summary>
    Vector3 _airControlDirection = Vector3.Zero;

    Vector3 _kickDirection = Vector3.Zero;

    /// <summary>
    /// directional jump velocity
    /// </summary>
    Vector3 _jumpVel;

    public ABone1 ActiveBone { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; } = null;

    ARibbonTrail2 _kickTrail;
    AHitEffect _hitEffect;

    const float FLOOR_CHECK_MAX_TIME = 1f / 25f; // проверять 25 раз в секунду
    const float FLOOR_CHECK_LEN = 30f;
    float _floorCheckRestTime = 0.0f;

    Vector3 _floorDirection;
    Vector3 _floorPosition;
    public float FloorHeight { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; private set; } = 0.0f;
    Spatial floor_body = null;
    bool has_floor => FloorHeight >= 0.0f;

    MeshInstance _mesh;
	Skeleton _skel;
	readonly Dictionary<ABone.EPart, ABone> _bones = new Dictionary<ABone.EPart, ABone>();

	// bones sorted to UpdateOverride the skeleton hierarchy
	ABone1[] _skBones = null;

	public void Init()
	{
		SceneManager = A.App.SceneManager as AScenePlay;

		_skel = this.FirstChild<Skeleton>(true);
		_mesh = _skel.FirstChild<MeshInstance>();

		// fill Bones
		// Bone names
		var di = new Dictionary<string, ABone.EPart>();
		foreach (ABone.EPart p in Enum.GetValues(typeof(ABone.EPart)))
			di.Add("Bone" + p.ToString(), p);

		int n = _skel.GetBoneCount();
		for (int i = 0; i < n; ++i)
		{
			var name = _skel.GetBoneName(i);
			//GD.Print(Name);
			var b = this.FirstChild<ABone>(false, name);
			if (b != null)
			{
				b.Person = this;
				b.SkBoneIdx = i;
				b.SkBoneParentIdx = _skel.GetBoneParent(i);

				if (di.TryGetValue(name, out var bp))
					_bones.Add(bp, b);
				else
					GD.PrintErr($"Unknown bone name {name}");
			}
		} // for

		// Parent link
		n = GetChildCount();
		for (int i = 0; i < n; ++i)
		{
			var j = GetChildOrNull<AJoint>(i);
			if (j != null)
			{
				var a = j.GetNodeOrNull<ABone1>(j.Nodes__nodeA);
				var b = j.GetNodeOrNull<ABone>(j.Nodes__nodeB);
				if (a == null)
				{
					GD.PrintErr($"Joint {j.Name} no node A");
					continue;
				}
				if (b == null)
				{
					GD.PrintErr($"Joint {j.Name} no node B");
					continue;
				}
				a.Joint = j;
				a.Parent = b;
			}
		}
		// sort by hierarchy level, ---all Bones with Parent (skel is child of Body node)
		var tmpBones = _bones
			.Select(x => x.Value)
			.OrderBy(x => x.LimbLevel)
			.ToArray();

		// Parent rotation offsets
		foreach (var bone in tmpBones)
		{
			// turn off Custom pose
			_skel.SetBoneCustomPose(bone.SkBoneIdx, Transform.Identity);

			// Get total Bone pose
			var bonePose = _skel.GetBoneRest(bone.SkBoneIdx) * _skel.GetBonePose(bone.SkBoneIdx);
			// turn off rest pose to reduse calculations
			_skel.SetBoneDisableRest(bone.SkBoneIdx, true);
			// change to total pose
			_skel.SetBonePose(bone.SkBoneIdx, bonePose);
			// skeleton Bone Position
			bone.SkBoneLocalOrigin = bonePose.origin;

			// rotates 		// b - Bones, r - rBody, d = delta = RBody2SkBoneOffset
			// b_global = r_global + d
			// b_global = b_parent_global + b_pose
			bone.SkBoneGlobalBasis = bonePose.basis;
			if (bone.Parent != null)
				bone.SkBoneGlobalBasis = bone.Parent.SkBoneGlobalBasis * bone.SkBoneGlobalBasis;
			bone.RBody2SkBoneOffset = Xts.TermSecond(bone.SkBoneGlobalBasis, bone.RBodyBasis);

			(bone as ABone1)?.FindDirectParent();
		} // for

		_skBones = tmpBones.Where(x => x is ABone1).Select(x => (ABone1)x).ToArray();

		Body = (ABone3)_bones[ABone.EPart.Body];

		Life = MAX_LIFE;
		Focus = 0;

		InitBehavior();
		InitFSM();

		// kick trail
		//_kickTrail = Asset.Instantiate2Scene<ARibbonTrail>(Asset.RibbonTrail);
		_kickTrail = Asset.Instantiate<ARibbonTrail2>(Asset.RibbonTrail2, this);
		//_kickTrail.Emitter = Body; // for trail Debug

		_hitEffect = Asset.Instantiate<AHitEffect>(hit_effect_resource, this);

		_type.Start(this);
	} // Init

	public new void QueueFree()
	{
		_type.Remove(this);
		base.QueueFree();
	}

	public void Update()
	{
		// UpdateOverride skeleton Bone rotations
		Body.SkBoneGlobalBasis = Body.RBodyBasis * Body.RBody2SkBoneOffset;
		foreach (var bone in _skBones)
		{
			// new Bone global rotation
			bone.SkBoneGlobalBasis = bone.RBodyBasis * bone.RBody2SkBoneOffset;
			var d = Xts.TermSecond(bone.SkBoneGlobalBasis, bone.Parent.SkBoneGlobalBasis);
			_skel.SetBonePose(bone.SkBoneIdx, new Transform(d, bone.SkBoneLocalOrigin));
		} // for

		Control.UpdateOverride();
		ControlDirection = _type.GetControlWorldDirection(this);
		if (ControlDirection != Vector3.Zero)
		{
			LastControlDirection = ControlDirection.Normalized();

            // accelerating
            SumControlDirection += ControlDirection * (CTRL_DIR_ACC * A.TimeStep);
			SumControlLength = SumControlDirection.Length();
			float len = ControlDirection.Length();
			if (SumControlLength > len)
			{
				SumControlDirection *= len / SumControlLength;
				SumControlLength = len;
			}
		}
		else if (SumControlDirection != Vector3.Zero)
		{
            // stopping
            var n = SumControlDirection * (1.0f / SumControlLength);

			var m = CTRL_DIR_ACC * A.TimeStep;
			if (SumControlLength > m)
			{
				SumControlDirection -= n * m;
				SumControlLength -= m;
			}
			else
			{
				SumControlDirection = Vector3.Zero;
				SumControlLength = 0.0f;
			}
		}

		_type.Update(this);
	} // UpdateOverride

	public void FixedUpdate()
	{
		UpDirection = Body.Position.Normalized();

		// UpdateOverride ground data
		float maxDot = float.MinValue;
		HasGroundContact = false;

		if (Body.HasGroundContact)
		{
			maxDot = UpDirection.Dot(Body.LastGroundContactNorm);
			HasGroundContact = true;
			GroundContactDirection = Body.LastGroundContactNorm;
			GroundContactPosition = Body.LastGroundContactPos;
		}

		foreach (var bone in Bones)
		{
			if (bone.HasGroundContact)
			{
				float dot = UpDirection.Dot(bone.LastGroundContactNorm);
				if (dot > maxDot)
				{
					HasGroundContact = true;
					maxDot = dot;
					GroundContactDirection = bone.LastGroundContactNorm;
					GroundContactPosition = bone.LastGroundContactPos;
				}
			}
		} // foreach

		_FSM.Update();

		FloorCheck(A.FixedStep);

		AirCheck(A.FixedStep);

		switch (_bonesGravityState)
		{
			case EBonesGravityState.DEFAULT:
				BonesCurrentGravityOverride = UpDirection * _gravityScale;
				break;
			case EBonesGravityState.FLOOR:
				float scale = _gravityScale;

				if (Xts.IsBetween(FloorHeight, 0.0f, GRAVITY_HEIGHT))
				{
					scale *= FloorHeight / GRAVITY_HEIGHT;
				}

				BonesCurrentGravityOverride = _floorDirection * scale;
				break;
		} // _bonesGravityState

		Debug = $"st={_bonesGravityState}\r\ndot={Math.Round(_floorDirection.Dot(RunDirection3), 2)}";
	} // void _PhysicsProcess

	protected void GravitySetStand() => _gravityScale = GRAVITY_STAND;
	protected void GravitySetRun() => _gravityScale = GRAVITY_RUN;
	protected void GravitySetInAir() => _gravityScale = GRAVITY_AIR;
	protected void GravitySetGrab() => _gravityScale = GRAVITY_GRAB;

	protected bool IsOnFloor
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _airTime < LOOSE_WALL_AIR_TIME && Xts.IsBetween(FloorHeight, 0.0f, ON_FLOOR_HEIGHT);
	}

	bool IsOnWall
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _airTime < LOOSE_WALL_AIR_TIME && Xts.IsBetween(GroundContactDirection.Dot(Body.Position - GroundContactPosition), 0.0f, ON_FLOOR_HEIGHT);
	}

	void FloorCheck(float delta)
	{
		_floorCheckRestTime -= delta;
		if (_floorCheckRestTime < 0.0f)
		{
			_floorCheckRestTime = FLOOR_CHECK_MAX_TIME;

			floor_body = null;
			FloorHeight = -1.0f;
			var bpos = Body.Position;
			if (Body.IntersectRay(bpos, UpDirection * -FLOOR_CHECK_LEN, Xts.GROUND_LAYER_VALUE, out var fdir, out var fpos, out var fbody))
			{
				_floorDirection = fdir;
				if (fdir.Dot(UpDirection) > Xts.SIN10)
				{
					_floorPosition = fpos;
					floor_body = fbody;
					const float offset = .09f; // so that the shadow is displayed normally
                    // HeightMapShape inconvenient to use
                    // var Shape = fbody.FirstChild<CollisionShape>();
                    // if (Shape?.Shape is HeightMapShape hm) {
                    //     offset += hm.Margin;
                    // }
                    _floorPosition += UpDirection * offset;

					FloorHeight = UpDirection.Dot(bpos - _floorPosition);
					if (IsOnFloor)
					{
						HasGroundContact = true;
						GroundContactDirection = _floorDirection;
						GroundContactPosition = _floorPosition;
					}
				}
			}
		}
    } // FloorCheck

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void AirCheck(float delta)
	{
		if (HasGroundContact)
		{
			_airTime = 0.0f;
			RestoreFocusCheck();
		}
		else
		{
			_airTime += delta;
		}
	} // void AirCheck

	void RestoreFocusCheck()
	{
		if (Focus == MAX_FOCUS_TIME)
			return;
		float up = UpDirection.Dot(GroundContactDirection);

		if (IsKick || up < -Xts.SIN15) // ceiling
            return; // do not restore

        if (up > Xts.SIN45) // floor
			_focusLastRestoreNorm = Vector3.Zero;
        else // walls
        {
			// new direction
            var newRestoreNorm = GroundContactDirection;
            // the walls should be differently directed
            if (_focusLastRestoreNorm != Vector3.Zero && newRestoreNorm.Dot(_focusLastRestoreNorm) > Xts.SIN45)
				return; // do not restore
            _focusLastRestoreNorm = newRestoreNorm;
		}

		Focus = MAX_FOCUS_TIME;
	} // void RestoreFocusCheck

    /// <summary>
    /// restore Focus forced
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void RestoreFocus()
	{
		if (Focus == MAX_FOCUS_TIME)
			return;
		Focus = MAX_FOCUS_TIME;
		_focusLastRestoreNorm = Vector3.Zero;
	} // void RestoreFocusCheck

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void BonesGravityDefault()
	{
		_bonesGravityState = EBonesGravityState.DEFAULT;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void BonesGravity2Floor()
	{
		_bonesGravityState = EBonesGravityState.FLOOR;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void BonesGravityZero()
	{
		if (_bonesGravityState == EBonesGravityState.ZERO)
			return;
		BonesCurrentGravityOverride = Vector3.Zero;
		_bonesGravityState = EBonesGravityState.ZERO;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void BonesGravityCustom(Vector3 direction)
	{
		BonesCurrentGravityOverride = direction * SceneManager.GRAVITY_SCALE;
		_bonesGravityState = EBonesGravityState.CUSTOM;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void BonesGravity2WallSlide()
	{
		BonesGravityCustom((GroundContactDirection + UpDirection).Normalized());
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void BonesGravity2Wall()
	{
		BonesGravityCustom(GroundContactDirection);
	}

    /// <summary>
    /// accurate check of the ground under feet
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CheckGroundExact() => CheckGroundExact(-GroundContactDirection);
	public bool CheckGroundExact(Vector3 checkDirection)
	{
		if (HasGroundContact)
			return true;

		if (IsInAir)
		{
			HasGroundContact = this.IntersectRay(Body.Position, checkDirection * GROUND_CHECK_HEIGHT, Xts.GROUND_LAYER_VALUE, out var normal, out var position);
			if (HasGroundContact)
			{
				_airTime = 0.0f; // on floor
				GroundContactPosition = position;
				GroundContactDirection = normal;
			}
			else
				return false; // in air
		}
		return true;
	} // public bool CheckGroundExact

	public bool IsKnockedOut
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _FSM.ActiveActionKey == EGMAction.KNOCK_OUT;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CanHit()
	{
		switch (_FSM.ActiveActionKey)
		{
			case EGMAction.KNOCK_DOWN:
			case EGMAction.KNOCK_OUT:
				return false;
		}
		return true;
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public virtual bool CanReceiveHit() => CanHit();

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool CanHit(in RigidBody rBody)
	{
		return SceneManager.Fight.GetHarmable(rBody) != this;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void TryHit(ref SHitData data)
	{
		if (CanHit())
		{
			data.Hitter = this;
			SceneManager.Fight.PerformHit(ref data);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void HitDone(ref SHitData data)
	{
		RestoreFocus();
		_hitEffect.Fire(ref data);
	}

	public void ReceiveHit(ref SHitData data)
	{
		data.VictimBody.ApplyImpulse(data.WorldPosition, data.Impulse);
		Life -= data.Hitter.HIT_STREINGTH;
		Focus = 0.0f;
		if (Life > 0.0f)
			DoAction(EGMAction.KNOCK_DOWN, false);
		else
			KnockOut();
	}

	/// <summary>
	/// KNOCK OUT
	/// </summary>
	public void KnockOut()
	{
		if (ActiveAction == EGMAction.KNOCK_OUT)
			return;
		DoAction(EGMAction.KNOCK_OUT, false);
		Life = 0.0f;
		_type.KnockOut(this);
		SceneManager.PersonKnockOut(this);
	}

	public virtual void OutOfBound()
	{
		Life -= 10.0f;
		if (Life <= 0.0f)
			KnockOut();
		else
			Teleport2Position();
	}

	protected void Disable()
	{
		SceneManager.Fight.RemovePerson(this);
	}
	protected virtual void Enable()
	{
	}

	public RigidBody FindRivalBody(Vector3 StartPosition)
	{
		SceneManager.Fight.HasNearestRival(this, StartPosition, out APerson _, out RigidBody b);
		return b;
	}

	public bool CanSeeRival(out APerson rival)
	{
		rival = null;
		if (false == SceneManager.Fight.HasNearestRival(this, Body.Position, out APerson rp, out RigidBody rb))
			return false; // the rival'S Person was not found

        var delta = rb.GlobalTransform.origin - Body.Position;
		float len = delta.LengthSquared();
		if (len > 5f)
		{
			uint mask = Body.CollisionMask; // & ~Body.CollisionLayer; // Without self

            if (!this.IntersectRay(Body.Position, rb.GlobalTransform.origin, mask, out var resBody))
				return false;

			if (!(resBody is RigidBody rig) || rig.CollisionLayer != rb.CollisionLayer)
				return false; // ray hits the body with another layer (not the rival'S layer)
        }
		rival = rp;
		return true; // rival is visible
    }

	public void Teleport2Position() => Teleport2Position(TeleportNode.GlobalTransform.origin);
	public void Teleport2Position(Vector3 worldPosition)
	{
		var offset = worldPosition - Body.Position;

		foreach (var bone in _bones)
			bone.Value.GlobalTranslate(offset);

		DoAction(EGMAction.RELAX);
	}

	public bool IsInAir { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _airTime > START_IN_AIR_TIME; }

	public bool HasGroundContact { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; protected set; }

	public Vector3 GroundContactDirection { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; protected set; }
	public Vector3 GroundContactPosition { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; protected set; }

	public EGMAction FSMActiveAction { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _FSM.ActiveActionKey; }
	public bool HasActivePart { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => ActiveBone != null; }

	public virtual float Focus
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _focus;
		protected set
		{
			_focus = value;
			SceneManager.PersonFocusChanged(this);
		}
	}
	public virtual float Life
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _life;
		set
		{
			_life = value;
			SceneManager.PersonLifeChanged(this);
			if (_life <= 0.0f)
				KnockOut();
		}
	}

	public float FocusPart { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Focus / MAX_FOCUS_TIME; }
	public float LifePart { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Life / MAX_LIFE; }
	public float FocusPercent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => FocusPart * 100.0f; }
	public float LifePercent { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => LifePart * 100.0f; }

	public bool IsJump { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Control.IsJumpPressed; }
	public bool IsKick { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Control.IsKickPressed; }

	public bool IsGrabbed { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Grab_IsGrabbed; }

	public PhysicsBody GrabbedBody { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Grab_GetGrabbedBody(); }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ReleaseGrab() => Grab_ReleaseGrab();

	public IPersonControl Control { [MethodImpl(MethodImplOptions.AggressiveInlining)] get; set; } = null;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void EnableControl(bool value)
	{
		if (Control != null)
			Control.Enabled = value;
	}

	public Vector2 ControlScreenDirection { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => Control.GetAxis1(); }

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public APerson GetRival() => _type.GetRival(this);

	public bool IsPlayer { [MethodImpl(MethodImplOptions.AggressiveInlining)] get => this == SceneManager?.Player; }

	public string Debug;

	public ATrigger ActiveTrigger
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get;
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private set;
	} = null;

	public void SetBAction(ATrigger trigger, bool enable)
	{
		if (ActiveTrigger == trigger)
		{
			if (!enable && (ActiveTrigger.IsActionFinished() || _FSM.ActiveActionKey != EGMAction.B_ACTION))
				trigger = null;
			else
				return;
		}
		ActiveTrigger = trigger;
		SceneManager.UI.SetBAction(trigger);
	} // SetBAction

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Finish()
	{
		_kickTrail.Finish();
	}

	public bool IsMeshVisible
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => _mesh.Visible;
		set => _mesh.Visible = value;
	}

	public bool IsInitialized
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => SceneManager != null;
	}
} // APerson
