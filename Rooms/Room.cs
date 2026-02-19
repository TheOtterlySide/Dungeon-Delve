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
    public List<RoomSocket> roomSockets = new List<RoomSocket>();

    public override void _Ready()
    {
       GetNode("Sockets").GetChildren().ToList().ForEach(x => roomSockets.Add((RoomSocket)x));
       socketCount = roomSockets.Count;
       base._Ready();
    }
    
    public RoomSocket GetAvailableSocket()
    {
        var result = roomSockets.FirstOrDefault(x => !x.isUsed);
        return result;
    }
    
    public List<RoomSocket> GetAvailableSocketList(List<RoomSocket> availableSockets)
    {
        return roomSockets;
    }
    
    public RoomSocket GetAvailableSocketOppositeSite(List<RoomSocket> availableSockets, Direction dir)
    {
        var result = availableSockets.FirstOrDefault(x => !x.isUsed && x.GetDirection() == x.GetOpposite(dir));
        return result;
    }
}
