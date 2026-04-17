using Godot;
using System.Collections.Generic;
using System.Linq;
using DungeonDelve.Level.Common;
using DungeonDelve.Rooms.SPECIAL;

public partial class Room : Node3D
{
    public int Id { get; set; }
    public int SocketCount { get; set; }
    public int UsedSocketCount { get; set; }

    public List<RoomSocket> RoomSockets = new List<RoomSocket>();
    private float _probabilityToChangeDirection = 0.1f;

    public override void _Ready()
    {
        GetNode("Sockets").GetChildren().ToList().ForEach(x => RoomSockets.Add((RoomSocket)x));
        SocketCount = RoomSockets.Count;
        base._Ready();
    }

    public RoomSocket GetAvailableSocket()
    {
        var result = RoomSockets.FirstOrDefault(x => !x.isUsed);
        return result;
    }

    public RoomSocket GetAvailableRandomSocket(Direction lastDirection)
    {
        var availableSockets = RoomSockets.Where(x => !x.isUsed).ToList();
        if (availableSockets.Count == 0) return null;
        var weights = availableSockets.Select(x => x.GetDirection() == lastDirection ? _probabilityToChangeDirection : 1.0f).ToArray();
        return availableSockets[(int)new RandomNumberGenerator().RandWeighted(weights)];
    }

    public List<RoomSocket> GetAvailableSockets()
    {
        return RoomSockets.Where(x => !x.isUsed).ToList();
    }
    
    public List<RoomSocket> GetAvailableSocketList()
    {
        return RoomSockets;
    }

    public RoomSocket GetAvailableSocketOppositeSite(List<RoomSocket> availableSockets, Direction dir)
    {
        var result = availableSockets.FirstOrDefault(x => !x.isUsed && x.GetDirection() == x.GetOppositeDirection(dir));
        return result;
    }
}