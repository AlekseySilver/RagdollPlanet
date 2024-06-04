using Godot;
using System.Threading.Tasks;

public abstract class AScene: Spatial, IUpdateable, IFixedUpdateAble, IUpdateHolder
{
    public AUpdateCase UPD { get; } = new AUpdateCase();

    public bool IsPaused { get; set; }

    public virtual Task InitAsync()
    {
        //await A.App.UI.
        return Task.CompletedTask;
    }

    public virtual void UpdateOverride()
    {
        if (!IsPaused)
            UPD.Update();
    }
    public virtual void FixedUpdate()
    {
        if (!IsPaused)
            UPD.FixedUpdate();
    }

    public abstract ACamera GetCamera();

    public abstract void Pause();
}
