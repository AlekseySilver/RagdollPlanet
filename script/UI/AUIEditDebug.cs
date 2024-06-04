using Godot;
using System.Collections.Generic;
using System.Reflection;

public class AUIEditDebug: Control
{
    VBoxContainer _box;
    LineEdit _filter;

    class AEdit: HBoxContainer
    {
        AUIEditDebug _parent;
        PropertyInfo _prop = null;
        FieldInfo _field = null;
        System.Type _type = null;
        Label _label;
        LineEdit _edit;

        public void Init(AUIEditDebug parent, MemberInfo prop, System.Type type)
        {
            _prop = prop as PropertyInfo;
            _field = prop as FieldInfo;
            _type = type;
            _parent = parent;

            parent._box.AddChild(this);

            _label = new Label();
            AddChild(_label);

            _edit = new LineEdit();
            AddChild(_edit);
            _edit.Connect("text_changed", this, "text_changed");

            if (_prop != null)
            {
                _label.Text = _prop.Name;
                _edit.Text = _prop.GetValue(parent._obj)?.ToString();
            }
            else if (_field != null)
            {
                _label.Text = _field.Name;
                _edit.Text = _field.GetValue(parent._obj)?.ToString();
            }
        }

        public void Remove()
        {
            _edit.Disconnect("text_changed", this, "text_changed");
            RemoveChild(_edit);
            RemoveChild(_label);
            _parent._box.RemoveChild(this);
            _edit.QueueFree();
            _label.QueueFree();
            QueueFree();
        }

        void text_changed(string new_text)
        {
            if (_type == typeof(float))
            {
                if (float.TryParse(new_text, out float f))
                {
                    _prop?.SetValue(_parent._obj, f);
                    _field?.SetValue(_parent._obj, f);
                }
            }
            else if (_type == typeof(int))
            {
                if (int.TryParse(new_text, out int i))
                {
                    _prop?.SetValue(_parent._obj, i);
                    _field?.SetValue(_parent._obj, i);
                }
            }
            else if (_type == typeof(string))
            {
                _prop?.SetValue(_parent._obj, new_text);
                _field?.SetValue(_parent._obj, new_text);
            }
        }
    } // AEdit

    readonly List<AEdit> _edits = new List<AEdit>();

    public override void _Ready()
    {
        _box = this.FirstChild<VBoxContainer>(true);
        _filter = this.FirstChild<LineEdit>(true);

    }

    object _obj = null;
    bool _enabled = false;

    public bool Enabled
    {
        get => _enabled;
        set
        {
            _enabled = value;
            if (!_enabled)
                SetObject(null);
        }
    }

    public void SetObject(object obj)
    {
        _obj = obj;
        Refresh();
    } // SetObject

    void _on_filter_text_changed(string new_text)
    {
        Refresh();
    } // _on_filter_text_changed

    void Refresh()
    {
        Visible = false;
        if (!Enabled)
            return;

        foreach (var e in _edits)
            e.Remove();
        _edits.Clear();

        if (_obj == null)
            return;

        Visible = true;

        string filter = _filter.Text.Trim();

        var props = _obj.GetType().GetProperties();
        foreach (var p in props)
            CheckAdd(p, p.PropertyType, filter);

        var fields = _obj.GetType().GetFields();
        foreach (var f in fields)
            CheckAdd(f, f.FieldType, filter);
    } // Refresh

    void CheckAdd(MemberInfo mi, System.Type type, string filter)
    {
        if (!mi.IsDefined(typeof(ExportAttribute)))
            return;
        if (!Xts.IsIn(type, typeof(int), typeof(float), typeof(string)))
            return;
        if (filter.Length > 0 && !mi.Name.Contains(filter))
            return;

        var e = new AEdit();
        e.Init(this, mi, type);
        _edits.Add(e);
    }
}
