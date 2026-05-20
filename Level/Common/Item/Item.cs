using DungeonDelve.Level.Common.Enum;
using Godot;

namespace DungeonDelve.Level.Common;

public partial class Item : Node3D
{
    [ExportGroup("Item")] 
    [Export]
    public string Name;
    [Export]
    public string Description;    
    [Export]
    public ItemTypeEnum Type;
}