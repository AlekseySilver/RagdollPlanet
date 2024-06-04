using Godot;

public static partial class Xts
{
    public static void SizeKeepAspect(this Control c)
    {
        var p = c.GetParent<Control>();

        var sz = c.GetGlobalRect().Size;
        var pz = p.GetGlobalRect().Size;

        float aspect = sz.x / sz.y;

        if (aspect > pz.x / pz.y)
        { // more elongated in width than the parent
            sz.x = pz.x;
            sz.y = sz.x / aspect;
        }
        else
        { // more elongated in height than the parent
            sz.y = pz.y;
            sz.x = (int)(sz.y * aspect);
        }
    }

    public static void ScaleKeepAspect(this Sprite c)
    {
        var p = c.GetParent<Control>();

        var pr = p.GetGlobalRect();
        var cr = c.GetRect();
        c.GlobalPosition = pr.Size * 0.5f + pr.Position;

        var ps = Xts.Min(pr.Size.x, pr.Size.y);
        var cs = Xts.Min(cr.Size.x, cr.Size.y);
        var s = ps / cs;
        c.Scale = new Vector2(s, s);
    }

    public static Vector2 GetCenter(this Rect2 c)
    {
        return c.Size * 0.5f + c.Position;
    }
    public static Vector2 GetRectGlobalCenter(this Control c)
    {
        return c.GetGlobalRect().GetCenter();
    }
    public static void SetRectGlobalCenter(this Control c, Vector2 pos)
    {
        c.RectGlobalPosition = c.GetGlobalRect().Size * -0.5f + pos;
    }

    public static string ToString3(this float value)
    {
        return value.ToString("0.000").Replace(',', '.');
    }
}
