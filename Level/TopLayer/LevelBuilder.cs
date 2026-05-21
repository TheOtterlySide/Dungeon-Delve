using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelve.Level.Common;
using DungeonDelve.Rooms.SPECIAL;

public partial class LevelBuilder : Node
{
    // -------------------------------------------------------------------------
    // Exports
    // -------------------------------------------------------------------------

    [Export] private PackedScene _startRoom;
    [Export] private PackedScene _exitRoom;

    [ExportGroup("Paths")]
    [Export] private string _roomPath;
    [Export] private string _specialRoomPath;
    [Export] private string _bossRoomPath;

    // -------------------------------------------------------------------------
    // Config
    // -------------------------------------------------------------------------

    private const int SpecialRoomCount = 2;
    private const int BossRoomCount    = 1;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private readonly Random _random = new();

    private List<PackedScene> _normalRoomScenes  = new();
    private List<PackedScene> _specialRoomScenes = new();
    private List<PackedScene> _bossRoomScenes    = new();

    /// <summary>Rooms waiting to be placed on the grid.</summary>
    private List<Room> _roomPool = new();

    /// <summary>All rooms that have at least one socket (used when placing the exit).</summary>
    private List<Room> _allRooms = new();

    /// <summary>Sparse grid: (column, row) → Room.</summary>
    private readonly Dictionary<(int x, int y), Room> _grid = new();

    private Room _startRoomInstance;
    private Room _exitRoomInstance;

    public int Level { get; set; } = 0;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void Initial()
    {
        LoadRoomScenes();
        InstantiateRooms();
        PlaceRoomsOnGrid();
        PlaceWallsAndDoors();
    }

    private void PlaceWallsAndDoors()
    {
        foreach (var room in _allRooms)
        {
            var wallNode = room.GetNode("Walls");
            var doorNode = room.GetNode("Doors");
            var usedEnums = new List<Direction>();

            var used = room.UsedSockets;
            foreach (var socket in used)
            {
                if (socket == null) continue;
                var direction  = socket.GetDirection();
                usedEnums.Add(direction);
            }

            foreach (var direction in usedEnums)
            {
                var resultChildren = doorNode.GetChildren();
                var result = resultChildren.FirstOrDefault(x => x.Name.ToString().Contains(direction.ToString()));
                if  (result != null)
                {
                    //(Node3d)result.Visible = true;
                }
            }
        }
    }

    // -------------------------------------------------------------------------
    // Loading
    // -------------------------------------------------------------------------

    private void LoadRoomScenes()
    {
        LoadScenesFromDirectory(_roomPath,        RoomType.normal,  _normalRoomScenes);
        LoadScenesFromDirectory(_specialRoomPath, RoomType.special, _specialRoomScenes);
        LoadScenesFromDirectory(_bossRoomPath,    RoomType.boss,    _bossRoomScenes);
    }

    private static void LoadScenesFromDirectory(string path, RoomType _, List<PackedScene> target)
    {
        foreach (var file in ResourceLoader.ListDirectory(path))
        {
            if (file.EndsWith(".tscn"))
                target.Add(GD.Load<PackedScene>(path + file));
        }
    }

    // -------------------------------------------------------------------------
    // Instantiation
    // -------------------------------------------------------------------------

    private void InstantiateRooms()
    {
        _startRoomInstance = InstantiateAndAdd(_startRoom);
        _exitRoomInstance  = InstantiateAndAdd(_exitRoom);

        SpawnRandomRooms(_normalRoomScenes,  Level);
        SpawnRandomRooms(_specialRoomScenes, SpecialRoomCount);
        SpawnRandomRooms(_bossRoomScenes,    BossRoomCount);

        // Shuffle the pool with a single shared Random instance
        _roomPool = _roomPool.OrderBy(_ => _random.Next()).ToList();
    }

    private Room InstantiateAndAdd(PackedScene scene)
    {
        var room = (Room)scene.Instantiate();
        AddChild(room);
        return room;
    }

    private void SpawnRandomRooms(List<PackedScene> scenes, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var scene = scenes[_random.Next(scenes.Count)];
            _roomPool.Add(InstantiateAndAdd(scene));
        }
    }

    // -------------------------------------------------------------------------
    // Grid placement
    // -------------------------------------------------------------------------

    private void PlaceRoomsOnGrid()
    {
        var cursor    = new Vector2I(0, 0);
        var lastDir   = default(Direction);
        var current   = _startRoomInstance;

        PlaceInGrid(current, cursor);
        _allRooms.AddRange(_roomPool);
        _allRooms.AddRange(_roomPool);

        while (_roomPool.Count > 0)
        {
            var next       = _roomPool[0];
            var freeSocket = current.GetAvailableRandomSocket(lastDir);

            if (freeSocket == null) break;

            lastDir = freeSocket.GetDirection();

            var newCursor = MoveInGrid(cursor, lastDir);

            if (IsGridOccupied(newCursor))
            {
                freeSocket.Use();
                continue;
            }

            var oppositeSocket = next.GetAvailableSocketOppositeSite(next.RoomSockets, lastDir);
            freeSocket.Use();
            current.UsedSockets.Add(freeSocket);
            oppositeSocket?.Use();
            next.UsedSockets.Add(oppositeSocket);

            cursor = newCursor;
            PlaceInGrid(next, cursor);
            _roomPool.RemoveAt(0);

            current = AttachRoom(current, next, freeSocket);
        }

        LogUnplacedRooms();
        PlaceExitRoom();
        PrintGridDebug();
    }

    private void PlaceExitRoom()
    {
        var anchor = PickRoomForExit();
        if (anchor == null)
        {
            GD.PrintErr("LevelBuilder: No room available to attach the exit.");
            return;
        }

        foreach (var socket in anchor.GetAvailableSockets())
        {
            var candidate = MoveInGrid(GetGridCoords(anchor), socket.SocketDirection);

            if (IsGridOccupied(candidate)) continue;

            PlaceInGrid(_exitRoomInstance, candidate);
            AttachRoom(anchor, _exitRoomInstance, socket);
            return;
        }

        GD.PrintErr("LevelBuilder: Could not find a free slot for the exit room.");
    }

    // -------------------------------------------------------------------------
    // Grid helpers
    // -------------------------------------------------------------------------

    private void PlaceInGrid(Room room, Vector2I pos) =>
        _grid[(pos.X, pos.Y)] = room;

    private bool IsGridOccupied(Vector2I pos) =>
        _grid.ContainsKey((pos.X, pos.Y));

    private Vector2I GetGridCoords(Room room)
    {
        foreach (var kvp in _grid)
        {
            if (kvp.Value == room)
                return new Vector2I(kvp.Key.x, kvp.Key.y);
        }
        throw new InvalidOperationException($"Room '{room.Name}' is not registered in the grid.");
    }

    private Room GetRoomAt(int x, int y)
    {
        _grid.TryGetValue((x, y), out var room);
        return room;
    }

    private static Vector2I MoveInGrid(Vector2I pos, Direction dir) => dir switch
    {
        Direction.North => new Vector2I(pos.X,     pos.Y - 1),
        Direction.South => new Vector2I(pos.X,     pos.Y + 1),
        Direction.East  => new Vector2I(pos.X + 1, pos.Y),
        Direction.West  => new Vector2I(pos.X - 1, pos.Y),
        _               => pos
    };

    // -------------------------------------------------------------------------
    // Room attachment
    // -------------------------------------------------------------------------

    private Room AttachRoom(Room origin, Room newRoom, RoomSocket socket)
    {
        newRoom.GlobalPosition = origin.GlobalPosition + origin.GetSizeOfRoom() * GetDirectionVector(socket.SocketDirection);
        return newRoom;
    }

    private static Vector3 GetDirectionVector(Direction dir) => dir switch
    {
        Direction.North => Vector3.Forward,
        Direction.South => Vector3.Back,
        Direction.East  => Vector3.Right,
        Direction.West  => Vector3.Left,
        _               => Vector3.Zero
    };

    // -------------------------------------------------------------------------
    // Exit room selection
    // -------------------------------------------------------------------------

    private Room PickRoomForExit()
    {
        var candidates = _allRooms
            .Where(r => r.GetAvailableSockets().Count > 0)
            .ToList();

        return candidates.Count > 0
            ? candidates[_random.Next(candidates.Count)]
            : null;
    }

    // -------------------------------------------------------------------------
    // Debug
    // -------------------------------------------------------------------------

    private void LogUnplacedRooms()
    {
        GD.Print("LevelBuilder: Rooms remaining in pool after placement: ", _roomPool.Count);
        foreach (var r in _roomPool)
            GD.PrintErr("LevelBuilder: Room never placed – ", r.Name);
    }

    private void PrintGridDebug()
    {
        foreach (var entry in _grid)
            GD.Print(entry.Key, " : ", entry.Value.Name);
    }
}