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
    private Vector2 currentGridCursorPosition = new Vector2(0, 0);

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
        var currentRoom = _start;
        SetRoomInGrid(currentRoom, currentGridCursorPosition);

        while (_roomPool.Count > 0)
        {
            var nextRoom = _roomPool[0];
            var freeSocket = currentRoom.GetAvailableSocket();
            
            if (freeSocket != null)
            {
                var oppositeSocket = nextRoom.GetAvailableSocketOppositeSite(nextRoom.roomSockets, freeSocket.GetDirection());
                freeSocket.Use();
                oppositeSocket.Use();
                _roomPool.RemoveAt(0);
                if (!IsGridOccupied(GetDirectionVector(freeSocket.SocketDirection)))
                {
                    currentGridCursorPosition = MoveInGrid(currentGridCursorPosition, freeSocket.GetDirection());
                    SetRoomInGrid(nextRoom, currentGridCursorPosition);
                    currentRoom = AttachRoom(currentRoom, nextRoom, freeSocket);
                }
            }
        }

        // Exit-Room
        var freeExitSocket = currentRoom.GetAvailableSocket();
        currentGridCursorPosition = MoveInGrid(currentGridCursorPosition, freeExitSocket.GetDirection());
        SetRoomInGrid(_exit, currentGridCursorPosition);
        var room = AttachRoom(currentRoom, _exit, freeExitSocket);
    }

    private void SetRoomInGrid(Room currentRoom, Vector2 currentGridPosition)
    {
        grid[((int)currentGridPosition.X, (int)currentGridPosition.Y)] = currentRoom;
    }

    private bool IsGridOccupied(Vector3 pos)
    {
        return grid.ContainsKey(((int)currentGridCursorPosition.X + (int)pos.X, (int)currentGridCursorPosition.Y + (int)pos.Z));
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

    Room AttachRoom(Room startRoom, Room newRoom, RoomSocket socket)
    {
        newRoom.GlobalPosition = startRoom.GlobalPosition + _roomSpacing * GetDirectionVector(socket.SocketDirection);
        return newRoom;
    }

    Vector3 GetDirectionVector(Direction dir) => dir switch
    {
        Direction.North => Vector3.Forward, // (0, 0, 1)
        Direction.South => Vector3.Back, // (0, 0,  -1)
        Direction.East => Vector3.Right, // (1, 0,  0)
        Direction.West => Vector3.Left, // (-1, 0, 0)
        _ => Vector3.Zero
    };
}