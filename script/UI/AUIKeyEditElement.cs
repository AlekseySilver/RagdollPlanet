using Godot;

public class AUIKeyEditElement: Control
{
    SpinBox _spinTime;
    SpinBox _spinRate;
    SpinBox _spinX;
    SpinBox _spinY;
    SpinBox _spinZ;

    OptionButton _keyType;

    public System.Action<AUIKeyEditElement, int> CallbackChanged = null;

    public int KeyId = 0;

    public float Time
    {
        get => (float)_spinTime.Value;
        set
        {
            if (Mathf.Abs((float)_spinTime.Value - value) > _spinTime.Step)
                _spinTime.Value = value;
        }
    }

    public float Rate
    {
        get => (float)_spinRate.Value;
        set
        {
            if (Mathf.Abs((float)_spinRate.Value - value) > _spinRate.Step)
                _spinRate.Value = value;
        }
    }

    public Vector3 V3
    {
        get => new Vector3((float)_spinX.Value, (float)_spinY.Value, (float)_spinZ.Value);
        set
        {
            if (Mathf.Abs((float)_spinX.Value - value.x) > _spinX.Step)
                _spinX.Value = value.x;
            if (Mathf.Abs((float)_spinY.Value - value.y) > _spinY.Step)
                _spinY.Value = value.y;
            if (Mathf.Abs((float)_spinZ.Value - value.z) > _spinZ.Step)
                _spinZ.Value = value.z;
        }
    }

    public int type
    {
        get => _keyType.GetSelectedId();
        set
        {
            if (type != value)
                _keyType.Select(value);
        }
    }

    public override void _Ready()
    {
        _spinTime = this.FirstChild<SpinBox>(true, "spin_time");
        _spinRate = this.FirstChild<SpinBox>(true, "spin_rate");
        _spinX = this.FirstChild<SpinBox>(true, "spin_x");
        _spinY = this.FirstChild<SpinBox>(true, "spin_y");
        _spinZ = this.FirstChild<SpinBox>(true, "spin_z");
        _keyType = this.FirstChild<OptionButton>(true, "key_type");

        _spinTime.Connect("value_changed", this, "value_changed");
        _spinRate.Connect("value_changed", this, "value_changed");
        _spinX.Connect("value_changed", this, "value_changed");
        _spinY.Connect("value_changed", this, "value_changed");
        _spinZ.Connect("value_changed", this, "value_changed");
        _keyType.Connect("item_selected", this, "item_selected");

        this.FirstChild<Button>(true, "btn_remove").Connect("pressed", this, "btn_remove_pressed");
        this.FirstChild<Button>(true, "btn_fire").Connect("pressed", this, "btn_fire_pressed");
        this.FirstChild<Button>(true, "btn_interpolate").Connect("pressed", this, "btn_interpolate_pressed");

        foreach (AAnimKey.EType t in System.Enum.GetValues(typeof(AAnimKey.EType)))
        {
            _keyType.AddItem(t.ToString(), (int)t);
        }
        _keyType.Select(0);
    } // _Ready

    void Callback(int state)
    {
        CallbackChanged?.Invoke(this, state);
    }

    void value_changed(float value)
    {
        Callback(0);
    }
    void item_selected(int value)
    {
        Callback(0);
    }
    void btn_remove_pressed()
    {
        Callback(1);
    }
    void btn_fire_pressed()
    {
        Callback(2);
    }
    void btn_interpolate_pressed()
    {
        Callback(3);
    }
}
