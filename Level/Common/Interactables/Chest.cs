using DungeonDelve.Scenes;
using Godot;

namespace DungeonDelve.Level.Common.Interactables;


public partial class Chest : Interactable
{
    private bool _isOpened;
    private Node3D _itemNode;

    [Export] private PackedScene _item;

    protected override void OnPlayerInteracted()
    {
        if (_isOpened) return;

        _isOpened = true;
        _itemNode = (Node3D)_item.Instantiate();
        AddChild(_itemNode);
    }
}
