using Godot;
using System;
using System.Linq;

[Tool]
public class ANodeLoader: Node, IUpdateable
{
	#region tuple
	class ATuple
	{
		public Spatial Spatial;
		public Vector3 Normal;
	}

	#endregion

	public enum EState { NONE = 0, COLLAPCE, EXPAND, DEEP_COLLAPCE, DEEP_EXPAND }
	public enum ECheckType { NONE = 0, POSITION, DIRECTION }

	EState _state = EState.NONE;

	[Export(PropertyHint.Enum)]
	public EState State
	{
		get => _state;
		set
		{
			ChangeState(value);
			_state = value;
		}
	}

	[Export(PropertyHint.Enum)] public ECheckType check_type = ECheckType.NONE;

	[Export] public float load_value;
	[Export] public float unload_value;

	[Export] public string[] files;
	[Export] public Vector3[] positions;
	[Export] public Vector3[] rotations;
	[Export] public Vector3[] scales;

	ATuple[] _loaded = null;
	int _loadingId;
	int _checkId = -1;

	AScenePlay Scene => A.App.SceneManager as AScenePlay;

	void ChangeState(EState newState)
	{
		if (Engine.EditorHint == false)
			return;
		var parent = GetParent();
		if (parent == null)
			return;
		switch (newState)
		{
			case EState.COLLAPCE:
				var count = parent.GetChildCount();
				var list = new System.Collections.Generic.List<Spatial>(count);
				for (int i = 0; i < count; ++i)
				{
					var sp = parent.GetChildOrNull<Spatial>(i);
					if (sp != null)
					{
						if (string.IsNullOrEmpty(sp.Filename) == false)
						{
							list.Add(sp);
						}
					}
				}
				count = list.Count;
				if (count > 0)
				{
					files = new string[count];
					positions = new Vector3[count];
					rotations = new Vector3[count];
					scales = new Vector3[count];
					for (int i = 0; i < count; ++i)
					{
						var sp = list[i];
						var xf = sp.Transform;
						files[i] = sp.Filename;
						positions[i] = xf.origin;
						rotations[i] = xf.basis.GetEuler();
						scales[i] = xf.basis.Scale;
						sp.QueueFree();
					}
				}
				list.Clear();
				break;
			case EState.EXPAND:
				for (int i = 0; i < files.Length; ++i)
				{
					Load(i);
				}
				break;
			case EState.DEEP_COLLAPCE:
				//todo
				break;
			case EState.DEEP_EXPAND:
				//todo
				break;
		}
		PropertyListChangedNotify();
	} // ChangeState

	public void load_start()
	{
		_loadingId = 0;
	}
	public bool load_next()
	{
		if (_loadingId >= files.Length)
			return false;
		if (Load(_loadingId) == null)
			return false;
		return _loadingId >= files.Length;
	} // load_next

	Spatial Load(int id)
	{
		var file = files[id];
		//if (Engine.EditorHint) GD.Print(file);
		var res = ResourceLoader.Load<PackedScene>(file);
		if (res.CanInstance() == false)
			return null;
		var sp = res.Instance<Spatial>();
        var b = new Basis(rotations[id])
        {
            Scale = scales[id]
        };
        sp.Transform = new Transform(b, positions[id]);
		var parent = GetParent();
		parent.AddChild(sp);
		sp.Owner = parent.Owner ?? parent;
		return sp;
	} // Load

	public override void _EnterTree()
	{
		if (files != null)
		{
			_loaded = new ATuple[files.Length];
			for (int i = 0; i < positions.Length; ++i)
			{
				_loaded[i] = new ATuple { Normal = positions[i].Normalized() };
			}
		}
		var s = Scene;
		if (s != null)
			s.Add4Lazy(this);
		else
			A.App.DoOnce(() => Scene.Add4Lazy(this));
	} // _Ready

	public override void _ExitTree()
	{
		Scene.Remove4Lazy(this);
		_loaded = null;
	} // _ExitTree

	public void UpdateOverride()
	{
		if (_loaded == null)
			return;

		++_checkId;
		if (_checkId >= _loaded.Length)
			_checkId = 0;

		var tuple = _loaded[_checkId];

		switch (check_type)
		{
			case ECheckType.DIRECTION:
				var cos = A.App.SceneManager.GetCamera().GlobalUp.Dot(tuple.Normal);
				if (tuple.Spatial == null)
				{
					// check Load
					if (cos > load_value)
					{
						tuple.Spatial = Load(_checkId);
					}
				}
				else
				{
					// check unload
					if (cos < unload_value)
					{
						tuple.Spatial.QueueFree();
						tuple.Spatial = null;
					}
				}
				break;
			case ECheckType.POSITION:
				var dist = A.App.SceneManager.GetCamera().Transform.origin.DistanceSquaredTo(positions[_checkId]);
				if (tuple.Spatial == null)
				{
					// check Load
					if (dist < load_value)
					{
						tuple.Spatial = Load(_checkId);
					}
				}
				else
				{
					// check unload
					if (dist > unload_value)
					{
						tuple.Spatial.QueueFree();
						tuple.Spatial = null;
					}
				}
				break;
			default:
				Load(_checkId);
				if (_checkId == _loaded.Length - 1)
					Scene.Remove4Lazy(this);
				break;
		}
	} // UpdateOverride
}
