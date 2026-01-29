using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelve.Level.Common;

public partial class LevelBuilder : Node
{
    private List<PackedScene> _roomScenesToInstantiate = new();
    private List<PackedScene> _specialRoomScenesToInstantiate = new();
    private List<PackedScene> _bossRoomScenesToInstantiate = new();

    private Random _random = new();
    private bool _isFirstRun = true;
    private List<Node3D> _placedRooms = new();

    [Export]
    private PackedScene _startRoom;
    [Export]
    private PackedScene _exitRoom;
    
    private Node3D _start;
    private Node3D _exit;

    [ExportGroup("Paths")] 
    [Export] private string _roomPath;
    [Export] private string _specialRoomPath;
    [Export] private string _bossRoomPath;

    public int Level { get; set; }

    public LevelBuilder()
    {
        Level = 0;
    }


    public void Initial()
    {
        var normalRoamLoader = ResourceLoader.ListDirectory(_roomPath);
        var speciaLoader = ResourceLoader.ListDirectory(_specialRoomPath);
        var bossLoader = ResourceLoader.ListDirectory(_bossRoomPath);


        GetRoomScene(normalRoamLoader, _roomPath, RoomType.normal);
        GetRoomScene(speciaLoader, _specialRoomPath, RoomType.special);
        GetRoomScene(bossLoader, _bossRoomPath, RoomType.boss);

        GenerateRooms();
        AlignRooms();
    }

    private void GetRoomScene(string[] roomLoader, string path, RoomType type)
    {
        var toAddList = new List<PackedScene>();

        switch (type)
        {
            case RoomType.normal:
                toAddList = _roomScenesToInstantiate;
                break;
            case RoomType.special:
                toAddList = _specialRoomScenesToInstantiate;
                break;
            case RoomType.boss:
                toAddList = _bossRoomScenesToInstantiate;
                break;
            default:
                break;
        }

        if (roomLoader.Length > 0)
        {
            foreach (var entity in roomLoader)
            {
                if (entity.Contains(".tscn"))
                {
                    var room = GD.Load<PackedScene>(path + entity);
                    toAddList.Add(room);
                }
            }
        }
    }


    private void AlignRooms()
    {
        var currentExitPosition = new Vector3();
        if (_isFirstRun)
        {
            _isFirstRun = false;
            var startExitPosition = _start.GetNode<Marker3D>("EXIT");
            currentExitPosition = startExitPosition.GlobalPosition;
        }

        var nextRoom = _placedRooms[_random.Next(0, _placedRooms.Count)];
        _placedRooms.Remove(nextRoom);

        for (int i = 0; i < _placedRooms.Count; i++)
        {
            nextRoom.Position = currentExitPosition;
            nextRoom = _placedRooms[_random.Next(0, _placedRooms.Count)];
            currentExitPosition = nextRoom.GetNode<Marker3D>("EXIT").GlobalPosition;
            _placedRooms.Remove(nextRoom);
        }
    }

    private void GenerateRooms()
    {
        var specialRoomCount = Level / 2;
        var bossRoomCount = 1;

        PrepareDefaultRooms();

        InstantiateRoomAndPlace(_roomScenesToInstantiate, Level);
        InstantiateRoomAndPlace(_specialRoomScenesToInstantiate, specialRoomCount);
        InstantiateRoomAndPlace(_bossRoomScenesToInstantiate, bossRoomCount);
    }

    private void PrepareDefaultRooms()
    {
        var instance = (Node3D)_startRoom.Instantiate();
        _start = instance;
        AddChild(instance);

        var endRoom = (Node3D)_exitRoom.Instantiate();
        AddChild(endRoom);
        _placedRooms.Add(endRoom);
    }

    private void InstantiateRoomAndPlace(List<PackedScene> scenesToInstantiate, int maxCount)
    {
        for (int i = 0; i < maxCount; i++)
        {
            var sceneToInstantiate = scenesToInstantiate[_random.Next(0, scenesToInstantiate.Count)];
            var instance = (Node3D)sceneToInstantiate.Instantiate();
            AddChild(instance);
            _placedRooms.Add(instance);
        }
    }
}