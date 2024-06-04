using Godot;

public class AEnemyControl: IPersonControl
{
    const float MAX_IDLE_TIME = 2.0f;
    const float MAX_SCAN_TIME = 2.0f;
    const float MIN_JUMP_TIME = 1.0f;
    const float MAX_JUMP_TIME = 2.0f;
    const float MAX_ATTACK_TIME = 2.0f;

    const float APROACH_CHECK_TIME = 1.0f;

    const float APROACH_DIST = 6.0f;
    const float APROACH_DIST_SQ = APROACH_DIST * APROACH_DIST;

    enum EAIAction
    {
        IDLE,
        SCAN, // seek Target

        APROACH, // aproach to Target
        JUMP,
        ATTACK,
        RETIRE,

        NPC_MODE, // stand and talk
    }

    readonly FSM<EAIAction> _FSM = new FSM<EAIAction>();

    Vector3 _dir;
    IControlable _person;

    APerson _rival = null;
    Vector3 _rivalLastSeenPoint;

    float _restTime;

    public bool Enabled
    {
        get => true;
        set
        {
        }
    }

    public Vector2 GetAxis1()
    {
        return Vector2.Zero;
    }
    public Vector3 GetAxis3D1()
    {
        return _dir;
    }

    public Vector2 GetAxis2()
    {
        return Vector2.Zero;
    }

    public float GetVertical()
    {
        return 0.0f;
    }

    public float GetZoom()
    {
        return 0.0f;
    }

    ////////////////////////////////////////////////////////////////////

    void UpdateLastSeen()
    {
        if (_rival != null)
            _rivalLastSeenPoint = _rival.Body.Position;
    }

    public void InitPerson(IControlable person)
    {
        _person = person;

        // FSM
        // IDLE
        var a = _FSM.AddAction(EAIAction.IDLE);
        a.OnStart = () =>
        {
            _restTime = MAX_IDLE_TIME;
            _dir = Vector3.Zero;
        };
        a.CheckFinish = () =>
        {
            _restTime -= TimeStep;
            return _restTime < 0.0f;
        };

        // SCAN
        a = _FSM.AddAction(EAIAction.SCAN);
        a.OnStart = () =>
        {
            _rival = null;
            _restTime = 0.0f;

        };
        a.CheckFinish = () =>
        {
            _restTime -= TimeStep;
            if (_restTime < 0.0f)
            {
                _person.CanSeeRival(out _rival);
                _restTime = MAX_SCAN_TIME;
                UpdateLastSeen();
            }
            return false;
        };

        // APROACH
        a = _FSM.AddAction(EAIAction.APROACH);
        a.OnStart = () =>
        {
            _restTime = APROACH_CHECK_TIME;
            _dir = Dir2Rival;
            if (person.IsGrabbed)
                person.ReleaseGrab();
        };
        a.CheckFinish = () =>
        {
            _restTime -= TimeStep;
            if (_restTime < 0.0f)
            {
                _restTime = APROACH_CHECK_TIME;
                _dir = Dir2Rival;
            }
            UpdateLastSeen();
            return false;
        };

        // JUMP
        a = _FSM.AddAction(EAIAction.JUMP);
        a.OnStart = () =>
        {
            _restTime = Mathf.Lerp(MIN_JUMP_TIME, MAX_JUMP_TIME, Xts.RndNextFloat());
        };
        a.CheckFinish = CheckFinishAct;

        // ATTACK
        a = _FSM.AddAction(EAIAction.ATTACK);
        a.OnStart = () =>
        {
            _restTime = MAX_ATTACK_TIME;
        };
        a.CheckFinish = CheckFinishAct;

        // RETIRE
        a = _FSM.AddAction(EAIAction.RETIRE);
        a.OnStart = () =>
        {
            _dir = -Dir2Rival;
        };
        a.CheckFinish = () =>
        {
            return Vec2Rival.LengthSquared() > 3.0f;
        };

        // NPC_MODE
        a = _FSM.AddAction(EAIAction.NPC_MODE);
        a.OnStart = () =>
        {
            _dir = Vector3.Zero;
            _rival = null;
        };

        // JUMPS
        _FSM.AddJumpReason(EAIAction.IDLE, EAIAction.SCAN, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EAIAction.SCAN, EAIAction.IDLE, () => _rival == null);

        _FSM.AddJumpReason(EAIAction.SCAN, EAIAction.APROACH, () => _rival != null);

        _FSM.AddJumpReason(EAIAction.JUMP, EAIAction.ATTACK, _FSM.IsActiveActionFinished);
        _FSM.AddJumpReason(EAIAction.APROACH, EAIAction.ATTACK, () => DistSq2Rival < APROACH_DIST_SQ);
        _FSM.AddJumpReason(EAIAction.ATTACK, EAIAction.IDLE, _FSM.IsActiveActionFinished);

        // DEFAULT
        _FSM.SetAsDefault(EAIAction.IDLE);
        _FSM.SetAsActive(EAIAction.IDLE);
    } // public void InitPerson

    /// <summary>
    /// UpdateOverride FSM
    /// </summary>
    public void UpdateOverride()
    {
        _FSM.Update();

        // debug
        (A.App.SceneManager as AScenePlay).debug0 = $"{_FSM.PrevActionKey} -> {_FSM.ActiveActionKey} -- {_rival?.Name}";

        if (IsJumpPressed)
            JumpPressedTime += A.TimeStep;
        else
            JumpPressedTime = 0.0f;

        if (IsKickPressed)
            KickPressedTime += A.TimeStep;
        else
            KickPressedTime = 0.0f;
    }

    bool CheckFinishAct()
    {
        _restTime -= TimeStep;
        if (_restTime < 0.0f)
            return true;

        if (_person.FSMActiveAction == APerson.EGMAction.KNOCK_DOWN)
        {
            return true;
        }
        if (_person.FSMActiveAction == APerson.EGMAction.KNOCK_OUT)
        {
            _FSM.SetAsActive(EAIAction.NPC_MODE);
            return true;
        }

        UpdateLastSeen();
        return false;
    }

    Vector3 Vec2Rival => _rivalLastSeenPoint - _person.Body.Position;
    Vector3 Dir2Rival => Vec2Rival.Normalized();
    float DistSq2Rival => Vec2Rival.LengthSquared();

    float TimeStep => A.TimeStep;

    public bool IsJumpPressed => _FSM.ActiveActionKey == EAIAction.JUMP;
    public bool IsKickPressed => _FSM.ActiveActionKey == EAIAction.ATTACK;

    public float JumpPressedTime { get; private set; } = 0.0f;
    public float KickPressedTime { get; private set; } = 0.0f;

    public bool NPCMode
    {
        get => _FSM.ActiveActionKey == EAIAction.NPC_MODE;
        set => _FSM.SetAsActive(value ? EAIAction.NPC_MODE : EAIAction.IDLE);
    }
}
