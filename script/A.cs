using Godot;
using System;

public sealed class A: Control, IUpdateHolder
{
	/// <summary>
	/// application singleton
	/// </summary>
	public static A App { get; private set; }
	public static float TimeStep { get; private set; } = 0.0f;
	public static float FixedStep { get; private set; } = 0.0f;

	public AScene SceneManager { get; private set; }
	public AUICommon UI { get; private set; }

	public AUpdateCase UPD { get; } = new AUpdateCase();

	public Node SceneRoot => GetTree().CurrentScene;

	[Export]
	public float ViewportScaleFactor
	{
		get => _viewportScaleFactor;
		set
		{
			_viewportScaleFactor = value;
			RootViewportSizeChanged();
		}
	}
	float _viewportScaleFactor = 1.0f;

	readonly AConfig _config = new AConfig();
	public static AConfig Config => App._config;

	public A()
	{
		App = this;
	}

	// Called when the node enters the Scene tree for the first Time.
	public override async void _Ready()
	{
		_config.Load();

		var lang = _config.Lang;
		if (!string.IsNullOrWhiteSpace(lang))
			TranslationServer.SetLocale(lang);

		//# Required to change the 3D viewport'S size when the window is resized.
		//#warning-ignore:return_value_discarded
		GetViewport().Connect("size_changed", this, "RootViewportSizeChanged");
		RootViewportSizeChanged();

		UI = this.FirstChild<AUICommon>(true);
		UI.Init();
		SceneManager = this.FirstChild<AScene>(true);
		await SceneManager.InitAsync();

		UPD.Add4Update(UI);
		UPD.Add4Update(SceneManager);
		UPD.Add4Fixed(SceneManager);
	} // _Ready

	//  // Called every frame. 'delta' is the elapsed Time since the previous frame.
	public override void _Process(float delta)
	{
		TimeStep = delta;
		UPD.Update();
	}
	public override void _PhysicsProcess(float delta)
	{
		FixedStep = delta;
		UPD.FixedUpdate();
	}

	public override void _Notification(int what)
	{
		base._Notification(what);
		switch (what)
		{
			case MainLoop.NotificationWmFocusOut:
				SceneManager?.Pause();
				break;
		}
	}

	void RootViewportSizeChanged()
	{
		// The viewport is resized depending on the window height.
		// To compensate for the larger resolution, the viewport sprite is scaled down.
		var vp = this.FirstChild<Viewport>();
		if (vp != null)
			vp.Size = GetViewport().Size * ViewportScaleFactor;
	}

	public static Vector2 UISize => App.UI.RectSize;

	public void DoOnce(Action action)
	{
		App.UPD.Add4Update(new AUpdateOnce(action));
	}
	struct AUpdateOnce: IUpdateable
	{
		readonly Action _action;
		public AUpdateOnce(Action action)
		{
			_action = action;
		}
		public void UpdateOverride()
		{
			App.UPD.Rem4Update(this);
			_action?.Invoke();
		}
	}
} // class A
