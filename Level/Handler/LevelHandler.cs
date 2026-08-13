using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DungeonDelve.Level.Common;
using DungeonDelve.Rooms.SPECIAL;
using Godot;

namespace DungeonDelve.Level.Handler;

public partial class LevelHandler : Node
{
    // -------------------------------------------------------------------------
    // Exports
    // -------------------------------------------------------------------------

    [Export] private PackedScene _startRoom;
    [Export] private PackedScene _exitRoom;
    [Export] private PackedScene _chestScene;

    [ExportGroup("Paths")]
    [Export] private string _roomPath;

    [Export] private string _specialRoomPath;
    [Export] private string _bossRoomPath;

    private Room _testRoom;

    private bool firstRun = true;
    // -------------------------------------------------------------------------
    // Config
    // -------------------------------------------------------------------------

    private const int SpecialRoomCount = 2;
    private const int BossRoomCount = 1;

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------

    private readonly Random _random = new();

    private List<PackedScene> _normalRoomScenes = new();
    private List<PackedScene> _specialRoomScenes = new();
    private List<PackedScene> _bossRoomScenes = new();

    private List<Room> _roomPool = new();
    private List<Room> _allRooms = new();
    private List<Node3D> _chestList = new();
    private Dictionary<(int x, int y), Room> _grid = new();

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

        SpawnChests();
    }

    public void ReloadLevel()
    {
        ResetFields();
        Initial();
    }

    // -------------------------------------------------------------------------
    // Chests & Enemies
    // -------------------------------------------------------------------------
    private void SpawnChests()
    {
        var areaNode = _testRoom.GetNode("Area Stuff");
        var spawnArea = areaNode.FindChild("SpawnArea", true, false);
        var spawnAreaRight = spawnArea as Node3D;

        if (spawnAreaRight == null)
        {
            DebugManager.Instance.LogError("Spawn area not found or is not a Node3D.");
            return;
        }

        var randomX = spawnAreaRight.GlobalPosition.X + (float)(_random.NextDouble() - 0.5) * spawnAreaRight.Scale.X;
        var randomZ = spawnAreaRight.GlobalPosition.Z + (float)(_random.NextDouble() - 0.5) * spawnAreaRight.Scale.Z;

        var spawnPosition = new Vector3(randomX, spawnAreaRight.GlobalPosition.Y, randomZ);
        var instant = (Node3D)_chestScene.Instantiate();

        _chestList.Add(instant);
        AddChild(instant);
        instant.GlobalPosition = spawnPosition;
    }

    // -------------------------------------------------------------------------
    // Walls & Doors
    // -------------------------------------------------------------------------

    private void PlaceWallsAndDoors()
    {
        foreach (var room in _allRooms)
        {
            DebugManager.Instance.Log($"=== Room: {room.Name} | Connected: {string.Join(", ", room.ConnectedDirections)} ===");

            var doorNode = room.GetNode("Doors");
            var wallNode = room.GetNode("Walls");

            foreach (Direction dir in Enum.GetValues<Direction>())
            {
                bool connected = room.ConnectedDirections.Contains(dir);
                var target = connected ? doorNode : wallNode;

                var child = target.GetChildren()
                    .FirstOrDefault(x => x.Name.ToString() == dir.ToString()) as Node3D;

                DebugManager.Instance.Log($"Dir: {dir} | Connected: {connected} | Child: {child?.Name} | Target: {target.Name}");

                if (child == null)
                {
                    continue;
                }

                child.Visible = true;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Loading
    // -------------------------------------------------------------------------

    private void LoadRoomScenes()
    {
        LoadScenesFromDirectory(_roomPath, _normalRoomScenes);
        LoadScenesFromDirectory(_specialRoomPath, _specialRoomScenes);
        LoadScenesFromDirectory(_bossRoomPath, _bossRoomScenes);
    }

    private static void LoadScenesFromDirectory(string path, List<PackedScene> target)
    {
        foreach (var file in ResourceLoader.ListDirectory(path))
        {
            if (file.EndsWith(".tscn"))
            {
                target.Add(GD.Load<PackedScene>(path + file));
            }
        }
    }

    // -------------------------------------------------------------------------
    // Instantiation
    // -------------------------------------------------------------------------

    private void InstantiateRooms()
    {
        _startRoomInstance = InstantiateAndAdd(_startRoom);
        _exitRoomInstance = InstantiateAndAdd(_exitRoom);

        SpawnRandomRooms(_normalRoomScenes, Level);
        SpawnRandomRooms(_specialRoomScenes, SpecialRoomCount);
        SpawnRandomRooms(_bossRoomScenes, BossRoomCount);

        _roomPool = _roomPool.OrderBy(_ => _random.Next()).ToList();
    }

    private Room InstantiateAndAdd(PackedScene scene)
    {
        var room = (Room)scene.Instantiate();
        AddChild(room);
        room.Init();

        //TODO: DEBUG, delete laterz
        if (room.Name == "Room01" && firstRun)
        {
            _testRoom = room;
            firstRun = false;
        }

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
        var cursor = new Vector2I(0, 0);
        var lastDir = default(Direction);
        var current = _startRoomInstance;
        var count = 0;

        PlaceInGrid(current, cursor);
        DebugManager.Instance.Log($"=== StartRoom: {current.Name} | Sockets: {current.RoomSockets.Count} ===");
        DebugManager.Instance.Log($"StartRoom sockets total: {_startRoomInstance.RoomSockets.Count}");
        DebugManager.Instance.Log($"StartRoom available sockets: {_startRoomInstance.GetAvailableSockets().Count}");

        foreach (var s in _startRoomInstance.RoomSockets)
            DebugManager.Instance.Log($"  Socket: {s.SocketDirection} | IsUsed: {s.IsUsed}");
        _allRooms.Add(_startRoomInstance);
        _allRooms.AddRange(_roomPool);

        var placedRooms = new List<Room> { _startRoomInstance };

        while (_roomPool.Count > 0)
        {
            var freeSocket = current.GetAvailableRandomSocket(lastDir);

            DebugManager.Instance.Log($"current: {current.Name} | available: {current.GetAvailableSockets().Count} | lastDir: {lastDir}");
            DebugManager.Instance.Log($"freeSocket: {freeSocket?.SocketDirection.ToString() ?? "NULL"}");

            if (freeSocket == null)
            {
                var fallback = placedRooms
                    .Where(r => r.GetAvailableSockets().Count > 0)
                    .OrderBy(_ => _random.Next())
                    .FirstOrDefault();

                if (fallback == null)
                {
                    DebugManager.Instance.LogError("LevelBuilder: No sockets left anywhere – aborting.");
                    break;
                }

                current = fallback;
                cursor = GetGridCoords(current);
                lastDir = default;
                continue;
            }

            lastDir = freeSocket.GetDirection();
            var newCursor = MoveInGrid(cursor, lastDir);

            if (!CanPlaceRoom(_roomPool[0], newCursor, lastDir))
            {
                var alternativeIndex = _roomPool
                    .FindIndex(r => CanPlaceRoom(r, newCursor, lastDir));

                if (alternativeIndex == -1)
                {
                    freeSocket.Use();
                    continue;
                }

                (_roomPool[0], _roomPool[alternativeIndex]) = (_roomPool[alternativeIndex], _roomPool[0]);
            }

            count++;
            var next = _roomPool[0];
            next.GetNodeOrNull<Label3D>("Debug Name")?.SetText(count.ToString());
            next.Name = "Room" + count;

            var oppositeSocket = next.GetAvailableSocketOppositeSite(next.RoomSockets, lastDir);
            freeSocket.Use();
            current.ConnectedDirections.Add(lastDir);

            if (oppositeSocket != null)
            {
                oppositeSocket.Use();
                next.ConnectedDirections.Add(Opposite(lastDir));
            }

            cursor = newCursor;
            PlaceInGrid(next, cursor);
            _roomPool.RemoveAt(0);

            current = AttachRoom(current, next, freeSocket);
            placedRooms.Add(current);
        }

        LogUnplacedRooms();
        PlaceExitRoom();
        _allRooms.Add(_exitRoomInstance);
        PrintGridDebug();
    }

    private void PlaceExitRoom()
    {
        var anchor = PickRoomForExit();
        if (anchor == null)
        {
            DebugManager.Instance.LogError("[Error]: PickRoomForExit() returned null.");
            return;
        }

        foreach (var socket in anchor.GetAvailableSockets())
        {
            var candidate = MoveInGrid(GetGridCoords(anchor), socket.SocketDirection);
            if (IsGridOccupied(candidate)) continue;

            var opposite = Opposite(socket.SocketDirection);
            var exitSocket = _exitRoomInstance.RoomSockets
                .FirstOrDefault(s => s.SocketDirection == opposite && !s.IsUsed);

            if (exitSocket == null)
            {
                continue;
            }

            anchor.ConnectedDirections.Add(socket.SocketDirection);
            _exitRoomInstance.ConnectedDirections.Add(Opposite(socket.SocketDirection));

            PlaceInGrid(_exitRoomInstance, candidate);
            AttachRoom(anchor, _exitRoomInstance, socket);
            return;
        }

        DebugManager.Instance.LogError("LevelBuilder: Could not find a free slot for the exit room.");
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
            {
                return new Vector2I(kvp.Key.x, kvp.Key.y);
            }
        }

        throw new InvalidOperationException($"Room '{room.Name}' is not registered in the grid.");
    }

    private static Vector2I MoveInGrid(Vector2I pos, Direction dir) => dir switch
    {
        Direction.North => new Vector2I(pos.X, pos.Y - 1),
        Direction.South => new Vector2I(pos.X, pos.Y + 1),
        Direction.East => new Vector2I(pos.X + 1, pos.Y),
        Direction.West => new Vector2I(pos.X - 1, pos.Y),
        _ => pos
    };

    private static Direction Opposite(Direction dir) => dir switch
    {
        Direction.North => Direction.South,
        Direction.South => Direction.North,
        Direction.East => Direction.West,
        Direction.West => Direction.East,
        _ => dir
    };

    // -------------------------------------------------------------------------
    // Room attachment
    // -------------------------------------------------------------------------

    private Room AttachRoom(Room origin, Room newRoom, RoomSocket socket)
    {
        newRoom.GlobalPosition = origin.GlobalPosition +
                                 origin.GetSizeOfRoom() * GetDirectionVector(socket.SocketDirection);
        return newRoom;
    }

    private bool CanPlaceRoom(Room next, Vector2I targetPos, Direction incomingDir)
    {
        if (IsGridOccupied(targetPos))
        {
            return false;
        }

        var opposite = Opposite(incomingDir);
        var hasSocket = next.RoomSockets
            .Any(s => s.SocketDirection == opposite && !s.IsUsed);

        return hasSocket;
    }

    private static Vector3 GetDirectionVector(Direction dir) => dir switch
    {
        Direction.North => Vector3.Back,
        Direction.South => Vector3.Forward,
        Direction.East => Vector3.Right,
        Direction.West => Vector3.Left,
        _ => Vector3.Zero
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
    // Reload
    // -------------------------------------------------------------------------
    private void ResetFields()
    {
        foreach (var room in _allRooms) room.Free();                                                                                                                                                                            
        foreach (var chest in _chestList) chest.Free();                                                                                                                                                                         
        _chestList = new();  
        
        firstRun = true;
        _normalRoomScenes = new();
        _specialRoomScenes = new();
        _bossRoomScenes = new();

        _roomPool = new();
        _allRooms = new();
        _grid = new();
    }

    // -------------------------------------------------------------------------
    // Debug
    // -------------------------------------------------------------------------

    private void LogUnplacedRooms()
    {
        DebugManager.Instance.LogError("LevelBuilder: Rooms remaining in pool: " + _roomPool.Count);
        foreach (var r in _roomPool)
            DebugManager.Instance.LogError("LevelBuilder: Room never placed – " + r.Name);
    }

    private void PrintGridDebug()
    {
        foreach (var entry in _grid)
            DebugManager.Instance.Log($"LevelBuilder: {entry.Key} - {entry.Value}");
    }
}