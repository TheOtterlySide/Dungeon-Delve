using Godot;
using System;

namespace DungeonDelve.Level.Manager;
public partial class GameManager : Node
{
    [Export] private Player _player;
    [Export] private DungeonDelve.Level.Handler.LevelHandler _levelHandler;
    
    [ExportGroup("Debug")]
    [Export] bool _debugMode;
    
    [ExportGroup("Pause")] 
    private bool _pausePressed;
    [Export] private Control _pauseMenu;
    private Button _reloadButton;



    public override void _Ready()
    {
        DebugManager.Instance.DebugMode = _debugMode;
        _reloadButton = _pauseMenu.GetNode<Button>("Button");
        _levelHandler.Level = 10;
        _levelHandler.Initial();
    }

  

    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("pause"))
        {
            if (!_pausePressed)
            {
                _pauseMenu.Visible = true;
                _pausePressed = true;
                if (_debugMode)
                {
                    _reloadButton.Visible = true;
                    _reloadButton.Pressed += ReloadButtonPressed;
                }
                else
                {
                    _reloadButton.Visible = false;
                }
            }
            else
            {
                _pauseMenu.Visible = false;
                _pausePressed = false;
                if (_debugMode)
                {
                    _reloadButton.Visible = true;
                    _reloadButton.Pressed -= ReloadButtonPressed;
                }
                else
                {
                    _reloadButton.Visible = false;
                }
            }
        }
        
        base._Process(delta);
    }


    private void ReloadButtonPressed()
    {
        _levelHandler.ReloadLevel();
    }
}
