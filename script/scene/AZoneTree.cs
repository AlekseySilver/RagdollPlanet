using Godot;

public class A_zone_tree: Spatial, IUpdateable
{
    [Export(PropertyHint.File)] public string content_resource { get; set; }
    [Export] public float load_cos { get; set; } = 0.7f;
    [Export] public float unload_cos { get; set; } = 0.6f;

    Vector3 _direction;
    Spatial _instance = null;

    public override void _Ready()
    {
        _direction = GlobalTranslation.Normalized();

        // TODO Add lazy children & UpdateOverride
    }

    public void UpdateOverride()
    {
        var cos = A.App.SceneManager.GetCamera().GlobalUp.Dot(_direction);
        if (cos > load_cos && _instance == null)
        {
            if (string.IsNullOrEmpty(content_resource) == false)
                _instance = Asset.Instantiate<Spatial>(content_resource, this);
        }
        else if (cos < unload_cos && _instance != null)
        {
            _instance.QueueFree();
            _instance = null;
        }
    }
} // A_zone_tree
