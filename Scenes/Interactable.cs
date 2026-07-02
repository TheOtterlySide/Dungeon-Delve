using DungeonDelve.Level.Common.Enum;
using Godot;

namespace DungeonDelve.Scenes;

public partial class Interactable : Node3D
{
    private bool _connected;

    [Export] protected Label3D _label;
    [Export] protected InteractableTypeEnum _interactableType;

    public void _on_interact_area_body_entered(Node3D body)
    {
        if (body.IsInGroup("Player") && body is Player player && !_connected)
        {
            player.canInteract = true;
            _connected = true;
            _label.Visible = true;
            player.PlayerInteracted += () => OnPlayerInteracted(player);
        }
    }

    public void _on_interact_area_body_exited(Node3D body)
    {
        if (body.IsInGroup("Player") && body is Player player)
        {
            player.canInteract = false;
            _label.Visible = false;
            _connected = false;
            player.PlayerInteracted -= () => OnPlayerInteracted(player); 
        }
    }

    protected virtual void OnPlayerInteracted(Player player) { }
}