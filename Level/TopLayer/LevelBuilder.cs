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
    private List<Node3D> _roomPool = new();
    private Dictionary<(int x, int y), Room> grid = new();
    
    [Export] private PackedScene _startRoom;
    [Export] private PackedScene _exitRoom;

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
        
        var currentSocketPosition = new Vector3();
        int lastIndex = 0;
        if (_isFirstRun)
        {
            _isFirstRun = false;
            var socketNodes = GetAllSockets(_start);
            if (socketNodes.Count > 0)
            {
                var startSocketPosition = socketNodes[0].Position;
                socketNodes[0].Use();
                currentSocketPosition = startSocketPosition;
                grid[(0, 0)] = _start as Room;
            }
        }

        int index = _random.Next(_roomPool.Count);
        var room = _roomPool[0];
        AttachRoom(_start, room, "ENTRY", "EXIT");
        var newroom = _roomPool[1];
        AttachRoom(room,newroom, "ENTRY", "EXIT");
    }

    private RoomSocket GetAvailableSocket(List<RoomSocket> availableSockets)
    {
        var result = availableSockets.FirstOrDefault(x => !x.isUsed);
        return result;
    }
    
    private RoomSocket GetAvailableSocketOppositeSite(List<RoomSocket> availableSockets, Direction dir)
    {
        var result = availableSockets.FirstOrDefault(x => !x.isUsed && x.GetDirection() == x.GetOpposite(dir));
        return result;
    }

    private List<RoomSocket> GetAllSockets(Node3D start)
    {
        var sockets = new List<RoomSocket>();

        var children = start.GetChildren();
        foreach (var child in children)
        {
            if (child is RoomSocket socket)
            {
                sockets.Add(socket);
            }
        }

        return sockets;
    }

    private void GenerateRooms()
    {
        var specialRoomCount = 0;
        var bossRoomCount = 0;
        int idRun;

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
        _roomPool.Add(endRoom);
    }

    private void InstantiateRoomAndPlace(List<PackedScene> scenesToInstantiate, int maxCount)
    {
        for (int i = 0; i < maxCount; i++)
        {
            var sceneToInstantiate = scenesToInstantiate[_random.Next(0, scenesToInstantiate.Count)];
            var instance = (Node3D)sceneToInstantiate.Instantiate();
            AddChild(instance);
            _roomPool.Add(instance);
        }
    }
    
    private Room GetRoomAt(int x, int y)
    {
        return grid[(x, y)];
    }
    
    Vector3 GetRoomSize(Node3D roomInstance)
    {
        var meshInstance = roomInstance.GetNode<MeshInstance3D>("MeshInstance3D");
        var aabb = meshInstance.GetAabb();

        return aabb.Size;
    }
    
    void AttachRoom(Node3D startRoom, Node3D newRoom, string startMarkerName, string newMarkerName)
    {
        Marker3D startMarker = startRoom.GetNode<Marker3D>(startMarkerName);
        Marker3D newMarker = newRoom.GetNode<Marker3D>(newMarkerName);
        

        // Berechne Offset zwischen Marker und Raum Pivot
        Vector3 markerToRoom = newRoom.GlobalPosition - newMarker.GlobalPosition;
        // Setze neuen Raum
        newRoom.GlobalPosition = startMarker.GlobalPosition + markerToRoom;

    }
    
}