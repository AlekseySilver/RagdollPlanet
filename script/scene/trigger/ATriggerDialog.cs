using Godot;

public sealed class ATriggerDialog: ATrigger
{
    [Export] public string dialog_name;

    AUIGameplayART UI => A.App.UI.Gameplay as AUIGameplayART;

    public APerson OwnerPerson
    {
        get
        {
            var b = GetParentOrNull<ABone3>();
            return b?.Person;
        }
    }

    public override void DoAction()
    {
        UI.DialogRoot.ShowDialog(dialog_name);
    }

    public override bool IsActionFinished()
    {
        return UI.DialogRoot.IsFinished;
    }

    public void Finish()
    {
        UI.DialogRoot.finish_dialog();
    }

    public override string GetNameOverride()
    {
        return "talk";
    }
}
