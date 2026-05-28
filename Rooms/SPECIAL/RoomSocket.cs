using DungeonDelve.Level.Common;
using Godot;

namespace DungeonDelve.Rooms.SPECIAL;

public partial class RoomSocket : Marker3D
{
    [Export] public Direction SocketDirection; 
    public bool IsUsed;
    
    public void Use()
    {
        if (IsUsed) return;
        IsUsed = true;
    }

    public Direction GetDirection() => SocketDirection;
    
    public Direction GetOppositeDirection(Direction dir) => dir switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East  => Direction.West,
        Direction.West  => Direction.East,
        _ => dir
    };
}