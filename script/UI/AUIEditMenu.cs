using Godot;
using System.Linq;
using System.Threading.Tasks;

public class AUIEditMenu: ColorRect
{
	const string DETAILS_PATH = "res://scene/details/";
	const string DETAILS_VAR_PATH = "res://scene/details/var/";

	AUIObjectPanel _objectPanel;
	AUIItemScroll _actionsItemScroll;
	AUIItemScroll _addItemScroll;
	AUIItemScroll _materialItemScroll;

	LineEdit _filePath;
	AcceptDialog _acceptDialog;

	LineEdit _nodeName;

	AScenePlay _scene => A.App.SceneManager as AScenePlay;
	//AUIGameplayART _ui => A.App.UI.Gameplay as AUIGameplayART;

	public override void _Ready()
	{
		_objectPanel = this.FirstChild<AUIObjectPanel>(true);
		_actionsItemScroll = this.FirstChild<AUIItemScroll>(true, "actions_item_scroll");
		_addItemScroll = this.FirstChild<AUIItemScroll>(true, "add_item_scroll");
		_materialItemScroll = this.FirstChild<AUIItemScroll>(true, "material_item_scroll");

		_objectPanel.OnPressed = _object_panel_pressed;

		_filePath = this.FirstChild<LineEdit>(true, "file_path");
		if (string.IsNullOrWhiteSpace(_filePath.Text))
			_filePath.Text = A.Config.DetailsFilePath;
		if (string.IsNullOrWhiteSpace(_filePath.Text))
			_filePath.Text = System.IO.Path.Combine(OS.GetSystemDir(OS.SystemDir.Documents), "details.tscn");

		_acceptDialog = this.FirstChild<AcceptDialog>(true, "accept_dialog");

		_nodeName = this.FirstChild<LineEdit>(true, "node_name");

		// details
		Xts.ForeachFile(DETAILS_PATH, file =>
		{
			if (file.EndsWith(".tscn"))
			{
                var b = new AUIActionButton
                {
                    Name = file,
                    HintTooltip = file,
                    Text = System.IO.Path.GetFileNameWithoutExtension(file),
                    OnPressed = _on_add_pressed
                };
                _addItemScroll.Add(b);
			}
		});

		// materials
		Xts.ForeachFile(DETAILS_PATH, f => MaterialAdd(DETAILS_PATH, f));
		Xts.ForeachFile(DETAILS_VAR_PATH, f => MaterialAdd(DETAILS_VAR_PATH, f));

		// Actions
		foreach (var a in _scene.Editor.Actions)
			_actionsItemScroll.Add(new AUIActionButton()
			{
				Text = a.Key,
				OnPressed = a.Value
			});
	} // _Ready

	void MaterialAdd(string path, string file)
	{
		var fn = _scene.Editor?.SelectedObject?.Root?.Filename;
		if (string.IsNullOrEmpty(fn))
			return;
		fn = System.IO.Path.GetFileNameWithoutExtension(fn);
		var io = fn.IndexOf('_');
		if (io >= 0)
			fn = fn.Left(io);

		if (file.StartsWith(fn))
		{
            var b = new AUIActionButton
            {
                Name = file,
                HintTooltip = path + file,
                Text = System.IO.Path.GetFileNameWithoutExtension(file),
                OnPressed = _on_material_pressed
            };
            _materialItemScroll.Add(b);
		}
	} // MaterialAdd

	bool _selectPressed = false;

	public override void _Process(float delta)
	{
		if (Input.IsActionJustPressed("select"))
		{
			_selectPressed = true;
		}
		else if (_selectPressed && Input.IsActionJustReleased("select"))
		{
			_scene.Resume();
			SetProcess(false);
		}
		else if (Input.IsPhysicalKeyPressed((int)KeyList.A))
		{
			_on_btn_play_pressed();
			SetProcess(false);
		}
		else if (Input.IsPhysicalKeyPressed((int)KeyList.Q))
		{
			_object_panel_pressed(AEditor.EMode.CURSOR);
			SetProcess(false);
		}
		else if (Input.IsPhysicalKeyPressed((int)KeyList.W))
		{
			_object_panel_pressed(AEditor.EMode.MOVE);
			SetProcess(false);
		}
		else if (Input.IsPhysicalKeyPressed((int)KeyList.E))
		{
			_object_panel_pressed(AEditor.EMode.ROTATE);
			SetProcess(false);
		}
		else if (Input.IsPhysicalKeyPressed((int)KeyList.R))
		{
			_object_panel_pressed(AEditor.EMode.SCALE);
			SetProcess(false);
		}
		else if (Input.IsPhysicalKeyPressed((int)KeyList.S))
		{
			_object_panel_pressed(AEditor.EMode.SELECT);
			SetProcess(false);
		}
	} // _Process

	void _on_btn_play_pressed()
	{
		_scene.SetEditor(false);
		_scene.Resume();
	}

	async void _on_btn_save_pressed()
	{
		await GetFilePermissionsAsync();
		if (_hasStoragePermission)
		{
			var res = _scene.Editor.Save(_filePath.Text);
			A.Config.DetailsFilePath = _filePath.Text;
			A.Config.Save();
			_acceptDialog.DialogText = $"save result: {res}";
		}
		else
			_acceptDialog.DialogText = "no storage permission";

		_acceptDialog.WindowTitle = "save";
		_acceptDialog.Popup_();
	} // _on_btn_save_pressed

	async void _on_btn_load_pressed()
	{
		await GetFilePermissionsAsync();
		if (_hasStoragePermission)
		{
			var res = _scene.Editor.Load(_filePath.Text);
			A.Config.DetailsFilePath = _filePath.Text;
			A.Config.Save();
			_acceptDialog.DialogText = $"load result: {res}";
		}
		else
			_acceptDialog.DialogText = "no storage permission";

		_acceptDialog.WindowTitle = "load";
		_acceptDialog.Popup_();
	} // _on_btn_load_pressed

	void _on_btn_file_select_pressed()
	{
		AUIFileDialog.Show(file => _filePath.Text = file);
	}

	bool _hasStoragePermission = false;
	async Task GetFilePermissionsAsync()
	{
		if (_hasStoragePermission)
			return;

		// Returns the Name of the host OS. Possible values are: "Android", "iOS", "HTML5",
		//     "OSX", "Server", "Windows", "UWP", "X11".
		switch (OS.GetName())
		{
			case "Android":
				for (int i = 0; i < 2; ++i)
				{
					var permissions = OS.GetGrantedPermissions();
					if (!permissions.Contains("android.permission.READ_EXTERNAL_STORAGE")
						|| !permissions.Contains("android.permission.WRITE_EXTERNAL_STORAGE")
						)
					{
						OS.RequestPermissions();
						await ToSignal(GetTree().CreateTimer(1), "timeout");
					}
					else
					{
						_hasStoragePermission = true;
						break;
					}
				} // for
				break;
			default:
				_hasStoragePermission = true;
				break;
		} // switch
	} // GetFilePermissionsAsync

	void _object_panel_pressed(AEditor.EMode value)
	{
		_scene.SetEditor(true);
		_scene.Editor.Mode = value;
		_nodeName.Text = _scene.Editor.SelectedObject?.Root?.Name;
		_scene.Resume();
	} // _object_panel_pressed

	void _on_add_pressed(AUIActionButton button)
	{
		_scene.SetEditor(true);
		_scene.Editor.AddDetail(string.Concat(DETAILS_PATH, button.HintTooltip));
		_scene.Resume();
	}

	void _on_material_pressed(AUIActionButton button)
	{
		var s = _scene.Editor.SelectedObject?.Root;
		if (s != null)
		{
			_scene.SetEditor(true);

			var t = s.Transform;
			_scene.Editor.DeleteSelected(null);

			var n = _scene.Editor.AddDetail(button.HintTooltip);
			n.Transform = t;
			_scene.Resume();
		}
	}

	void _on_node_name_text_changed(string new_text)
	{
		var node = _scene.Editor.SelectedObject?.Root;
		if (node != null)
			node.Name = new_text;
	}
} // AUIEditMenu
