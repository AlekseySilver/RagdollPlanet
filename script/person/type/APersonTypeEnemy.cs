using Godot;

public partial class APerson
{
    sealed class ATypeEnemy: ATypeDummy
    {
        public override void Start(APerson p)
        {
            var c = new AEnemyControl();
            p.Control = c;
            c.InitPerson(p);
            p.SceneManager.Fight.AddEnemy(p);
            base.Start(p);
        }

        public override void Remove(APerson p)
        {
            p.SceneManager.Fight.RemovePerson(p);
        }

        public override Vector3 GetControlWorldDirection(APerson p)
        {
            Vector3 controlDirection = p.Control.GetAxis3D1();
            if (controlDirection != Vector3.Zero)
                controlDirection = controlDirection.InPlane(p.UpDirection);

            return controlDirection;
        }

        public override APerson GetRival(APerson p)
        {
            return p.SceneManager.Fight.Player;
        }
    } // ATypeEnemy
} // APerson
