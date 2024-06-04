using Godot;

public partial class APerson {
    // never null
    ATypeDummy _type = new ATypeDummy();

    public enum EType { DUMMY = 0, PLAYER, ENEMY, NPC }

    [Export]
    public EType TYPE {
        get => _type is ATypePlayer ? EType.PLAYER
             : _type is ATypeEnemy ? EType.ENEMY
             : _type is ATypeNPC ? EType.NPC
             : EType.DUMMY;
        set {
            _type.Remove(this);
            switch (value) {
                case EType.PLAYER:
                    _type = new ATypePlayer();
                    break;
                case EType.ENEMY:
                    _type = new ATypeEnemy();
                    break;
                case EType.NPC:
                    _type = new ATypeNPC();
                    break;
                default:
                    _type = new ATypeDummy();
                    break;
            }
            if (IsInitialized)
                _type.Start(this);
        }
    }

    class ATypeDummy {
        public virtual Vector3 GetControlWorldDirection(APerson p) => Vector3.Zero;

        public virtual void Start(APerson p) { 
            p.DoAction(EGMAction.RELAX);
            p.EnableControl(true);
        }
        public virtual void Remove(APerson p) { }
        public virtual void Update(APerson p) { }

        public virtual void GrabStart(APerson p) { }
        public virtual void GrabFinish(APerson p) { }
        public virtual void KnockOut(APerson p) { }
        public virtual APerson GetRival(APerson p) => null;
    } // ATypeDummy

    sealed class ATypePlayer : ATypeDummy {
        public override Vector3 GetControlWorldDirection(APerson p) {
            Vector2 controlDirection = p.Control.GetAxis1();
            if (controlDirection == Vector2.Zero)
                return Vector3.Zero;
            var right = p.SceneManager.Camera.CurrentCameraRight;
            var forward = p.UpDirection.Cross(right);
            return right * controlDirection.x + forward * controlDirection.y;
        }

        AUnderShadow _underShadow = null;
        //ADashCursor _dashCursor = null;

        public override void Start(APerson p) {
            _underShadow = Asset.Instantiate2Scene<AUnderShadow>(Asset.UnderShadow);
            //_dashCursor = Asset.Instantiate2Scene<ADashCursor>(Asset.DashCursor);

            p.Control = p.SceneManager.UI.Gamepad;
			p.SceneManager.Camera.AssignTarget(p.Body);
            p.SceneManager.SetPlayer(p);

            base.Start(p);
        }

        public override void Remove(APerson p) {
            p.SceneManager.Fight.RemovePerson(p);
            _underShadow.QueueFree();
            _underShadow = null;
            //_dashCursor.QueueFree();
            //_dashCursor = null;
        }

        public override void Update(APerson p) {
            UpdateShadow(p);
            //UpdateDashCursor(p);

            // HideShadow on Close Camera
            if (p.IsMeshVisible == p.SceneManager.Camera.IsTargetClose) {
                p.IsMeshVisible = !p.IsMeshVisible;
            }
        } // UpdateOverride

        /*void UpdateDashCursor(APerson p) {
            if (p.has_floor && _dashCursor.Orient(p.Body.Position, p._floorPosition, p.SceneManager.Camera.CurrentCameraDirection)) {
                _dashCursor.Visible |= true;
            }
            else {
                _dashCursor.Visible &= false;
            }
        } // UpdateDashCursor
        */
        
        // under shadow
        void UpdateShadow(APerson p) {
            if (p.has_floor == false)
                _underShadow.HideShadow();
            else
                _underShadow.Orient(p.Body.Position, p._floorPosition, p._floorDirection, p.MOVE_SPEED);
        } // UpdateOverride shadow

        /* now Camera GAP better
        public override async void GrabStart(APerson p) {
            await p.SceneManager.Camera.AssignTargetAsync(p.GrabbedBody);
        }

        public override async void GrabFinish(APerson p) {
            await p.SceneManager.Camera.AssignTargetAsync(p.Body);
        }
        */

        public override void KnockOut(APerson p) {
            p.SceneManager.Camera.AssignTarget(null);
        }

        public override APerson GetRival(APerson p) {
            return p.SceneManager.Fight.Enemy;
        }
    } // ATypePlayer
} // APerson
