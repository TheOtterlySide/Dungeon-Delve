using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class LevelBuilder : Node
{
    private List<PackedScene> _scenes = new List<PackedScene>();
    private List<PackedScene> _scenesToInstantiate = new List<PackedScene>();
    public int Level { get; set; }

    public LevelBuilder()
    {
        Level = 0;
    }


    public void BuildInitial()
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
                    _scenes.Add(room);
                }
            }
        }

        InstantiateRooms();
        MoveRooms();
    }

    private void MoveRooms()
    {
    }

    private void InstantiateRooms()
    {
        var random = new Random();

        for (int i = 0; i < Level; i++)
        {
            var sceneToInstantiate = _scenes[random.Next(0, _scenes.Count)];
            _scenesToInstantiate.Add(sceneToInstantiate);
        }

        foreach (var scene in _scenesToInstantiate)
        {
            var instance = (Node3D)scene.Instantiate();
            AddChild(instance);
            
            if (instance.GetName().ToString().Contains("03"))
            {
                instance.GlobalPosition = new Vector3(-60, 0, 0);
            }
        }
    }
}