using Godot;
using DungeonDelve.Level.Common;
using InventoryHandler = DungeonDelve.Level.Handler.InventoryHandler;

public partial class Player : CharacterBody3D
{
    [ExportGroup("PlayerStats")] 
    [Export] private float _speed;
    [Export] private float _jumpForce;
    [Export] private float _sprintSpeed;
    [Export] private float _acceleration;
    [Export] private float _braking;
    [Export] private float _airAcceleration;
    [Export] private int _jumpCountBase;

    private int _jumpCount;
    [Export] private bool _doubleJump;

    [Export] private bool _isRunning;
    [Export] private bool _isJumping;
    [Export] private State _state;

    [ExportGroup("Camera")] 
    [Export] private Camera3D _camera;
    [Export] private Vector2 _cameraInput;
    [Export] private float _cameraSensivity;

    [ExportGroup("World")] 
    [Export] private Variant _gravity = ProjectSettings.GetSetting("physics/3d/default_gravity");
    [Export] private float _gravityMultiplier = 3f;
    [Export] public InventoryHandler _itemHandler;
    [Signal] public delegate void PlayerInteractedEventHandler();
    public bool canInteract = false;
    public override void _Ready()
    {
        _jumpCount = _jumpCountBase;
        Input.SetMouseMode(Input.MouseModeEnum.Captured);
    }

    public override void _PhysicsProcess(double delta)
    {
        HandleMovement(delta);
        HandleCamera();
        HandleMouse();
        HandleInteraction();
        MoveAndSlide();
    }

    private void HandleInteraction()
    {
        if (canInteract)
        {
            if (Input.IsActionJustPressed("interact"))
            {
                EmitSignal("PlayerInteracted");
            }
        }
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

            // Pitch (vertical)
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
        float speed = Input.IsActionPressed("sprint") ? _sprintSpeed : _speed;

        if (Input.IsActionJustPressed("jump") && _jumpCount >= 0)
        {
            Velocity = new Vector3(
                Velocity.X,
                _jumpForce,
                Velocity.Z
            );

            _jumpCount--;
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
                Velocity.X,
                Velocity.Y - gravity * (float)delta,
                Velocity.Z
            );
            
            AirMove(moveDirection.Normalized(), _speed, (float)delta);
        }

        if (IsOnFloor())
        {
            Velocity = new Vector3(
                moveDirection.X * speed,
                Velocity.Y,
                moveDirection.Z * speed
            );

            _jumpCount = _jumpCountBase;
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
    
    private void AirMove(Vector3 wishDir, float speed, float delta)
    {
        if (wishDir == Vector3.Zero)
        {
            return;
        }

        float wishSpeed = wishDir.Length();
        wishSpeed = Mathf.Min(wishSpeed, 1.0f) * speed;

        float currentSpeed = Velocity.Dot(wishDir);
        float addSpeed = wishSpeed - currentSpeed;

        if (addSpeed <= 0)
        {
            return;
        }

        float accelSpeed = _airAcceleration * wishSpeed * delta;
        if (accelSpeed > addSpeed)
        {
            accelSpeed = addSpeed;
        }

        Velocity += accelSpeed * wishDir;
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