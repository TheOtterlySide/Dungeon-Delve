using Godot;
using System.Collections.Generic;
using System.Linq;
using DungeonDelve.Level.Common;
using DungeonDelve.Rooms.SPECIAL;

public partial class Room : Node3D
{
    public int Id           { get; set; }
    public int SocketCount  { get; set; }

    public List<RoomSocket>    RoomSockets          { get; private set; }
    public HashSet<Direction>  ConnectedDirections  { get; } = new();

    private const float ChangeDirProbability = 0.1f;

    public override void _Ready()
    {
        RoomSockets = GetNode("Sockets").GetChildren()
            .OfType<RoomSocket>()
            .ToList();

        SocketCount = RoomSockets.Count;
    }

    public RoomSocket GetAvailableRandomSocket(Direction lastDirection)
    {
        var available = RoomSockets.Where(x => !x.isUsed).ToList();
        if (available.Count == 0) return null;

        var weights = available
            .Select(x => x.GetDirection() == lastDirection ? ChangeDirProbability : 1f)
            .ToArray();

        return available[(int)new RandomNumberGenerator().RandWeighted(weights)];
    }

    public List<RoomSocket> GetAvailableSockets() =>
        RoomSockets.Where(x => !x.isUsed).ToList();

    public RoomSocket GetAvailableSocketOppositeSite(List<RoomSocket> sockets, Direction dir) =>
        sockets.FirstOrDefault(x => !x.isUsed && x.GetDirection() == x.GetOppositeDirection(dir));

    public float GetSizeOfRoom()
    {
        var shape = GetNode("StaticBody3D").GetChildren()
            .OfType<CollisionShape3D>()
            .FirstOrDefault()
            ?.GetShape() as BoxShape3D;

        GD.Print(shape?.Size + " Room: " + Name);
        return shape?.Size.X ?? 0f;
    }
}