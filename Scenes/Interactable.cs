using Godot;
using System;

public partial class Interactable : Node3D
{
    private Signal testSignal;
    
    [Export] Label3D _label;
    public void _on_area_3d_body_entered(Node3D body)
    {
        if (body.IsInGroup("Player") && body is Player player)
        {
            player.canInteract = true;
            _label.Visible = true;
        }
    }
    
    public void _on_area_3d_body_exited(Node3D body)
    {
        if (body.IsInGroup("Player") && body is Player player)
        {
            player.canInteract = false;
            _label.Visible = false;
        }
    }
}
