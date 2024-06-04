using Godot;
using System.Collections.Generic;

public class ARibbonTrail: ImmediateGeometry
{
    /// <summary>
    /// trail line width
    /// </summary>
    [Export]
    public float width = 1.0f;

    /// <summary>
    /// Max point Count in line (
    /// </summary>
    [Export]
    public int point_count = 25;

    /// <summary>
    /// Time Between adding points
    /// </summary>
    [Export]
    public float point_add_time = 1.0f / 50.0f;

    [Export]
    public float min_draw_len_sq = 1.0f;

    /// <summary>
    /// line Emitter
    /// </summary>
    [Export]
    public Spatial Emitter
    {
        get => _emitter;
        set
        {
            _emitter = value;
            if (_emitter != null)
            {
                if (Camera == null)
                    Camera = Emitter.GetViewport().GetCamera();

                _points.Clear();
                _addRest = 0.0f;
                SetProcess(true);
            }
        }
    }
    Spatial _emitter = null;
    float _addRest = float.MaxValue;

    readonly List<Vector3> _points = new List<Vector3>(10);

    public Camera Camera = null;

    /// <summary>
    /// call every frame
    /// </summary>
    /// <param Name="delta">fram Time step</param>
    public override void _Process(float delta)
    {
        Clear();

        _addRest -= delta;
        if (_addRest < 0.0f)
        { // Add new point
            if (Emitter != null)
            {
                _points.Add(Emitter.GlobalTransform.origin);
                if (_points.Count > point_count)
                    _points.RemoveAt(0);
            }
            else
            { // Remove point until trail ends
                if (_points.Count < 2)
                {
                    _points.Clear();
                    SetProcess(false);
                    return;
                }
                else
                    _points.RemoveAt(0);
            }
            _addRest = point_add_time;
        }

        if (_points.Count < 2)
            return; // not enough point to draw line

        var cross = _points[0] - _points[_points.Count - 1];
        if (cross.LengthSquared() < min_draw_len_sq)
            return;

        var camDir = Camera.GlobalTransform.basis.z;
        cross = cross.Normalized();
        cross = camDir.Cross(cross);//.Normalized();

        // Begin draw. // TriangleStrip screenshot = https://startandroid.ru/ru/uroki/vse-uroki-spiskom/399-urok-170-opengl-graficheskie-primitivy.html
        Begin(Mesh.PrimitiveType.TriangleStrip);

        // Prepare attributes for add_vertex.
        // to Camera Z for all vertices
        SetNormal(camDir);

        float uvx = 1f;
        float invCount = 1f / _points.Count;
        float f = 0.0f; // will be == 1f at last point
        foreach (var p in _points)
        {
            // color alpha
            f += invCount;
            var c = Colors.White;
            float b = 1f - f;
            c.a = 1f - b * b * b;
            SetColor(c);

            // swap uv.X
            uvx = uvx > 0.0f ? 0.0f : 1f;

            float w = EaseBackOut(f) * width;

            // down point
            SetUv(new Vector2(uvx, 0.0f));
            AddVertex(cross * -w + p);

            // up point
            SetUv(new Vector2(uvx, 1f));
            AddVertex(cross * w + p);

        } // foreach

        // End drawing.
        End();

    } // _Process


    float EaseBackOut(float time)
    {
        const float overshoot = 6.5f;
        return time * time * (overshoot * -time + overshoot);
    }
}
