using System;
using System.Collections.Generic;
using DungeonDelve.Level.Common.Enum;
using DungeonDelve.Level.Common.Item;
using Godot;

namespace DungeonDelve.Level.Handler;

public partial class InventoryHandler : Node
{
    [Export] private Player _player;
    public List<Item> Inventory = new List<Item>();
    private Control _hud;


    private Action testAction;
    
    public override void _Ready()
    {
        base._Ready();
        ClearInventory();
        _hud = GetNode<Control>("/root/Main/HUD");
    }

    private void ClearInventory()
    {
        Inventory.Clear();
    }

    public void AddItem(Item item)
    {
        Inventory.Add(item);
        _hud.GetChild<Label>(0).Text = $"Inventory: {Inventory.Count}";
        DebugManager.Instance.LogMessage(DebugKind.LOG, $"Adding {item}", DebugCategory.Inventory);
        DebugManager.Instance.LogMessage(DebugKind.LOG, $"Added {Inventory.Count}", DebugCategory.Inventory);
    }

    public void RemoveItem(Item item)
    {
        DebugManager.Instance.LogMessage(DebugKind.LOG, $"Removing {item}", DebugCategory.Inventory);
        Inventory.Remove(item);
    }
}