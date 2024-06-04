using Godot;
using System;
using System.Collections.Generic;

public partial class AEditor
{
    public Dictionary<string, Action<AUIActionButton>> Actions { get; private set; }

    void InitActions()
    {
        Actions = new Dictionary<string, Action<AUIActionButton>> {
            { "\r\ndelete\r\n", DeleteSelected },
            { "\r\nUP align\r\n", UpAlign },
            { "\r\nreset scale\r\n", ResetScale },
            { "\r\nduplicate\r\n", Duplicate },
            { "\r\ndebug UI\r\n", DebugUI },
            { "\r\nedit anim\r\n", EditAnim },
        };
    }

    public void DeleteSelected(AUIActionButton _)
    {
        if (SelectedObject != null)
        {
            _editObjects.Remove(SelectedObject);
            _idxCollisions.Remove(SelectedObject.CollisionObject);
            Mode = EMode.CURSOR;
            SelectedObject.Root.QueueFree();
            SelectedObject = null;
        }
    }

    void UpAlign(AUIActionButton _)
    {
        SelectedObject?.Root.UpAlign();
    }

    void ResetScale(AUIActionButton _)
    {
        if (SelectedObject != null)
        {
            SelectedObject.Root.Scale = Vector3.One;

        }
    }

    void Duplicate(AUIActionButton _)
    {
        if (SelectedObject != null)
        {
            var old = SelectedObject.Root;
            var newi = old.Duplicate((int)(Node.DuplicateFlags.Signals | Node.DuplicateFlags.Groups | Node.DuplicateFlags.Scripts)) as Spatial;

            EditRoot.AddChild(newi);
            EditRoot.MoveChild(newi, 0);
            newi.GlobalTransform = old.GlobalTransform;
            Cursor?.SetGlobalPos(newi.GetGlobalPos());

            AddDetail(newi);

            Scene.SetEditor(true);
            Scene.Resume();
        }
    }

    void DebugUI(AUIActionButton _)
    {
        var ui = A.App.UI.FirstChild<AUIEditDebug>(true);
        if (ui != null)
            ui.Enabled = !ui.Enabled;
    }
    void EditAnim(AUIActionButton _)
    {
        Scene.Resume();

        A.App.UI.FirstChild<AUIGameplayART>(true).ShowMenu_AnimEdit();

        var ui = A.App.UI.FirstChild<AUIAnimEdit>(true);
        if (ui != null)
        {
            if (ui.Person == null)
            {
                ui.Person = Scene.Player;
                ui.Person?.DoAction(APerson.EGMAction.ANIM_EDIT);
            }
        }
    }
} // class AEditor
