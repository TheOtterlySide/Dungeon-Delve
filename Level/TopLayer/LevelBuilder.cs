using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LevelBuilder : Node
{
    private List<PackedScene> _roomScenes = new();
    private List<PackedScene> _roomScenesToInstantiate = new();
    private List<Marker3D> _freeExits = new();
    
    private List<Node3D> _placedRooms = new();
    public int Level { get; set; }

    public LevelBuilder()
    {
        Level = 0;
    }


    public void Initial()
    {
        var roomLoader = ResourceLoader.ListDirectory("res://Rooms/NORMAL");
        var speciaLoader = ResourceLoader.ListDirectory("res://Rooms/SPECIAL");
        var bossLoader = ResourceLoader.ListDirectory("res://Rooms/BOSS");

        if (roomLoader.Length > 0)
        {
            foreach (var entity in roomLoader)
            {
                if (entity.Contains(".tscn"))
                {
                    var room = GD.Load<PackedScene>("res://Rooms/NORMAL/" + entity);
                    _roomScenes.Add(room);
                }
            }
        }

        GenerateRooms();
        AlignRooms();
    }

    private void AlignRooms()
    {
        var random = new Random();
        var currentRoom = _placedRooms[random.Next(0, _placedRooms.Count)];
        _placedRooms.Remove(currentRoom);
        var currentRoomEntry = currentRoom.GetNode<Node3D>("Entry");
        
        var currentExit = _freeExits[random.Next(0, _freeExits.Count)];
        _freeExits.Remove(currentExit);
        
        
        Basis targetBasis = currentExit.GlobalTransform.Basis * currentRoomEntry.GlobalTransform.Basis.Inverse();

    }

    private void GenerateRooms()
    {
        var random = new Random();

        for (int i = 0; i < Level; i++)
        {
            var sceneToInstantiate = _roomScenes[random.Next(0, _roomScenes.Count)];
            _roomScenesToInstantiate.Add(sceneToInstantiate);
        }

        foreach (var room in _roomScenesToInstantiate)
        {
            var instance = (Node3D)room.Instantiate();
            var Entry = GetNode<Marker3D>("Entry");
            
            foreach (var node in GetTree().GetNodesInGroup("Exit"))
            {
                if (node.IsAncestorOf(this))
                {
                    _freeExits.Add(node as Marker3D);
                }
            }
            
            AddChild(instance);
            _placedRooms.Add(instance);
        }
    }
}