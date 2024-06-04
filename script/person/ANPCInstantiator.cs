using Godot;

public class ANPCInstantiator: Spatial
{
    [Export(PropertyHint.File)] public string person_resource;
    [Export] public string init_dialog_name;

    APerson _NPCPerson = null;

    public override void _Ready()
    {
        if (Visible)
        {
            A.App.DoOnce(() =>
            {
                var p = Asset.Instantiate<APerson>(person_resource, this);
                p.TYPE = APerson.EType.NPC;
                p.Init();
                p.NPCDialogName = init_dialog_name;
                _NPCPerson = p;
            });
        }
    } // _Ready

    public override void _ExitTree()
    {
        _NPCPerson?.SceneManager.Fight.QueueFreePerson(_NPCPerson);
    }
} // class ANPCInstantiator
