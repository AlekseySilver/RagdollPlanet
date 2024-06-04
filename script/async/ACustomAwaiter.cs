using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public abstract class ACustomAwaiterBase<T>
{
    public Action OnUpdate = null;

    TaskCompletionSource<T> _taskSource = null;
    readonly protected IUpdateHolder _updateHolder;

    public ACustomAwaiterBase(IUpdateHolder updateHolder)
    {
        _updateHolder = updateHolder;
    }
    ~ACustomAwaiterBase()
    {
        Finish(default);
    }

    public Task<T> WaitAsync()
    {
        Start();
        _taskSource = new TaskCompletionSource<T>();
        Add4Update();
        return _taskSource.Task;
    }

    public void Finish(T result)
    {
        Rem4Update();
        _taskSource?.TrySetResult(result);
    }

    protected abstract void Add4Update();
    protected abstract void Rem4Update();
    protected virtual void Start() { }
} // ACustomAwaiterBase

public class ACustomAwaiterUpdate<T>: ACustomAwaiterBase<T>, IUpdateable
{
    public ACustomAwaiterUpdate(IUpdateHolder updateHolder) : base(updateHolder)
    {
    }
    protected override void Add4Update() => _updateHolder.UPD.Add4Update(this);
    protected override void Rem4Update() => _updateHolder.UPD.Rem4Update(this);
    void IUpdateable.UpdateOverride() => OnUpdate();
} // ACustomAwaiterUpdate

public class ACustomAwaiterFixed<T>: ACustomAwaiterBase<T>, IFixedUpdateAble
{
    public ACustomAwaiterFixed(IUpdateHolder updateHolder) : base(updateHolder)
    {
    }
    protected override void Add4Update() => _updateHolder.UPD.Add4Fixed(this);
    protected override void Rem4Update() => _updateHolder.UPD.Rem4Fixed(this);
    void IFixedUpdateAble.FixedUpdate() => OnUpdate();
} // ACustomAwaiterFixed

public class ATimeAwaiter<T>: ACustomAwaiterUpdate<T>
{
    public float TimeOutSecond;

    public ATimeAwaiter(IUpdateHolder updateHolder) : base(updateHolder)
    {
    }

    public bool CheckTimeout()
    {
        TimeOutSecond -= A.TimeStep;
        if (TimeOutSecond < 0.0f)
            return true;
        return false;
    }

    public Task<T> WaitAsync(float seconds)
    {
        TimeOutSecond = seconds;
        return WaitAsync();
    }
} // ATimeAwaiter

public class AStepAwaiter<T>: ATimeAwaiter<T>
{
    public float StepTime;
    float _restStepTime;

    public AStepAwaiter(IUpdateHolder updateHolder) : base(updateHolder)
    {
    }

    public bool CheckStep()
    {
        _restStepTime -= A.TimeStep;
        if (_restStepTime < 0.0f)
        {
            _restStepTime = StepTime;
            return true;
        }
        return false;
    }

    protected override void Start()
    {
        base.Start();
        _restStepTime = 0.0f;
    }
} // AStepAwaiter

