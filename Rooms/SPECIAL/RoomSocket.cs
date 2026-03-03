using DungeonDelve.Level.Common;
using Godot;

namespace DungeonDelve.Rooms.SPECIAL;

public partial class RoomSocket : Marker3D
{
    [Export] public Direction SocketDirection; 
    public bool isUsed;
    
    public void Use()
    {
        if (isUsed) return;
        isUsed = true;
    }

    public Direction GetDirection() => SocketDirection;
    
    public Direction GetOpposite(Direction dir) => dir switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East  => Direction.West,
        Direction.West  => Direction.East,
        _ => dir
    };
}