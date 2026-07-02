using System;
using System.Collections.Generic;
using DungeonDelve.Level.Common.Item;
using Godot;

namespace DungeonDelve.Level.TopLayer;

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
        GD.Print("Picked up");  
        Inventory.Add(item);
        _hud.GetChild<Label>(0).Text = $"Inventory: {Inventory.Count}";
        GD.Print($"Added {item}");
        GD.Print($"Added {Inventory.Count}");
    }

    public void RemoveItem(Item item)
    {
        GD.Print($"Removed {item}");
        Inventory.Remove(item);
    }
}