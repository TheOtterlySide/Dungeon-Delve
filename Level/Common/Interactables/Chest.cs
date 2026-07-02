using System.Collections.Generic;
using DungeonDelve.Level.Common.Enum;
using DungeonDelve.Scenes;
using System;
using System.Linq;
using Godot;

namespace DungeonDelve.Level.Common.Interactables;

public partial class Chest : Interactable
{
    private bool _isOpened;
    private Node3D _itemNode;
    private List<ItemTypeEnum> _itemTypes;
    private List<WeaponTypeEnum> _weaponTypes;
    private ItemTypeEnum _itemEnum;
    private PackedScene _item;

    public override void _Ready()
    {
        base._Ready();
        _isOpened = false;

        _itemTypes = System.Enum.GetValues(typeof(ItemTypeEnum)).Cast<ItemTypeEnum>().ToList();
        _weaponTypes = System.Enum.GetValues(typeof(WeaponTypeEnum)).Cast<WeaponTypeEnum>().ToList();
        _itemEnum = DecideItemType();
    }

    protected override void OnPlayerInteracted()
    {
        if (_isOpened) return;

        _isOpened = true;
        _itemNode = GetItemNode(_itemEnum);
        AddChild(_itemNode);
    }

    private ItemTypeEnum DecideItemType()
    {
        var rng = new RandomNumberGenerator();
        var item = _itemTypes[rng.RandiRange(0, _itemTypes.Count - 1)];

        return item;
    }

    private Node3D GetItemNode(ItemTypeEnum itemType)
    {
        string path = string.Empty;

        switch (itemType)
        {
            case ItemTypeEnum.Weapons:
                path = "res://Scenes/Items/Weapon";
                break;
            case ItemTypeEnum.Potions:
                path = "res://Scenes/Items/Weapon";
                break;
            default:
                break;
        }

        var availableScenes = DirContents(path);
        var rng = new RandomNumberGenerator();
        var item = availableScenes[rng.RandiRange(0, availableScenes.Count - 1)];
        
        return (Node3D)GD.Load<PackedScene>(item).Instantiate();
    }
    
    public List<string> DirContents(string path)
    {
        using var dir = DirAccess.Open(path);
        var tempList = new List<string>();
        if (dir != null)
        {
            dir.ListDirBegin();
            string fileName = dir.GetNext();
            while (fileName != "")
            {
                if (dir.CurrentIsDir())
                {
                    GD.Print($"Found directory: {fileName}");
                }
                else
                {
                    GD.Print($"Found file: {fileName}");
                    var result = $"{path}/{fileName}";
                    tempList.Add(result);
                }
                fileName = dir.GetNext();
            }
            
            return tempList;
        }
        
        else
        {
            GD.Print("An error occurred when trying to access the path.");
            return null;
        }
    }
}