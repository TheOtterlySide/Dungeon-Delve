using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelve.Level.Common;
using DungeonDelve.Rooms.SPECIAL;

public partial class LevelBuilder : Node
{
    private List<PackedScene> _roomScenesToInstantiate = new();
    private List<PackedScene> _specialRoomScenesToInstantiate = new();
    private List<PackedScene> _bossRoomScenesToInstantiate = new();

    private Random _random = new();
    private bool _isFirstRun = true;
    private List<Room> _roomPool = new();
    private Dictionary<(int x, int y), Room> grid = new();
    private Vector2 currentGridPosition = new Vector2(0, 0);

    [Export] private PackedScene _startRoom;
    [Export] private PackedScene _exitRoom;

    private Room _start;
    private Room _exit;

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
        var currentSocketDirection = Direction.North;
        var currentSocket = new DungeonDelve.Rooms.SPECIAL.RoomSocket();
        int lastIndex = 0;

        if (_isFirstRun)
        {
            _isFirstRun = false;
            var socketNodes = GetAllSockets(_start);
            if (socketNodes.Count > 0)
            {
                var startSocketPosition = socketNodes[0].Position;
                currentSocket = socketNodes[0];
                socketNodes[0].Use();
                currentSocketPosition = startSocketPosition;
                currentSocketDirection = socketNodes[0].GetDirection();
                grid[((int x, int y))(currentGridPosition.X, currentGridPosition.Y)] = _start as Room;
            }
        }

        int index = _random.Next(_roomPool.Count);
        var roomNode3D = _roomPool[0];
        var freeSocket = roomNode3D.GetAvailableSocket();


        currentGridPosition = MoveInGrid(currentGridPosition, currentSocketDirection);


        roomNode3D = AttachRoom(_start, roomNode3D, currentSocket.Name, true);
        var newroom = _roomPool[1];
        newroom = AttachRoom(roomNode3D, newroom, freeSocket.Name);  
        var newnewroom = _roomPool[2];
        freeSocket = newroom.GetAvailableSocket();
        newroom = AttachRoom(newroom, newnewroom, freeSocket.Name);
    }


    private List<DungeonDelve.Rooms.SPECIAL.RoomSocket> GetAllSockets(Node3D start)
    {
        var sockets = new List<DungeonDelve.Rooms.SPECIAL.RoomSocket>();
        var startSockets = start.GetNode<Node3D>("Sockets");
        var children = startSockets.GetChildren();
        foreach (var child in children)
        {
            if (child is DungeonDelve.Rooms.SPECIAL.RoomSocket socket)
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

        PrepareDefaultRooms();

        InstantiateRoomAndPlace(_roomScenesToInstantiate, Level);
        InstantiateRoomAndPlace(_specialRoomScenesToInstantiate, specialRoomCount);
        InstantiateRoomAndPlace(_bossRoomScenesToInstantiate, bossRoomCount);
    }

    private void PrepareDefaultRooms()
    {
        var instance = (Room)_startRoom.Instantiate();
        _start = instance;
        AddChild(instance);

        var endRoom = (Room)_exitRoom.Instantiate();
        _exit = endRoom;
        AddChild(endRoom);
        _roomPool.Add(endRoom);
    }

    private void InstantiateRoomAndPlace(List<PackedScene> scenesToInstantiate, int maxCount)
    {
        for (int i = 0; i < maxCount; i++)
        {
            var sceneToInstantiate = scenesToInstantiate[_random.Next(0, scenesToInstantiate.Count)];
            var instance = (Room)sceneToInstantiate.Instantiate();
            AddChild(instance);
            _roomPool.Add(instance);
        }
    }

    private Room GetRoomAt(int x, int y)
    {
        return grid[(x, y)];
    }

    private Vector2 MoveInGrid(Vector2 currentPosition, Direction dir)
    {
        return dir switch
        {
            Direction.North => new Vector2(currentPosition.X, currentPosition.Y - 1),
            Direction.South => new Vector2(currentPosition.X, currentPosition.Y + 1),
            Direction.East => new Vector2(currentPosition.X + 1, currentPosition.Y),
            Direction.West => new Vector2(currentPosition.X - 1, currentPosition.Y),
            _ => currentPosition
        };
    }

    Room AttachRoom(Room startRoom, Room newRoom, string startMarkerName, bool usex2 = false)
    {
        var startRoomPosition = startRoom.GlobalPosition;
        
        var start = startRoom.GetNode<Node3D>("Sockets");
        Marker3D startMarker = start.GetNode<Marker3D>(startMarkerName);
        var newMarker = newRoom.GetNode<Node3D>("Sockets").GetNode<Marker3D>(startMarkerName);
        
        if (usex2)
        {
            newRoom.GlobalPosition = 2 * startMarker.GlobalPosition;
        }
        else
        {
            Vector3 markerToRoom = newRoom.GlobalPosition - newMarker.GlobalPosition * 2;
            var test = startMarker as RoomSocket;
            var dir = test.GetDirection();
            switch (dir)
            {
                case Direction.North:
                    // -z
                    markerToRoom = new Vector3(0, 0, -2);
                    break;
                case Direction.South:
                    // +z
                    markerToRoom = new Vector3(0, 0, 2);
                    break;
                case Direction.East:
                    // +x
                    markerToRoom = new Vector3(2, 0, 0);
                    break;
                case Direction.West:
                    // -x
                    markerToRoom = new Vector3(-2, 0, 0);
                    break;
                default:
                    break;
            }
            var result = markerToRoom * GetDirectionVector(dir);
            newRoom.GlobalPosition = startRoomPosition + 2 * result;
        }

        return newRoom;
    }
    
    Vector3 GetDirectionVector(Direction dir)
    {
        return dir switch
        {
            Direction.North => new Vector3(0, 0, -1),
            Direction.South => new Vector3(0, 0, 1),
            Direction.East => new Vector3(1, 0, 0),
            Direction.West => new Vector3(-1, 0, 0),
            _ => new Vector3()
        };
    }
}