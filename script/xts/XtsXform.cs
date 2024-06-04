using Godot;
using System;
using System.Runtime.CompilerServices;

public static partial class Xts
{
    /// <summary>
    /// scale in the basis in the local space
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetLocalScale(this Basis b)
    {
        return new Vector3(b.x.Length(), b.y.Length(), b.z.Length());
    }

    /// <summary>
    /// set the scale in the basis in the local space
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void MultiplyLocalScale(ref Basis b, in Vector3 scale)
    {
        b.x *= scale.x;
        b.y *= scale.y;
        b.z *= scale.z;
    }

    /// <summary>
    /// scale in the basis in the local space
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Basis GetRotation(this Basis b)
    {
        var inv = b.GetLocalScale().Inverse();
        MultiplyLocalScale(ref b, inv);
        return b;
    }

    /// <summary>
    /// align the matrix so that the Y axis was in the direction of the up direction
    /// </summary>
    public static void UpAlign(this Spatial node)
    {
        node.GlobalTransform = UpAlign(node.GlobalTransform);
    }

    /// <summary>
    /// align the matrix so that the Y axis was in the direction of the up direction
    /// </summary>
    public static Transform UpAlign(in Transform xf)
    {
        return UpAlign(xf, xf.origin.Normalized());
    }

    /// <summary>
    /// align the matrix so that the Y axis was in the direction of the up direction
    /// </summary>
    public static Transform UpAlign(Transform xf, in Vector3 upDir)
    {
        xf.basis = UpAlign(xf.basis, upDir);
        return xf;
    }

    /// <summary>
    /// align the matrix so that the Y axis was in the direction of the up direction
    /// </summary>
    public static Basis UpAlign(Basis basis, in Vector3 upDir)
    {
        var scale = basis.GetLocalScale();

        // Z = [X * Y]
        // X = [Y * Z]
        if (IsBetween(basis.x.Dot(upDir), -SIN45, SIN45))
        {
            basis.z = basis.x.Cross(upDir).Normalized();
            basis.x = upDir.Cross(basis.z);
        }
        else
        {
            basis.x = upDir.Cross(basis.z).Normalized();
            basis.z = basis.x.Cross(upDir);
        }
        basis.y = upDir;
        //GD.Print($"basis {basis.ToString()}");
        MultiplyLocalScale(ref basis, scale);
        return basis;
    }

    /// <summary>
    /// rotation and scale in local coordinates
    /// </summary>
    public static void BasisXformLocal(this Spatial node, in Vector3 addRotate, in Vector3 addScale)
    {
        var t = node.Transform;
        var b = t.basis;

        var localScale = b.GetLocalScale();

        // normalize basis
        MultiplyLocalScale(ref b, localScale.Inverse());

        if (addRotate != Vector3.Zero)
        {
            b = b.Rotated(b.x, addRotate.x);
            b = b.Rotated(b.y, addRotate.y);
            b = b.Rotated(b.z, addRotate.z);
            b = b.Orthonormalized();
        }

        if (addScale != Vector3.Zero)
            localScale += addScale;

        MultiplyLocalScale(ref b, localScale);

        t.basis = b;
        node.Transform = t;
    } // BasisXformLocal

    public delegate void BASIS_LOCAL_XFORMER(ref Basis rotate, ref Vector3 scale);
    public static void BasisXFormLocal(this Spatial node, BASIS_LOCAL_XFORMER xform)
    {
        var t = node.Transform;
        var b = t.basis;

        var localScale = b.GetLocalScale();

        // normalize basis
        MultiplyLocalScale(ref b, localScale.Inverse());
        xform(ref b, ref localScale);
        MultiplyLocalScale(ref b, localScale);

        t.basis = b;
        node.Transform = t;
    } // BasisXformLocal

    public static Quat FromRotationTo(Vector3 start, Vector3 end)
    {
        start = start.Normalized();
        end = end.Normalized();

        const float epsilon = 0.000001f;
        float d = start.Dot(end);

        if (d > -1.0f + epsilon)
        {
            Vector3 c = start.Cross(end);
            float s = (float)Math.Sqrt((1.0f + d) * 2.0f);
            float invS = 1.0f / s;

            return new Quat(c.x * invS
                            , c.y * invS
                            , c.z * invS
                            , 0.5f * s).Normalized();
        }

        Vector3 axis = new Vector3(1.0f, 0.0f, 0.0f).Cross(start);
        if (axis.Length() < epsilon)
            axis = new Vector3(0.0f, 1.0f, 0.0f).Cross(start);

        return new Quat(axis, (float)Math.PI).Normalized();
    } // FromRotationTo

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Basis Vector2Basis(Vector3 x, Vector3 y)
    {
        x = x.Normalized();
        y = y.Normalized();
        return new Basis(x, y, x.Cross(y)).Orthonormalized();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Basis2Vector(Basis b, out Vector3 x, out Vector3 y)
    {
        x = b.Column0;
        y = b.Column1;
    }
} // Xts
