using Godot;
using System;

public partial class DebugManager : Node
{
    public static DebugManager Instance { get; private set; } 
    [Export] public bool DebugMode = false;

    public override void _EnterTree()
    {
        Instance = this;
    }
    
    public void Log(string message)                                                                                                                                                                                         
    {                                                                                                                                                                                                                       
        if (!DebugMode) return;                                                                                                                                                                                             
        GD.Print($"[Debug] {message}");                                                                                                                                                                                     
    }

    public void LogError(string message)
    {
        if (!DebugMode) return;
        GD.PrintErr($"[Error] {message}");
    }
}
