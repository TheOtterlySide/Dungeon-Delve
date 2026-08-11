using Godot;
using System;

public partial class GameManager : Node
{
    [Export] private Player _player;
    [Export] private DungeonDelve.Level.Handler.LevelHandler _levelHandler;
    

    [ExportGroup("Pause")] 
    private bool _pausePressed;


    public override void _Ready()
    {
        _levelHandler.Level = 10;
        _levelHandler.Initial();   
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }
}
