using Godot;
using System;
using DungeonDelve.Level.Common.Enum;

public partial class DebugManager : Node
{
    public static DebugManager Instance { get; private set; }
    private RichTextLabel _console;
    public bool DebugMode;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _console = Instance.GetNode<RichTextLabel>("Console");
    }

    public override void _Process(double delta)
    {
        _console.Visible = DebugMode;
    }

    public void LogMessage(DebugKind kind, string message, DebugCategory category)
    {
        switch (kind)
        {
            case DebugKind.LOG:
                Log(message, category);
                break;
            case DebugKind.ERROR:
                LogError(message, category);
                break;
            case DebugKind.WARNING:
                LogWarning(message, category);
                break;
            default:
                GD.Print($"[Unknown] {message}");
                break;
        }
    }
    
    private void Log(string message, DebugCategory category)                                                                      
    {
        GD.Print($"[Debug] {message}");
        _console.AppendText($"[Debug] {message}\n");
    }

    private void LogError(string message, DebugCategory category)
    {
        GD.PrintErr($"[Error] {message}");
        _console.AppendText($"[Error] {message}\n");
    }
    
    private void LogWarning(string message, DebugCategory category)
    {
        GD.PrintRich($"[color=yellow] [Warning] {message} [/color]");
        _console.AppendText($"[Warning] {message}\n");
    }
}
