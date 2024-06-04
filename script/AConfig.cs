using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class AConfig
{
    const string CONFIG_FILE = "user://settings.cfg";

    readonly ConfigFile _config = new ConfigFile();

    public void Load()
    {
        _config.Load(CONFIG_FILE);
    }

    public void Save()
    {
        _config.Save(CONFIG_FILE);
    }

    T Get<T>(string section, string key, T @default)
    {
        return (T)_config.GetValue(section, key, @default);
    }
    void Set<T>(string section, string key, T value)
    {
        _config.SetValue(section, key, value);
    }

    public Vector3 SizeJoystick
    {
        get => Get("screenpad", "joystick", Vector3.Zero);
        set => Set("screenpad", "joystick", value);
    }
    public Vector3 SizeButtonA
    {
        get => Get("screenpad", "button_A", Vector3.Zero);
        set => Set("screenpad", "button_A", value);
    }
    public Vector3 SizeButtonB
    {
        get => Get("screenpad", "button_B", Vector3.Zero);
        set => Set("screenpad", "button_B", value);
    }
    public Vector3 SizeButtonStart
    {
        get => Get("screenpad", "button_start", Vector3.Zero);
        set => Set("screenpad", "button_start", value);
    }
    public Vector3 SizeButtonSelect
    {
        get => Get("screenpad", "button_select", Vector3.Zero);
        set => Set("screenpad", "button_select", value);
    }
    public Vector3 SizeButtonMoveUp
    {
        get => Get("screenpad", "button_MU", Vector3.Zero);
        set => Set("screenpad", "button_MU", value);
    }
    public Vector3 SizeButtonMoveDown
    {
        get => Get("screenpad", "button_MD", Vector3.Zero);
        set => Set("screenpad", "button_MD", value);
    }
    public Vector2 SizeCameraMin
    {
        get => Get("screenpad", "camera_min", Vector2.Zero);
        set => Set("screenpad", "camera_max", value);
    }
    public Vector2 SizeCameraMax
    {
        get => Get("screenpad", "camera_max", Vector2.Zero);
        set => Set("screenpad", "camera_max", value);
    }
    public float Opacity
    {
        get => Get("screenpad", "opacity", AActionControl.DEFAULT_OPACITY);
        set => Set("screenpad", "opacity", value);
    }
    public float CameraSpeed
    {
        get => Get("screenpad", "camera_speed", 50.0f);
        set => Set("screenpad", "camera_speed", value);
    }

    public string Lang
    {
        get => Get("lang", "locale", string.Empty);
        set => Set("lang", "locale", value);
    }


    public string DetailsFilePath
    {
        get => Get("editor", "details_file_path", string.Empty);
        set => Set("editor", "details_file_path", value);
    }
} // AConfig

