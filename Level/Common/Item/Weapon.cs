using DungeonDelve.Level.Common.Enum;
using DungeonDelve.Level.Common.Item;
using Godot;

public partial class Weapon : Item
{
    [ExportGroup("Weapon Part")]
    [Export] public int Damage { get; set; }

    [Export] public int Durability { get; set; }

    public override void _Ready()
    {
        _startPosition = Position;
    }

    [Export]
    public WeaponTypeEnum WeaponType { get; set; }

    public Weapon()
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAnimation(delta);
    }

  
}