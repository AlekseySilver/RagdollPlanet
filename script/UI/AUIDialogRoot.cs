using Godot;

public class AUIDialogRoot: Control
{
    public bool IsFinished { get; private set; } = true;

    public void ShowDialog(string dialogName)
    {
        var d = DialogicSharp.Start(dialogName, useCanvasInstead: false);
        d.Connect("wait_input", this, "wait_input");
        d.Connect("timeline_start", this, "timeline_start");
        d.Connect("timeline_end", this, "timeline_end");
        d.Connect("tree_exited", this, "finish_dialog");
        d.Connect("dialogic_signal", this, "dialogic_signal");
        CallDeferred("add_child", d);
        IsFinished = false;
    }
    public void finish_dialog()
    {
        IsFinished = true;
    }
    public async void wait_input(bool visible)
    {
        if (visible)
        {
            await this.DelayAsync(0.5f);
            A.App.UI.Cursor.ShowOverride();
        }
        else
            A.App.UI.Cursor.HideOverride();
    }
    public void timeline_start(string timeLineName)
    {
        //A.App.UI.Cursor.ShowOverride();
    }
    public void timeline_end(string timeLineName)
    {
        //A.App.UI.Cursor.ShowOverride();
    }
    public void dialogic_signal(string sceneMethod)
    {
        if (IsInstanceValid(this))
            A.App.SceneManager.Call(sceneMethod);
    }
} // AUIDialogRoot
