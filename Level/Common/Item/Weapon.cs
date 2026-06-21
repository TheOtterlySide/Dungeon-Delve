
using System;
using DungeonDelve.Level.Common;
using DungeonDelve.Level.Common.Enum;
using Godot;

public partial class Weapon : Item
{
    [ExportGroup("Weapon Part")]
    [Export] public int Damage { get; set; }

    [Export] public int Durability { get; set; }
    [Export] bool _moveable;
    [Export] private float _amplitude;
    [Export] private float _freq;
    [Export] private float _rotationSpeed;


    private float _time;
    private Vector3 _startPosition;

    public override void _Ready()
    {
        _startPosition = Position;
    }

    [Export]
    public WeaponTypeEnum WeaponType { get; set; }

    public Weapon()
    {

    }

    public override void _PhysicsProcess(double delta)
    {
        MoveAnimation(delta);
    }

    private void MoveAnimation(double delta)
    {
        if (!_moveable) return;

        _time += (float)delta;

        float offsetY = Mathf.Sin(_time * _freq) * _amplitude;
        RotateY(_rotationSpeed * (float)delta);

        Position = new Vector3(
            _startPosition.X ,
            _startPosition.Y + offsetY,
            _startPosition.Z
        );
        
    }
}