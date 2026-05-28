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

    private readonly Dictionary<(int x, int y), Room> _grid = new();

    private Room _startRoomInstance;
    private Room _exitRoomInstance;

    public int Level { get; set; } = 0;

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public void Initial()
    {
        GD.Print("North: Blue, South: Green, East: Red, West: Yellow");
        LoadRoomScenes();
        InstantiateRooms();
        PlaceRoomsOnGrid();
        PlaceWallsAndDoors();
    }

    // -------------------------------------------------------------------------
    // Walls & Doors
    // -------------------------------------------------------------------------

    private void PlaceWallsAndDoors()
    {
        foreach (var room in _allRooms)
        {
            GD.Print($"=== Room: {room.Name} | Connected: {string.Join(", ", room.ConnectedDirections)} ===");

            var doorNode = room.GetNode("Doors");
            var wallNode = room.GetNode("Walls");

            foreach (Direction dir in Enum.GetValues<Direction>())
            {
                bool connected = room.ConnectedDirections.Contains(dir);
                var target = connected ? doorNode : wallNode;

                var child = target.GetChildren()
                    .FirstOrDefault(x => x.Name.ToString() == dir.ToString()) as Node3D;

                GD.Print($"Dir: {dir} | Connected: {connected} | Child: {child?.Name} | Target: {target.Name}");

                if (child == null) continue;

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
                target.Add(GD.Load<PackedScene>(path + file));
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
        _allRooms.Add(_startRoomInstance);
        _allRooms.AddRange(_roomPool);

        while (_roomPool.Count > 0)
        {
            var next = _roomPool[0];
            var freeSocket = current.GetAvailableRandomSocket(lastDir);

            if (freeSocket == null) break;

            lastDir = freeSocket.GetDirection();
            var newCursor = MoveInGrid(cursor, lastDir);

            if (IsGridOccupied(newCursor))
            {
                freeSocket.Use();
                continue;
            }

            count++;
            _roomPool[0].GetNodeOrNull<Label3D>("Debug Name")?.SetText(count.ToString());
            _roomPool[0].Name = "Room" + count;

            var oppositeSocket = next.GetAvailableSocketOppositeSite(next.RoomSockets, lastDir);
            freeSocket.Use();
            current.ConnectedDirections.Add(lastDir);

            if (oppositeSocket != null)
            {
                oppositeSocket.Use();
                next.ConnectedDirections.Add(Opposite(lastDir));
                GD.Print($"Room: {_roomPool[0].Name} | Free Socket: {freeSocket.SocketDirection} | Cursor: {newCursor} | Opposite: {oppositeSocket.GetParent().Name}");
            }

            cursor = newCursor;
            PlaceInGrid(next, cursor);
            _roomPool.RemoveAt(0);

            current = AttachRoom(current, next, freeSocket);
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
            GD.PrintErr("LevelBuilder: No room available to attach the exit.");
            return;
        }

        foreach (var socket in anchor.GetAvailableSockets())
        {
            var candidate = MoveInGrid(GetGridCoords(anchor), socket.SocketDirection);
            if (IsGridOccupied(candidate)) continue;

            anchor.ConnectedDirections.Add(socket.SocketDirection);
            _exitRoomInstance.ConnectedDirections.Add(Opposite(socket.SocketDirection));

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
            if (kvp.Value == room)
                return new Vector2I(kvp.Key.x, kvp.Key.y);

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
    // Debug
    // -------------------------------------------------------------------------

    private void LogUnplacedRooms()
    {
        GD.Print("LevelBuilder: Rooms remaining in pool: ", _roomPool.Count);
        foreach (var r in _roomPool)
            GD.PrintErr("LevelBuilder: Room never placed – ", r.Name);
    }

    private void PrintGridDebug()
    {
        foreach (var entry in _grid)
            GD.Print(entry.Key, " : ", entry.Value.Name);
    }
}