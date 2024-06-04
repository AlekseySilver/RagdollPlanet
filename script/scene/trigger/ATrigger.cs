using Godot;

public abstract class ATrigger: Area
{
    public abstract void DoAction();

    public abstract bool IsActionFinished();

    public abstract string GetNameOverride();

    public APerson Player { get; private set; } = null;

    public void _on_area_body_entered(Node body)
    {
        if (IsPlayer(body, out var player))
        {
            player.SetBAction(this, true);
            this.Player = player;
        }
    }

    public void _on_area_body_exited(Node body)
    {
        if (IsPlayer(body, out var player))
        {
            this.Player = null;
            player.SetBAction(this, false);
        }
    }

    bool IsPlayer(Node node, out APerson player)
    {
        player = null;
        if (node is ABone3 bone3)
        {
            var p = bone3.Person;
            if (p.IsPlayer)
            {
                player = p;
                return true;
            }
        }
        return false;
    }
}
