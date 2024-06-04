using Godot;
using System;
using System.Runtime.CompilerServices;

public static partial class Xts
{
    public static bool ForeachChild<T>(this Node node, bool recursive, Func<T, bool> breakFunc) where T : Node
    {
        var n = node.GetChildCount();
        for (int i = 0; i < n; ++i)
        {
            var r = node.GetChildOrNull<T>(i);
            if (r != null && breakFunc(r))
                return true;
        }
        if (recursive)
        {
            for (int i = 0; i < n; ++i)
            {
                var r = node.GetChild(i);
                if (r.ForeachChild(true, breakFunc))
                    return true;
            }
        }
        return false;
    } // ForeachChild

    public static T FirstChild<T>(this Node node, bool recursive = false) where T : Node
    {
        var n = node.GetChildCount();
        for (int i = 0; i < n; ++i)
        {
            var r = node.GetChildOrNull<T>(i);
            if (r != null)
                return r;
        }
        if (recursive)
        {
            for (int i = 0; i < n; ++i)
            {
                var r = node.GetChild(i).FirstChild<T>(true);
                if (r != null)
                    return r;
            }
        }
        return null;
    } // FirstChild

    public static T FirstChild<T>(this Node node, bool recursive, string name) where T : Node
    {
        var n = node.GetChildCount();
        for (int i = 0; i < n; ++i)
        {
            var r = node.GetChildOrNull<T>(i);
            if (r != null && r.Name == name)
                return r;
        }
        if (recursive)
        {
            for (int i = 0; i < n; ++i)
            {
                var r = node.GetChild(i).FirstChild<T>(true, name);
                if (r != null)
                    return r;
            }
        }
        return null;
    } // FirstChild

    public static Node LastChild(this Node node)
    {
        int i = node.GetChildCount();
        return i > 0 ? node.GetChild(i - 1) : null;
    }

    public static void ClearChildren(this Node node)
    {
        while (node.GetChildCount() > 0)
        {
            var c = node.GetChild(0);
            node.RemoveChild(c);
            c.QueueFree();
        }
    } // ClearChildren

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Basis BasisLocal2World(this Spatial node)
    {
        return node.GlobalTransform.basis; //.Orthonormalized();
        //return node.GlobalTransform.basis.Quat().Xform(dir);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 DirLocal2World(this Spatial node, in Vector3 dir)
    {
        return node.BasisLocal2World().Xform(dir);
        //return node.GlobalTransform.basis.Quat().Xform(dir);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 DirWorld2Local(this Spatial node, in Vector3 dir)
    {
        return node.BasisLocal2World().XformInv(dir);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetGlobalPos(this Spatial node)
    {
        return node.GlobalTransform.origin;
    }
    public static void SetGlobalPos(this Spatial node, in Vector3 pos)
    {
        var a = node.GlobalTransform;
        a.origin = pos;
        node.GlobalTransform = a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 GetLocalPos(this Spatial node)
    {
        return node.Transform.origin;
    }
    public static void SetLocalPos(this Spatial node, in Vector3 pos)
    {
        var a = node.Transform;
        a.origin = pos;
        node.Transform = a;
    }

    public static SignalAwaiter DelayAsync(this Node node, float seconds)
    {
        return node.ToSignal(node.GetTree().CreateTimer(seconds), "timeout");
    }

    public static SignalAwaiter IdleFrameAsync(this Node node)
    {
        return node.ToSignal(node.GetTree(), "idle_frame");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IntersectRayRes(this Spatial node, in Vector3 from, in Vector3 dir, uint collisionMask, out Godot.Collections.Dictionary result)
    {
        result = node.GetWorld().DirectSpaceState.IntersectRay(from
            , from + dir
            , collisionMask: collisionMask);
        return result.Count > 0;
    }

    public static bool IntersectRay(this Spatial node, in Vector3 from, in Vector3 dir, uint collisionMask, ref Vector3 normal)
    {
        if (node.IntersectRayRes(from, dir, collisionMask, out var res))
        {
            normal = (Vector3)res["normal"];
            return true;
        }
        return false;
    }

    public static bool IntersectRay(this Spatial node, in Vector3 from, in Vector3 dir, uint collisionMask, out Vector3 normal, out Vector3 position)
    {
        if (node.IntersectRayRes(from, dir, collisionMask, out var res))
        {
            normal = (Vector3)res["normal"];
            position = (Vector3)res["position"];
            //var id = PhysicsServer.BodyGetObjectInstanceId((RID)res["RID"]);
            //var obj = GD.InstanceFromId(id) as PhysicsBody;
            return true;
        }
        normal = Vector3.Zero;
        position = Vector3.Zero;
        return false;
    }
    // Spatial = PhysicsBody | CSGCombiner
    public static bool IntersectRay(this Spatial node, in Vector3 from, in Vector3 dir, uint collisionMask, out Vector3 normal, out Vector3 position, out Spatial body)
    {
        if (node.IntersectRayRes(from, dir, collisionMask, out var res))
        {
            normal = (Vector3)res["normal"];
            position = (Vector3)res["position"];
            body = (Spatial)res["collider"];
            return true;
        }
        normal = Vector3.Zero;
        position = Vector3.Zero;
        body = null;
        return false;
    }

    public static bool IntersectRay(this Spatial node, in Vector3 from, in Vector3 to, uint collisionMask, out Spatial body)
    {
        var res = node.GetWorld().DirectSpaceState.IntersectRay(from
            , to
            , collisionMask: collisionMask);

        if (res.Count > 0)
        {
            body = (Spatial)res["collider"];
            return true;
        }
        body = null;
        return false;
    }

    public static bool IntersectRay(this Spatial node, in Vector3 from, in Vector3 dir, uint collisionMask, Godot.Collections.Array exclude
                            , out Vector3 position, out PhysicsBody body)
    {
        var res = node.GetWorld().DirectSpaceState.IntersectRay(from
            , from + dir
            , exclude
            , collisionMask);

        if (res.Count > 0)
        {
            position = (Vector3)res["position"];
            body = (PhysicsBody)res["collider"];
            return true;
        }
        position = Vector3.Zero;
        body = null;
        return false;
    }

    public static void MakeLocal(this Node n, Node owner)
    {
        n.Filename = string.Empty;
        n.Owner = owner;
        n.ForeachChild<Node>(true, c =>
        {
            c.Owner = owner;
            return false;
        });
    }

    public static void MakeExternal(this Node n, Node owner, string fileName)
    {
        n.Filename = fileName;
        n.Owner = owner;
        n.ForeachChild<Node>(true, c =>
        {
            c.Owner = null;
            return false;
        });
    }

    public static void ForeachFile(string path, Action<string> action)
    {
        var dir = new Directory();
        if (dir.Open(path) == Error.Ok)
        {
            dir.ListDirBegin(true, true);
            while (true)
            {
                var file = dir.GetNext();
                if (string.IsNullOrWhiteSpace(file))
                    break;
                action(file);
            } // while
        }

    }
} // Xts
