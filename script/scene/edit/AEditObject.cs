using Godot;

public abstract class AEditObjectBase
{
    public Spatial Root { get; private set; } = null;
    public CollisionObject CollisionObject { get; protected set; } = null;


    protected virtual void init(Spatial root)
    {
        this.Root = root;
        CollisionObject = FindCollisionObject(root);
    }

    static CollisionObject FindCollisionObject(Spatial root)
    {
        return root as CollisionObject ?? root.FirstChild<CollisionObject>(true);
    }

    public virtual void SetEditMode()
    {
    }
    public virtual void SetPlayMode()
    {
    }

    public void OutlineAdd()
    {
        Root?.ForeachChild<MeshInstance>(true, mi =>
        {
            if (mi.GetSurfaceMaterialCount() > 0)
            {
                var mat = mi.GetSurfaceMaterial(0);
                if (mat != null && mat.NextPass == null)
                    mat.NextPass = ResourceLoader.Load<Material>(Asset.material_outline);
            }
            return false;
        });
    }
    public void OutlineRemove()
    {
        Root?.ForeachChild<MeshInstance>(true, mi =>
        {
            if (mi.GetSurfaceMaterialCount() > 0)
            {
                var mat = mi.GetSurfaceMaterial(0);
                if (mat != null && mat.NextPass != null && mat.NextPass.ResourcePath == Asset.material_outline)
                    mat.NextPass = null;
            }
            return false;
        });
    }

    public virtual void AddScaleLocal(Vector3 deltaScale)
    {
        Root.BasisXformLocal(Vector3.Zero, deltaScale);
    }

    public static AEditObjectBase NewInstance(Spatial root)
    {
        AEditObjectBase res = null;

        var co = FindCollisionObject(root);
        if (co != null)
        {
            switch (co.FirstChild<CollisionShape>()?.Name)
            {
                case "grab_bar_shape":
                    res = new AEditObjectGrabBar();
                    break;
            }

            if (res == null)
            {
                switch (co.FirstChild<CollisionShape>()?.Shape?.GetClass())
                {
                    case nameof(CylinderShape):
                        res = new AEditObjectCylinder();
                        break;
                    case nameof(SphereShape):
                        res = new AEditObjectSphere();
                        break;
                    default:
                        res = new AEditObjectBlock();
                        break;
                }
            }
        }

        res?.init(root);

        return res;
    } // NewInstance
} // AEditObjectBase

public abstract class AEditObjectCustomScaler: AEditObjectBase
{
    class AScaler
    {
        public Vector3 AddScale;
        public AEditObjectCustomScaler parent;

        public void LocalXFormer(ref Basis rotate, ref Vector3 scale)
        {
            if (AddScale != Vector3.Zero)
                scale += AddScale;
            parent.FixScale(ref scale);
        }
    }

    protected abstract void FixScale(ref Vector3 scale);

    public override void AddScaleLocal(Vector3 delta_scale)
    {
        Root.BasisXFormLocal(new AScaler { AddScale = delta_scale, parent = this }.LocalXFormer);
    }
} // class AEditObjectCustomScaler

public sealed class AEditObjectBlock: AEditObjectBase
{
} // AEditObjectBlock

public sealed class AEditObjectGrabBar: AEditObjectCustomScaler
{
    protected override void FixScale(ref Vector3 scale)
    {
        scale.x = scale.y = 1f;
    }
} // AEditObjectGrabBar

public sealed class AEditObjectCylinder: AEditObjectCustomScaler
{
    protected override void FixScale(ref Vector3 scale)
    {
        scale.x = scale.z;
    }
} // AEditObjectCylinder


public sealed class AEditObjectSphere: AEditObjectCustomScaler
{
    protected override void FixScale(ref Vector3 scale)
    {
        scale.x = scale.y = scale.z;
    }
} // AEditObjectSphere
