using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class AUIAnimEdit: Control
{
    APerson _person;
    AAnimation _anim;
    MeshInstance _debugArrow = null;

    AUIItemScroll _keys;
    Tree _tree;
    ABone _selectedBone;

    SpinBox _currentTime;
    SpinBox _totalTime;
    SpinBox _overdrive;
    Slider _currentSlider;

    CheckButton _checkLoop;

    Button _btnPlay;
    LineEdit _name;

    bool _play = false;

    int _currentTimeNestLevel = 0;

    public void ShowArrow(Vector3 pos, Vector3 dir)
    {
        if (_debugArrow == null)
            _debugArrow = Asset.Instantiate2Scene<MeshInstance>(Asset.debug_arrow);
        _debugArrow.GlobalTransform = new Transform(Xts.UpAlign(Basis.Identity, dir), pos);
    }
    public void HideArrow()
    {
        if (_debugArrow != null)
        {
            _debugArrow.QueueFree();
            _debugArrow = null;
        }
    }

    public APerson Person
    {
        get => _person;
        set
        {
            _person = value;
            _tree.Clear();

            if (_person == null)
            {
                Visible = false;
            }
            else
            {
                var root = _tree.CreateItem();
                root.SetText(0, _person.Body.Name);
                AddTreeItem(_person.Body, root);
                Visible = true;
                if (_anim == null)
                    _anim = AAnimation.Create();

                _totalTime.Value = _anim.TotalTime;
                _overdrive.Value = _anim.Overdrive;
                _checkLoop.Pressed = _anim.IsLoop;
                _name.Text = _anim.Name;
            }
        }
    }

    void AddTreeItem(ABone parent_bone, TreeItem parent_item)
    {
        foreach (var b in _person.Bones)
        {
            if (b.DirectParent == parent_bone)
            {
                var item = _tree.CreateItem(parent_item);
                item.SetText(0, b.Name);
                AddTreeItem(b, item);
            }
        }
    }

    public override void _Ready()
    {
        _keys = this.FirstChild<AUIItemScroll>(false, "keys");
        _tree = this.FirstChild<Tree>(false, "tree");

        _currentTime = this.FirstChild<SpinBox>(false, "current_time");
        _totalTime = this.FirstChild<SpinBox>(false, "total_time");
        _overdrive = this.FirstChild<SpinBox>(false, "overdrive");
        _currentSlider = this.FirstChild<Slider>(false, "current_slider");

        _checkLoop = this.FirstChild<CheckButton>(false, "chk_loop");

        _btnPlay = this.FirstChild<Button>(false, "btn_play");

        _name = this.FirstChild<LineEdit>(false, "name");
    } // _Ready

    public override void _Process(float delta)
    {
        if (_play)
        {
            _anim.Update(delta);
            CurrentTime = _anim.CurrentTime;
        }
    } // _Process

    void _on_tree_item_selected()
    {
        _selectedBone = null;
        var name = _tree.GetSelected()?.GetText(0);

        if (_person.Body.Name == name)
        {
            _selectedBone = _person.Body;
            ShowKeys();
        }
        else
        {
            foreach (var b in _person.Bones)
            {
                if (b.Name == name)
                {
                    _selectedBone = b;
                    ShowKeys();
                    break;
                }
            }
        }
    } // _on_tree_item_selected

    void ShowKeys()
    {
        _keys.Label = $"keys: {_selectedBone?.Name}";
        _keys.Clear();

        var b = _anim.GetBone(_selectedBone);
        if (b == null)
            return;

        for (int i = 0; i < b.Keys.Length; ++i)
        {
            var e = Asset.Instantiate<AUIKeyEditElement>(Asset.key_edit_element);
            _keys.Add(e);

            var k = b.Keys[i];
            e.KeyId = i;
            e.Time = k.Time;
            e.Rate = k.Rate;
            e.type = (int)k.Type;

            if (k.Type == AAnimKey.EType.DIRECT && b.Bone is ABone1 b1)
            {
                var euler = b1.ParentDirectTarget2Euler(k.Value);
                e.V3 = euler;
            }
            else
                e.V3 = k.Value;

            e.CallbackChanged = OnKeyChanged;
        }
    } // ShowKeys

    void OnKeyChanged(AUIKeyEditElement e, int state)
    {
        var bone = _anim.GetBone(_selectedBone);
        if (bone == null)
            return;

        switch (state)
        {
            case 0: // UpdateOverride
                var type = AAnimKey.EType.DIRECT;
                if (e.type >= 0)
                    type = (AAnimKey.EType)e.type;

                var value = e.V3;
                if (type == AAnimKey.EType.DIRECT)
                {
                    if (bone.Bone is ABone1 bone1)
                    {
                        value = bone1.Euler2ParentDirectTarget(value);
                    }
                }

                bone.Keys[e.KeyId] = new AAnimKey
                {
                    Time = e.Time,
                    Rate = e.Rate,
                    Value = value,
                    Type = type
                };
                if (bone.SortKeys())
                    ShowKeys();
                break;
            case 1: // Remove
                bone.RemoveKey(e.KeyId);
                ShowKeys();
                break;
            case 2: // Fire
                var key = bone.Keys[e.KeyId];
                bone.ApplyKey(key);
                if (bone.Bone is ABone1 b11)
                {
                    ShowArrow(bone.Bone.Position, b11.DirectParentTargetWorld);
                }
                break;
            case 3: // Interpolate
                if (Interpolate(bone, e.KeyId))
                    ShowKeys();
                break;
        } // switch
    } // OnKeyChanged

    public float CurrentTime
    {
        get => _anim.CurrentTime;
        set
        {
            ++_currentTimeNestLevel;
            if (_currentTimeNestLevel == 1)
            {
                if (CurrentTime != value)
                    _anim.GotoTime(value);
                if (_currentSlider.Value != value)
                    _currentSlider.Value = value;
                if (_currentTime.Value != value)
                    _currentTime.Value = value;
            }
            --_currentTimeNestLevel;
        }
    }

    void _on_btn_add_anim_pressed()
    {
    }
    void _on_btn_add_set_pressed()
    {
        if (_selectedBone == null)
            return;
        _anim.AddBone(_selectedBone);
    }
    void _on_btn_add_key_pressed()
    {
        if (_selectedBone == null)
            return;
        var s = _anim.GetBone(_selectedBone);
        if (s == null)
            return;

        s.AddKey(
            new AAnimKey
            {
                Rate = 1f,
                Type = AAnimKey.EType.DIRECT,
                Time = CurrentTime,
                Value = Vector3.Zero,
            }
        );
        ShowKeys();
    }

    void _on_btn_rem_set_pressed()
    {
    }

    void _on_btn_play_pressed()
    {
        _play = !_play;
        if (_play)
        {
            _btnPlay.Text = "stop";
        }
        else
        {
            _btnPlay.Text = "play";
        }
    }

    void _on_total_time_value_changed(float value)
    {
        _currentSlider.MaxValue = value;
        _currentTime.MaxValue = value;

        if (_anim.TotalTime != value)
            _anim.TotalTime = value;
    }
    void _on_overdrive_value_changed(float value)
    {
        if (_anim.Overdrive != value)
            _anim.Overdrive = value;
    }

    void _on_current_time_value_changed(float value)
    {
        CurrentTime = value;
    }

    void _on_current_slider_value_changed(float value)
    {
        CurrentTime = value;
    }
    void _on_chk_loop_toggled(bool value)
    {
        if (_anim.IsLoop != value)
            _anim.IsLoop = value;
    }
    void _on_name_text_changed(string value)
    {
        if (_anim.Name != value)
            _anim.Name = value;
    }

    void _on_btn_finish_pressed()
    {
        var ui = A.App.UI.FirstChild<AUIAnimEdit>(true);
        if (ui?.Person != null)
        {
            ui.Person.DoAction(APerson.EGMAction.RELAX);
            ui.Person = null;
        }
        (A.App.SceneManager as AScenePlay)?.Resume();
    }

    void _on_btn_save_pressed()
    {
        if (string.IsNullOrWhiteSpace(_name.Text))
        {
            AUIFileDialog.Show(file => _name.Text = file);
        }

        if (string.IsNullOrWhiteSpace(_name.Text) == false)
        {
            _anim.Save(_name.Text);
        }
    }
    void _on_btn_load_pressed()
    {
        if (string.IsNullOrWhiteSpace(_name.Text))
        {
            AUIFileDialog.Show(file => _name.Text = file);
        }

        if (!string.IsNullOrWhiteSpace(_name.Text))
        {
            _anim.Load(_person, _name.Text);
            Person = Person; // trigger fill
        }
    }

    void Relax(in Vector3 up, in Vector3 side)
    {
        HideArrow();
        _person.Relax();
        _person.Body.StartDirect(ABone3.EAxis.MAIN, up, 0.5f, ABone3.EAxis.FRONT, side, 0.5f);
    }

    void _on_btn_relax_L_pressed()
    {
        var up = _person.UpDirection;
        var side = up.Cross((A.App.SceneManager as AScenePlay).Camera.CurrentCameraDirection).Normalized();

        Relax(up, side);
    }
    void _on_btn_relax_R_pressed()
    {
        var up = _person.UpDirection;
        var side = -up.Cross((A.App.SceneManager as AScenePlay).Camera.CurrentCameraDirection).Normalized();

        Relax(up, side);
    }
    void _on_btn_relax_U_pressed()
    {
        var up = _person.UpDirection;
        var side = up.Cross((A.App.SceneManager as AScenePlay).Camera.CurrentCameraDirection).Normalized();
        side = up.Cross(side).Normalized();

        Relax(up, side);
    }
    void _on_btn_relax_D_pressed()
    {
        var up = _person.UpDirection;
        var side = up.Cross((A.App.SceneManager as AScenePlay).Camera.CurrentCameraDirection).Normalized();
        side = -up.Cross(side).Normalized();

        Relax(up, side);
    }

    void _on_btn_relax_pressed()
    {
        HideArrow();
        _person.Relax();
    }

    void _on_btn_interpolate_anim_pressed()
    {
        Interpolate();
    }

    void _on_btn_interpolate_bone_pressed()
    {
        var s = _anim.GetBone(_selectedBone);
        if (s != null)
            Interpolate(s);
    }

    bool Interpolate()
    {
        bool res = false;
        for (int i = 0; i < _anim.Bones.Length; ++i)
            res |= Interpolate(_anim.Bones[i]);
        return res;

    }
    bool Interpolate(AAnimBone b)
    {
        bool res = false;
        for (int i = 0; i < b.Keys.Length; ++i)
        {
            if (Interpolate(b, i))
            {
                ++i;
                res |= true;
            }
        }
        return res;
    }
    bool Interpolate(AAnimBone b, int keyId)
    {
        int next_key_id = keyId + 1;
        if (next_key_id >= b.Keys.Length)
            next_key_id = 0;

        var key1 = b.Keys[keyId];
        var key2 = b.Keys[next_key_id];

        if (key1.Type == key2.Type)
        {
            float t = key2.Time + key1.Time;
            if (key1.Time > key2.Time)
                t += _anim.TotalTime;
            t *= 0.5f;

            var v3 = Vector3.Zero;
            if (key1.Type == AAnimKey.EType.DIRECT)
            {
                v3 = new Basis(Quat.Identity.Slerp(Xts.FromRotationTo(key1.Value, key2.Value), 0.5f)).Xform(key1.Value);
            }
            else if (key1.Type == AAnimKey.EType.LIMIT)
            {
                v3 = Xts.Avg(key1.Value, key2.Value);
            }

            b.AddKey(
                new AAnimKey
                {
                    Rate = Xts.Avg(key1.Rate, key2.Rate),
                    Type = key1.Type,
                    Time = t,
                    Value = v3,
                }
            );
            return true;
        }
        return false;
    } // Interpolate
} // AUIAnimEdit
