using Godot;
using System.Collections.Generic;

public partial class AEditor
{
    const float ROTATE_RATE = 0.01f;
    const float SCALE_RATE = 0.01f;

    public const string EDIT_ROOT = "edit_root";

    public enum EMode
    {
        CURSOR = 0, SELECT, MOVE, ROTATE, SCALE
    }
    EMode _mode = EMode.CURSOR;
    public EMode Mode
    {
        get => _mode;
        set
        {
            if (_mode == value)
                return;
            _mode = value;
            StartEdit();
            switch (value)
            {
                case AEditor.EMode.SELECT:
                    var list = Select();

                    if (list.Count == 0)
                    {
                        Mode = AEditor.EMode.CURSOR;
                        Scene.Resume();
                    }
                    else if (list.Count == 1)
                    {
                        SelectedObject = list[0];
                        Mode = AEditor.EMode.MOVE;
                        Scene.Resume();
                    }
                    else
                    {
                        Scene.UI.ShowMenu_SelectItem();
                        var sm = Scene.UI.Menu as AUISelectItem;
                        sm.OnResult = _on_item_scroll_result;
                        foreach (var o in list)
                            sm.Add(o.Root.Name, o);
                    }
                    break;
                default:
                    Scene.Resume();
                    break;
            } // switch		
        }
    } // Mode

    public Spatial Cursor { get; private set; } = null;

    AScenePlay Scene => A.App.SceneManager as AScenePlay;

    public AGamepadPlay Control { get; set; } = null;

    readonly List<AEditObjectBase> _editObjects = new List<AEditObjectBase>();

    // for a quick search by CollisionObject
    readonly Dictionary<CollisionObject, AEditObjectBase> _idxCollisions = new Dictionary<CollisionObject, AEditObjectBase>();

    Material _selectedMaterial = null;

    AEditObjectBase _selectedObject = null;
    Vector3 _selectedOffset;
    public AEditObjectBase SelectedObject
    {
        get => _selectedObject;
        set
        {
            if (_selectedObject == value)
                return;
            //_selectedObject?.OutlineRemove();
            _selectedObject = value;
            if (_selectedObject?.Root != null && Cursor != null)
                _selectedOffset = Cursor.GetGlobalPos() - SelectedObject.Root.GetGlobalPos();
            else
                _selectedOffset = Vector3.Zero;
            //_selectedObject?.OutlineAdd();
            StartEdit();
        }
    }

    Spatial _editRoot = null;
    public Spatial EditRoot
    {
        get
        {
            if (_editRoot == null)
            {
                _editRoot = Scene.FirstChild<Spatial>(false, EDIT_ROOT);
                if (_editRoot == null)
                {
                    _editRoot = new Spatial() { Name = EDIT_ROOT };
                    Scene.AddChild(_editRoot);
                }
                else
                    FillEditObjects();
            }
            return _editRoot;
        }
    } // _editRoot

    public AEditor()
    {
        InitActions();
    }

    public bool Enabled
    {
        get => Cursor != null;
        set
        {
            if (Enabled == value)
                return;
            if (value)
                StartEdit();
            else
            {
                _idxCollisions.Clear();
                Cursor.GetParent().RemoveChild(Cursor);
                Cursor.QueueFree();
                Cursor = null;
                foreach (var o in _editObjects)
                    o.SetPlayMode();
            }
        }
    } // Enabled

    void StartEdit()
    {
        if (Cursor == null)
        {
            Cursor = Asset.Instantiate2Scene<Spatial>(Scene.cursor_resource);
            Cursor.SetGlobalPos(Scene.Camera.LastTargetPosition);
            foreach (var o in _editObjects)
            {
                o.SetEditMode();
                _idxCollisions.Add(o.CollisionObject, o);
            }
        }

        if (SelectedObject == null)
            Mode = EMode.CURSOR;

        if (Mode == EMode.CURSOR)
        {
            Cursor.Visible = true;

            if (SelectedObject != null)
                Cursor.SetGlobalPos(SelectedObject.Root.GetGlobalPos());

            Scene.Camera.AssignTarget(Cursor);
        }
        else
        {
            Cursor.Visible = false;
            Scene.Camera.AssignTarget(SelectedObject.Root, _selectedOffset);
        }

        A.App.UI.SetDebug2(Mode.ToString());
    } // StartEdit

    public void Update()
    {
        if (!Enabled)
            return;

        if (Control != null)
        {
            Control.UpdateOverride();

            var axis = Control.GetAxis1();
            var vert = Control.GetVertical();

            if (axis != Vector2.Zero || vert != 0.0f)
            {

                var dir = Scene.Camera.DirectPad2World(axis);

                //const float CAM_ROT_SPEED = 250f;
                const float CAM_MOVE_SPEED = 40f;

                var offset = (dir + Scene.Camera.GlobalUp * vert) * (CAM_MOVE_SPEED * A.TimeStep);
                var sel = SelectedObject?.Root;

                float magn = 1f;
                if (Control.IsKickPressed)
                    magn = 0.2f;
                if (Control.IsJumpPressed)
                    magn = 10.0f;

                if (Mode == EMode.CURSOR)
                {
                    Cursor.GlobalTranslate(offset * magn);
                }
                else if (sel != null)
                {
                    switch (Mode)
                    {
                        case EMode.MOVE:
                            sel.GlobalTranslate(offset * magn);
                            break;
                        case EMode.ROTATE:
                            if (Control.IsJumpPressed)
                                magn = 5.0f;
                            var addRotate = axis.XAY(vert) * (ROTATE_RATE * magn);
                            sel.BasisXformLocal(addRotate, Vector3.Zero);
                            break;
                        case EMode.SCALE:
                            if (Control.IsJumpPressed)
                                magn = 100.0f;
                            var addScale = axis.XAY(vert) * (SCALE_RATE * magn);
                            SelectedObject.AddScaleLocal(addScale);
                            break;
                    } // switch
                } // else if (sel != null)

            } // if zero

            if (Input.IsActionJustPressed("editor_prev"))
            {
                Mode = Mode == EMode.CURSOR ? EMode.SCALE : Mode == EMode.MOVE ? EMode.CURSOR : Mode - 1;
            }
            else if (Input.IsActionJustPressed("editor_next"))
            {
                Mode = Mode == EMode.SCALE ? EMode.CURSOR : Mode + 1;
            }

            Scene.debug0 = Mode.ToString();
        } // if Control
    } // UpdateOverride

    public Spatial AddDetail(string assetPath)
    {
        var s = Asset.Instantiate<Spatial>(assetPath, EditRoot);

        s.SetGlobalPos(Cursor.GetGlobalPos());
        s.UpAlign();

        AddDetail(s);
        return s;
    } // AddDetail


    void AddDetail(Spatial s)
    {
        var o = AEditObjectBase.NewInstance(s);
        if (o != null)
        {
            s.Owner = EditRoot;
            _editObjects.Add(o);
            _idxCollisions.Add(o.CollisionObject, o);
            SelectedObject = o;
            Mode = EMode.MOVE;
        }
    }

    void FillEditObjects()
    {
        EditRoot.ForeachChild<Spatial>(false, x =>
        {
            AddDetail(x);
            return false;
        });
    }

    public List<AEditObjectBase> Select()
    {
        var list = new List<AEditObjectBase>();

        var sh = new SphereShape
        {
            Radius = Cursor.Scale.x
        };

        var res = Scene.GetWorld().DirectSpaceState.IntersectShape(new PhysicsShapeQueryParameters
        {
            ShapeRid = sh.GetRid(),
            CollideWithAreas = true,
            CollideWithBodies = true,
            Transform = Cursor.Transform
        }, maxResults: 32);

        for (int i = 0; i < res.Count; ++i)
        {
            var d = res[i] as Godot.Collections.Dictionary;
            var col = d["collider"] as CollisionObject;
            if (_idxCollisions.TryGetValue(col, out var obj))
            {
                list.Add(obj);
                GetMaterialFromObject();
            }
        }

        return list;
    } // Select

    public string Save(string fileName)
    {
        var ps = new PackedScene();
        ps.Pack(EditRoot);
        return ResourceSaver.Save(fileName, ps).ToString();
    }

    public string Load(string fileName)
    {
        if (!System.IO.File.Exists(fileName))
            return "no file exists";

        _editRoot?.QueueFree();
        _editRoot = null;

        var ps = (PackedScene)ResourceLoader.Load(fileName);
        _editRoot = ps.Instance<Spatial>();
        _editRoot.Name = EDIT_ROOT;
        Scene.AddChild(_editRoot);

        FillEditObjects();
        SelectedObject = null;

        return "Ok";
    } // Load

    public void GetMaterialFromObject()
    {
        var mi = SelectedObject?.Root?.FirstChild<MeshInstance>(true);
        if (mi != null)
            _selectedMaterial = mi.GetSurfaceMaterial(0);
    }

    void _on_item_scroll_result(object value)
    {
        if (value is AEditObjectBase e)
        {
            SelectedObject = e;
            Mode = AEditor.EMode.MOVE;
        }
        else
        {
            Mode = AEditor.EMode.CURSOR;
        }
        Scene.Resume();
    } // _on_item_scroll_result
} // AEditor
