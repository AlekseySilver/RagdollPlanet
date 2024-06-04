using Godot;

public class ADashCursor: MeshInstance
{
    [Export] float MIN_DIST = 1.0f;
    [Export] float MAX_DIST = 30.0f;
    [Export] float WIDTH = 1.0f;

    public bool Orient(in Vector3 from, in Vector3 to, in Vector3 camDir)
    {
        var up = from - to;
        var len = up.Length();
        if (Xts.IsNotBetween(len, MIN_DIST, MAX_DIST))
            return false;

        up *= 1f / len;

        // Z = [X * Y]
        // X = [Y * Z]
        var basis = new Basis
        {
            y = up,
            x = -up.Cross(camDir).Normalized()
        };
        basis.z = basis.x.Cross(up);

        // scale
        basis.x *= WIDTH * len / MAX_DIST;
        basis.y *= len;

        var xf = new Transform(basis, up * (len * 0.5f) + to);
        Transform = xf;
        return true;
    }
}
