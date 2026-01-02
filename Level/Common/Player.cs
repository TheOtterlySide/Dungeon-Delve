using Godot;
using System;
using DungeonDelve.Level.Common;

public partial class Player : CharacterBody3D
{
    [ExportGroup("PlayerStats")] 
    [Export] private float _speed;
    [Export] private float _jumpForce;
    [Export] private float _sprintSpeed;
    [Export] private float _acceleration;
    [Export] private float _braking;
    [Export] private float _airAcceleration;

    [Export] private bool _isRunning;
    [Export] private bool _isJumping;
    [Export] private State _state;

    [ExportGroup("Camera")] 
    [Export] private Camera3D _camera;
    [Export] private Vector2 _cameraInput;
    [Export] private float _cameraSensivity;

    [ExportGroup("World")] [Export] private Variant _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity");
    [Export] private float _gravityMultiplier = 3f;

    public override void _Ready()
    {
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement(delta);
        HandleCamera();
        HandleMouse();

        MoveAndSlide();
    }

    private static void HandleMouse()
    {
        if (Input.IsActionJustPressed("ui_cancel"))
        {
            if (Input.GetMouseMode() == Input.MouseModeEnum.Captured)
            {
                Input.SetMouseMode(Input.MouseModeEnum.Visible);
            }
            else
            {
                Input.SetMouseMode(Input.MouseModeEnum.Captured);
            }
        }
    }

    private void HandleCamera()
    {
        if (Input.GetMouseMode() == Input.MouseModeEnum.Captured)
        {
            // Yaw (horizontal)
            RotateY(-_cameraInput.X * _cameraSensivity);

            // Pitch (vertikal)
            _camera.RotateX(-_cameraInput.Y * _cameraSensivity);

            var rotation = _camera.Rotation;
            rotation.X = Mathf.Clamp(rotation.X, -1.5f, 1.5f);
            _camera.Rotation = rotation;
            _cameraInput = Vector2.Zero;
        }
    }

    private void HandleMovement(double delta)
    {
        float gravity = _gravity.AsSingle() * _gravityMultiplier;

        if (Input.IsActionPressed("jump") && IsOnFloor())
        {
            Velocity = new Vector3(0, _jumpForce, 0);
        }

        var moveInput = Input.GetVector("mv_left", "mv_right", "mv_for", "mv_back");
        var moveDirection = Transform.Basis * new Vector3(moveInput.X, 0, moveInput.Y);
        var currentSmooth = _acceleration;

        if (!IsOnFloor())
        {
            currentSmooth = _airAcceleration;
        }

        if (moveDirection == Vector3.Zero)
        {
            currentSmooth = _braking;
        }
        
        if (!IsOnFloor())
        {
            Velocity = new Vector3(
                Mathf.Lerp(Velocity.X, moveDirection.X * _speed, currentSmooth * (float)delta),
                -(float)(gravity * delta),
                Mathf.Lerp(Velocity.Z, moveDirection.Z * _speed, currentSmooth * (float)delta)
            );
        }

        if (IsOnFloor())
        {
            Velocity = new Vector3(
                moveDirection.X * _speed,
                Velocity.Y,
                moveDirection.Z * _speed
            );
        }
        
        if (moveInput == Vector2.Zero && IsOnFloor())
        {
            Velocity = new Vector3(
                Mathf.MoveToward(Velocity.X, 0, currentSmooth * (float)delta),
                Velocity.Y,
                Mathf.MoveToward(Velocity.Z, 0, currentSmooth * (float)delta)
            );
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            _cameraInput = mouseMotion.Relative;
        }

        base._UnhandledInput(@event);
    }

    private void HandleStateChange()
    {
    }

    private State ChangeStateOfCharacter(State newState)
    {
        switch (newState)
        {
            case State.ATTACK:
                break;
            case State.RUN:
                break;
            case State.WALK:
                break;
            case State.JUMP:
                break;
            default:
                break;
        }

        return newState;
    }
}