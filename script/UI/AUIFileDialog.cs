using Godot;

public class AUIFileDialog: FileDialog
{
    System.Action<string> OnFileSelect = null;

    public static void Show(System.Action<string> onFileSelect)
    {
        var fd = Asset.Instantiate2UI<AUIFileDialog>(Asset.file_dialog);
        fd.OnFileSelect = onFileSelect;
        fd.Connect("file_selected", fd, "_on_file_select");
        fd.Connect("popup_hide", fd, "_on_popup_hide");
        fd.Popup_();
    } // ShowOverride

    void _on_file_select(string fileName)
    {
        OnFileSelect?.Invoke(CurrentPath);
    }
    void _on_popup_hide()
    {
        QueueFree();
    }
}
