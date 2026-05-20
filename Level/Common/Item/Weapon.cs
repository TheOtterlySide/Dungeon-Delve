
using DungeonDelve.Level.Common;
using DungeonDelve.Level.Common.Enum;
using Godot;

public partial class Weapon : Item
{
    [ExportGroup("Weapon Part")] 
    [Export]
    public int Damage { get; set; }
    [Export]
    public int Durability { get; set; }
    
    [Export]
    public WeaponTypeEnum WeaponType { get; set; }

    public Weapon()
    {
    }
}