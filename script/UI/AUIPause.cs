using Godot;

public class AUIPause: ColorRect
{
	AScenePlay _scene => A.App.SceneManager as AScenePlay;
	AUIGameplayART _ui => A.App.UI.Gameplay as AUIGameplayART;

	public void _on_btn_resume_pressed()
	{
		_scene?.Resume();
	}
	public void _on_btn_control_pressed()
	{
		_ui?.ShowMenu_EditControl();
	}
	public void _on_btn_lang_pressed()
	{
		var lang = TranslationServer.GetLocale() == "en" ? "ru" : "en";
		TranslationServer.SetLocale(lang);
		A.Config.Lang = lang;
		A.Config.Save();
		_ui?.ShowMenu_Pause();
	}
	public void _on_btn_exit_pressed()
	{
		GetTree().Notification(MainLoop.NotificationWmQuitRequest);
	}
} // AUIPause
