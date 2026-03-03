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

    [ExportGroup("Paths")]
    [Export] private string _roomPath;

    [Export] private string _specialRoomPath;
    [Export] private string _bossRoomPath;

    public int Level { get; set; }
    [Export] private float _roomSpacing = 4f;


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
        if (_isFirstRun)
        {
            _isFirstRun = false;
            grid[((int)currentGridPosition.X, (int)currentGridPosition.Y)] = _start;
        }

        var currentRoom = _start;

        while (_roomPool.Count > 0)
        {
            var nextRoom = _roomPool[0];

            var (freeSocket, freeGridPos) = FindFreeSocketAndPosition(currentRoom, currentGridPosition);

            if (freeSocket != null)
            {
                _roomPool.RemoveAt(0);
                freeSocket.Use();
            
                currentGridPosition = freeGridPos;
                grid[((int)currentGridPosition.X, (int)currentGridPosition.Y)] = nextRoom;
            
                currentRoom = AttachRoom(currentRoom, nextRoom, freeSocket.Name);
            }
            else
            {
                var (fallbackRoom, fallbackSocket, fallbackPos) = FindAnyFreePositionInGrid();

                if (fallbackSocket != null)
                {
                    _roomPool.RemoveAt(0);
                    fallbackSocket.Use();
                
                    currentRoom = fallbackRoom;
                    currentGridPosition = fallbackPos;
                    grid[((int)currentGridPosition.X, (int)currentGridPosition.Y)] = nextRoom;
                
                    currentRoom = AttachRoom(currentRoom, nextRoom, fallbackSocket.Name);
                }
                else
                {
                    //No Free Room
                    break;
                }
            }
        }

        // Exit-Room
        var (exitSocket, exitGridPos) = FindFreeSocketAndPosition(currentRoom, currentGridPosition);
        if (exitSocket != null)
        {
            exitSocket.Use();
            grid[((int)exitGridPos.X, (int)exitGridPos.Y)] = _exit;
            AttachRoom(currentRoom, _exit, exitSocket.Name);
        }
    }

    private (RoomSocket socket, Vector2 gridPos) FindFreeSocketAndPosition(Room room, Vector2 roomGridPos)
    {
        var sockets = GetAllSockets(room);
        foreach (var socket in sockets)
        {
            if (socket.isUsed) continue;
            var dir = socket.GetDirection();
            var nextPos = MoveInGrid(roomGridPos, dir);
            
            if (!IsGridOccupied(nextPos))
            {
                return (socket, nextPos);
            }
        }

        return (null, Vector2.Zero);
    }

    private (Room room, RoomSocket socket, Vector2 gridPos) FindAnyFreePositionInGrid()
    {
        foreach (var (pos, room) in grid)
        {
            var roomGridPos = new Vector2(pos.x, pos.y);
            var (socket, freePos) = FindFreeSocketAndPosition(room, roomGridPos);
            if (socket != null)
            {
                return (room, socket, freePos);
            }
        }

        return (null, null, Vector2.Zero);
    }

    private bool IsGridOccupied(Vector2 pos)
    {
        return grid.ContainsKey(((int)pos.X, (int)pos.Y));
    }


    private List<RoomSocket> GetAllSockets(Node3D start)
    {
        var sockets = new List<RoomSocket>();
        var startSockets = start.GetNode<Node3D>("Sockets");
        var children = startSockets.GetChildren();
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
        grid.TryGetValue((x, y), out var room);
        return room;
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

    Room AttachRoom(Room startRoom, Room newRoom, string startMarkerName)
    {
        var startMarker = startRoom.GetNode<Node3D>("Sockets").GetNode<Marker3D>(startMarkerName);

        newRoom.GlobalPosition = startRoom.GlobalPosition + _roomSpacing * GetDirectionVector(((RoomSocket)startMarker).GetDirection());

        return newRoom;
    }

    Vector3 GetDirectionVector(Direction dir) => dir switch
    {
        Direction.North => Vector3.Back, // (0, 0, -1)
        Direction.South => Vector3.Forward, // (0, 0,  1)
        Direction.East => Vector3.Right, // (1, 0,  0)
        Direction.West => Vector3.Left, // (-1, 0, 0)
        _ => Vector3.Zero
    };
}