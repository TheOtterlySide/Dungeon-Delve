using DungeonDelve.Level.Common;
using Godot;

public partial class Potion : Item
{
    [ExportGroup("Potion Part")]
    [Export]
    public int HealthRestore { get; set; }

    [Export]
    public int ManaRestore { get; set; }

    [Export]
    public int Duration { get; set; }

    [Export]
    public bool IsPoisonous { get; set; }
    public Potion()
    {
    }
}