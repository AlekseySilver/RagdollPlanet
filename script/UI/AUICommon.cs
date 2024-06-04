using Godot;
using System.Threading.Tasks;

public class AUICommon: Control, IUpdateable
{
    Label _txtDebug0;
    Label _txtDebug1;
    Label _txtDebug2;

    ADifferedObject<string> _differedSceneData = null;

    Panel _fader;
    ShaderMaterial _faderShader;
    readonly ALerperBase _faderLerper = new ALerperBase
    {
        Speed = 0.35f
    };

    public AUIGameplay Gameplay { get; private set; } = null;

    public ACursor Cursor { get; private set; } = null;

    public void Init()
    {
        Cursor = this.FirstChild<ACursor>(true);
        Cursor.Init();
        Cursor.HideOverride();

        _txtDebug0 = this.FirstChild<Label>(true, "txt_debug0");
        _txtDebug1 = this.FirstChild<Label>(true, "txt_debug1");
        _txtDebug2 = this.FirstChild<Label>(true, "txt_debug2");

        _fader = GetNode<Panel>("fader");
        _faderShader = _fader.Material as ShaderMaterial;
    } // _Ready

    public void UpdateOverride()
    {
        Gameplay?.UpdateOverride();

        if (_faderLerper.Update(A.TimeStep))
        {
            _faderShader.SetShaderParam("color", new Color(0.0f, 0.0f, 0.0f, _faderLerper.Source));

            if (_faderLerper.IsDone)
            {
                if (_faderLerper.Source == 0.0f)
                {
                    _fader.Visible = false;
                }
                _faderTCS?.TrySetResult(true);
            }
        } // fader
    } // UpdateOverride

    public void StartFade(bool fade, float speed = 0.35f)
    {
        _fader.Visible = true;
        _faderLerper.Speed = speed;
        if (fade)
        {
            _faderLerper.Source = 0.0f;
            _faderLerper.Target = 1f;
        }
        else
        { // unfade
            _faderLerper.Source = 1f;
            _faderLerper.Target = 0.0f;
        }

        _faderTCS?.TrySetResult(false);
    }

    TaskCompletionSource<bool> _faderTCS = null;
    public Task FadeAsync(bool fade, float speed = 0.5f)
    {
        StartFade(fade, speed);
        _faderTCS = new TaskCompletionSource<bool>();
        return _faderTCS.Task;
    }

    public void SetDebug0(string s) => _txtDebug0.Text = s;
    public void SetDebug1(string s) => _txtDebug1.Text = s;
    public void SetDebug2(string s) => _txtDebug2.Text = s;

    public async Task SetSceneUIAsync(string resource)
    {
        if (string.IsNullOrEmpty(resource))
        {
            Gameplay = null;
        }
        else
        {
            _differedSceneData = new ADifferedObject<string> { Value = resource };
            CallDeferred(nameof(DifferedSetSceneUI));
            await _differedSceneData.Task;
        }
    } // SetSceneUIAsync

    void DifferedSetSceneUI()
    {
        if (_differedSceneData == null)
            return;
        Gameplay?.Free();

        Gameplay = Asset.Instantiate<AUIGameplay>(_differedSceneData.Value, this);
        Gameplay.Init();

        _differedSceneData.Finish();
    } // DifferedSetSceneUI
}
