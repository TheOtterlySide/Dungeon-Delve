using Godot;
using System;

public partial class GameManager : Node
{
    [Export] private Player _player;
    [Export] private LevelBuilder _levelBuilder;

    [ExportGroup("Pause")] 
    private bool _pausePressed;


    public override void _Ready()
    {
        _levelBuilder.Level = 2;
        _levelBuilder.BuildInitial();   
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
