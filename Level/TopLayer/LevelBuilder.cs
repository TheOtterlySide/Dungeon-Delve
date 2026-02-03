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

    [Export] private PackedScene _startRoom;
    [Export] private PackedScene _exitRoom;

    private Node3D _start;
    private Node3D _exit;

    [ExportGroup("Paths")] [Export] private string _roomPath;
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
        var currentSocketPosition = new Vector3();
        if (_isFirstRun)
        {
            _isFirstRun = false;
            var startSocketPosition = _start.GetNode<SOCKET>("EXIT");
            startSocketPosition.Use();
            currentSocketPosition = startSocketPosition.GlobalPosition;
        }


        while (_placedRooms.Count > 0)
        {
            int index = _random.Next(_placedRooms.Count);
            var room = _placedRooms[index];
            _placedRooms.RemoveAt(index);
            room.GlobalPosition = currentSocketPosition;
            GD.Print(room.Name);
            var exitSocketPosition = room.GetNode<SOCKET>("EXIT");
            exitSocketPosition.Use();
            
            currentSocketPosition = exitSocketPosition.GlobalPosition;
        }
        
        var finalexitSocketPosition = _exit.GetNode<SOCKET>("EXIT");
        finalexitSocketPosition.Use();
        currentSocketPosition = finalexitSocketPosition.GlobalPosition;


        
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
        _exit = endRoom;
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