using Godot;

public sealed class AUIGameplayART: AUIGameplay, IUpdateable
{
    public Control Menu { get; private set; } = null;
    ABarRound _focusBar;
    ABar _playerLifeBar;
    ABar _enemyLifeBar;

    Label _BActionLabel;

    public AUIDialogRoot DialogRoot { get; private set; } = null;

    public AGamepadPlay Gamepad { get; private set; } = null;

    AScenePlay Scene => A.App.SceneManager as AScenePlay;

    public override void Init()
    {
        _focusBar = GetNode<ABarRound>("play_controls/focus_bar");
        _playerLifeBar = GetNode<ABar>("play_controls/player_bar");
        _enemyLifeBar = GetNode<ABar>("play_controls/enemy_bar");

        _BActionLabel = this.FirstChild<Label>(true, "b_action_label");
        _BActionLabel.GetParent().FirstChild<Sprite>(true).ScaleKeepAspect();

        DialogRoot = this.FirstChild<AUIDialogRoot>(true);
    } // _Ready

    public float Focus
    {
        get => _focusBar.TargetPercent;
        set => _focusBar.TargetPercent = value;
    }

    public string PlayerName
    {
        get => _playerLifeBar.NameText;
        set
        {
            _playerLifeBar.NameText = value;
            _playerLifeBar.Visible = !string.IsNullOrEmpty(value);
        }
    }
    public string EnemyName
    {
        get => _enemyLifeBar.NameText;
        set
        {
            _enemyLifeBar.NameText = value;
            _enemyLifeBar.Visible = !string.IsNullOrEmpty(value);
        }
    }
    public void PlayerChangeHP(float life)
    {
        _playerLifeBar.ChangeHP(life);
    }
    public void EnemyChangeHP(float life)
    {
        _enemyLifeBar.ChangeHP(life);
    }

    public override void UpdateOverride()
    {
        _focusBar.UpdateOverride();

    } // UpdateOverride

    void RemoveCurrentMenu()
    {
        if (Menu == null)
            return;
        RemoveChild(Menu);
        Menu.QueueFree();
        Menu = null;
        A.App.UI.Cursor.HideOverride();
        GetNode<Control>("play_controls").Visible = false;
    }

    public void ShowMenu_Play()
    {
        RemoveCurrentMenu();

        Menu = Gamepad = Asset.Instantiate<AGamepadPlay>(Asset.touch_gamepad, this);
        Gamepad.Init();
        GetNode<Control>("play_controls").Visible = true;
    } // ShowMenu_Play

    public void ShowMenu_Pause()
    {
        ShowWithCursorUI(Asset.pause_menu);
    }
    public void ShowMenu_PauseSelect()
    {
        ShowWithCursorUI(Asset.select_menu);
    }
    public void ShowMenu_EditControl()
    {
        ShowWithCursorUI(Asset.edit_touch_gamepad);
    }
    public void ShowMenu_SelectItem()
    {
        ShowWithCursorUI(Asset.select_item_menu);
    }
    public void ShowMenu_AnimEdit()
    {
        ShowWithCursorUI(Asset.anim_edit_menu);
    }

    void ShowWithCursorUI(string menuAssetPath)
    {
        RemoveCurrentMenu();
        Menu = Asset.Instantiate<Control>(menuAssetPath, this);
        A.App.UI.Cursor.ShowOverride();
    }

    public void SetBAction(ATrigger trigger)
    {
        if (trigger != null)
        {
            _BActionLabel.Text = Tr(trigger.GetNameOverride());
            _BActionLabel.GetParent<Panel>().Visible = true;
        }
        else
            _BActionLabel.GetParent<Panel>().Visible = false;
    }
}
