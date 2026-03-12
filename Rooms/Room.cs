using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelve.Level.Common;

public partial class Room : Node3D
{
    public int id { get; set; }
    public int socketCount { get; set; }
    public int usedSocketCount { get; set; }
    public List<DungeonDelve.Rooms.SPECIAL.RoomSocket> roomSockets = new List<DungeonDelve.Rooms.SPECIAL.RoomSocket>();

    public override void _Ready()
    {
       GetNode("Sockets").GetChildren().ToList().ForEach(x => roomSockets.Add((DungeonDelve.Rooms.SPECIAL.RoomSocket)x));
       socketCount = roomSockets.Count;
       base._Ready();
    }
    
    public DungeonDelve.Rooms.SPECIAL.RoomSocket GetAvailableSocket()
    {
        var result = roomSockets.FirstOrDefault(x => !x.isUsed);
        return result;
    }
    
    public DungeonDelve.Rooms.SPECIAL.RoomSocket GetAvailableRandomSocket(Random random)
    {
        var availableSockets = roomSockets.Where(x => !x.isUsed).ToList();
        var result = random.Next(availableSockets.Count);
        return availableSockets[result];
    }
    
    public List<DungeonDelve.Rooms.SPECIAL.RoomSocket> GetAvailableSocketList(List<DungeonDelve.Rooms.SPECIAL.RoomSocket> availableSockets)
    {
        return roomSockets;
    }
    
    public DungeonDelve.Rooms.SPECIAL.RoomSocket GetAvailableSocketOppositeSite(List<DungeonDelve.Rooms.SPECIAL.RoomSocket> availableSockets, Direction dir)
    {
        var result = availableSockets.FirstOrDefault(x => !x.isUsed && x.GetDirection() == x.GetOpposite(dir));
        return result;
    }
}
