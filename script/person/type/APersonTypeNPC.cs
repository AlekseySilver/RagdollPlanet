public partial class APerson
{
    public string NPCDialogName
    {
        get => (_type as ATypeNPC)?.DialogName;
        set
        {
            if (_type is ATypeNPC t)
                t.DialogName = value;
        }
    }

    sealed class ATypeNPC: ATypeDummy
    {
        ATriggerDialog _dialog = null;

        public override void Start(APerson p)
        {
            p.Control = new ANPCControl();

            _dialog = Asset.Instantiate<ATriggerDialog>(Asset.trigger_dialog, p.Body);
            p.SceneManager.Fight.AddNPC(p);

            base.Start(p);
        }

        public override void Remove(APerson p)
        {
            _dialog.Finish();
            _dialog.QueueFree();
            _dialog = null;
            p.SceneManager.Fight.RemovePerson(p);
        }

        public override APerson GetRival(APerson p)
        {
            return p.SceneManager.Fight.Player;
        }

        public string DialogName
        {
            get => _dialog.dialog_name;
            set => _dialog.dialog_name = value;
        }
    } // ATypeNPC
} // APerson
