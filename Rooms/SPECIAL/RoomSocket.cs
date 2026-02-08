using Godot;
using System;
using DungeonDelve.Level.Common;

public partial class RoomSocket : Marker3D
{
    private Direction Direction;
    public bool isUsed;
    
    public void Use()
    {
        if (isUsed)
        {
            return;
        }

        isUsed = true;
    }

    public Direction GetDirection()
    {
        var globalPosition = GlobalPosition;

        if (Mathf.Abs(globalPosition.X) > Mathf.Abs(globalPosition.Z))
        {
            return globalPosition.X > 0 ? Direction.West : Direction.East;
        }

        return globalPosition.Z > 0 ? Direction.South : Direction.North;
    }
    
    public Direction GetOpposite(Direction dir)
    {
        return dir switch
        {
            Direction.North => Direction.South,
            Direction.South => Direction.North,
            Direction.East  => Direction.West,
            Direction.West  => Direction.East,
            _ => dir
        };
    }
    
}
