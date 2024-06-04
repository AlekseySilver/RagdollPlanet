using Godot;

public class ARibbonTrail2: CPUParticles, IFixedUpdateAble
{
    [Export]
    public float MAX_EMIT_TIME = 3f;


    /// <summary>
    /// line Emitter
    /// </summary>
    [Export]
    public Spatial Emitter
    {
        get => _emitter;
        set
        {
            if (value != null)
            {
                _emitter = value;
                A.App.SceneManager.UPD.Add4Fixed(this);
                this.SetGlobalPos(_emitter.GetGlobalPos());
                Visible = true;
                OneShot = false;
                Emitting = true;
            }
            else
            {
                _emitRest = float.MaxValue;
                OneShot = true;
            }
        }
    }
    Spatial _emitter = null;

    float _emitRest;

    public void FixedUpdate()
    {
        if (Emitting)
        {
            this.SetGlobalPos(_emitter.GetGlobalPos());
        }
        else
        {
            if (_emitRest > MAX_EMIT_TIME)
            {
                _emitRest = MAX_EMIT_TIME;
            }
            _emitRest -= A.FixedStep;
            if (_emitRest < 0.0f)
            {
                A.App.SceneManager.UPD.Rem4Fixed(this);
                Visible = false;
                _emitter = null;
            }
        }
    } // FixedUpdate


    public void Finish()
    {
        A.App.SceneManager.UPD.Rem4Fixed(this);
        if (IsInstanceValid(this))
        {
            Visible = false;
            Emitting = false;
        }
        _emitter = null;
    }
}
