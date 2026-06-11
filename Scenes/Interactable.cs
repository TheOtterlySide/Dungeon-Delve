using Godot;
using System;
using DungeonDelve.Level.Common.Enum;

public partial class Interactable : Node3D
{
    private Signal testSignal;
    private bool _connected;
    
    [Export] Label3D _label;
    [Export] InteractableTypeEnum  _interactableType;
    [Export] PackedScene _item;
    public void _on_area_3d_body_entered(Node3D body)
    {
        if (body.IsInGroup("Player") && body is Player player && !_connected)
        {
            player.canInteract = true;
            _connected = true;
            _label.Visible = true;
            player.PlayerInteracted += OnPlayerInteracted;
        }
    }

    private void OnPlayerInteracted()
    {
        GD.Print("Greetings!");
        AddChild(_item.Instantiate());
    }

    public void _on_area_3d_body_exited(Node3D body)
    {
        if (body.IsInGroup("Player") && body is Player player)
        {
            player.canInteract = false;
            _label.Visible = false;
            _connected = false;
        }
    }
}
