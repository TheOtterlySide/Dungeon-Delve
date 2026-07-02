using DungeonDelve.Level.Common.Enum;
using DungeonDelve.Scenes;
using Godot;

namespace DungeonDelve.Level.Common.Item;

public partial class Item : Interactable
{
    [ExportGroup("Item")] 
    [Export]
    public string Name;
    [Export]
    public string Description;    
    [Export]
    public ItemTypeEnum Type;
    
    [Export] bool _moveable;
    [Export] private float _amplitude;
    [Export] private float _freq;
    [Export] private float _rotationSpeed;
    
    private float _time;
    public Vector3 _startPosition;
    [Signal]
    public delegate void ItemPickupSignalEventHandler();
    
    public void MoveAnimation(double delta)
    {
        if (!_moveable) return;

        _time += (float)delta;

        float offsetY = Mathf.Sin(_time * _freq) * _amplitude;
        RotateY(_rotationSpeed * (float)delta);

        Position = new Vector3(
            _startPosition.X ,
            _startPosition.Y + offsetY,
            _startPosition.Z
        );
        
    }
    
    protected override void OnPlayerInteracted(Player player)
    {
        QueueFree();
        player._itemHandler.AddItem(this);
    }
}