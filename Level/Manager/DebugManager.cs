using Godot;
using System;
using System.Collections.Generic;
using DungeonDelve.Level.Common.Enum;

public partial class DebugManager : Node
{
    public static DebugManager Instance { get; private set; }
    private RichTextLabel _console;
    private TabBar _tabBar;
    public bool DebugMode;
    
    private Dictionary<int, (DebugCategory, string)> _categoryLogs = new Dictionary<int, (DebugCategory, string)>();
    private List<string> _logMessages = new List<string>();
    private int _logid = 0;

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _Ready()
    {
        _console = Instance.GetNode<RichTextLabel>("Console");
        _tabBar = Instance.GetNode<TabBar>("TabBar");
        
        foreach (var category in Enum.GetValues<DebugCategory>())
        {
            if (_tabBar.GetTabCount() >= Enum.GetValues<DebugCategory>().Length) continue;
            _tabBar.AddTab(category.ToString());
        }
        _tabBar.TabChanged += OnTabChanged;
        
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
        _categoryLogs.Add(_logid++,(category, message));
        SendToConsole();
    }

    private void LogError(string message, DebugCategory category)
    {
        GD.PrintErr($"[Error] {message}");
        _categoryLogs.Add(_logid++,(category, message));


        SendToConsole();
    }
    
    private void LogWarning(string message, DebugCategory category)
    {
        GD.PrintRich($"[color=yellow] [Warning] {message} [/color]");
        _categoryLogs.Add(_logid++,(category, message));


        SendToConsole();
    }
    
    private void BuiltTabBar(TabBar tabBar)
    {
     
    }

    private void OnTabChanged(long tab)
    {
        var selectedCategory = (DebugCategory)tab;
        GD.Print($"Selected Debug Category: {selectedCategory}");
    }
    
    private void SendToConsole()
    {
        _console.Clear();
        foreach(var message in _categoryLogs)
        {
            _console.AppendText(message.Value + "\n");
        }
    }
}
