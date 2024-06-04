using Godot;
using System.Threading.Tasks;

public class ADifferedObject<T>: Object
{
    public T Value;
    readonly TaskCompletionSource<bool> _taskSource = new TaskCompletionSource<bool>();

    public void Finish() => _taskSource.TrySetResult(true);

    public Task Task => _taskSource.Task;
}