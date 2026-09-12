using Godot;
using System;

public partial class Player : CharacterBody3D
{
    private static readonly float MAX_ANGLE_LOOK_UP = Mathf.DegToRad(70f);
    private static readonly float MAX_ANGLE_LOOK_DOWN = Mathf.DegToRad(-70f);

    [Export] public float Acceleration;
    [Export] public float JumpForce;
    [Export] public float Gravity;
    [Export] public float MouseSensitivity;
    [Export] public float RunSpeed;
    [Export] public float WalkSpeed;

    public AnimationPlayer Anim;
    public Camera3D Camera;
    public RayCast3D SelectRaycast;
    public EquipmentComponent EquipmentComponent;
    public HealthComponent HealthComponent;
    private StateMachine _stateMachine;
    public RayCast3D WeaponReachRaycast;
    public RayCast3D KickReachRaycast;

    public Vector2 InputDir = Vector2.Zero;
    public PickableItem CurrentPickableFocusedItem;

    public override void _Ready()
    {
        base._Ready();
        Anim = GetNode<AnimationPlayer>("character/AnimationPlayer");
        Camera = GetNode<Camera3D>("%MainCamera");
        SelectRaycast = GetNode<RayCast3D>("%SelectRaycast");
        EquipmentComponent = GetNode<EquipmentComponent>("%EquipmentComponent");
        HealthComponent = GetNode<HealthComponent>("%HealthComponent");
        _stateMachine = GetNode<StateMachine>("%StateMachine");
        WeaponReachRaycast = GetNode<RayCast3D>("%WeaponReachRaycast");
        KickReachRaycast = GetNode<RayCast3D>("%KickReachRaycast");
        GameState.Instance.RegisterPlayer(this);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        InputDir = Input.GetVector("strafe_left", "strafe_right", "backward", "forward");
    }

    public override void _PhysicsProcess(double delta)
    {
        ProcessGravity();
        MoveAndSlide();
        CheckSelectRaycast();
    }


    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (Input.MouseMode == Input.MouseModeEnum.Captured && @event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            Camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
            Camera.Rotation = Camera.Rotation with { X = Mathf.Clamp(Camera.Rotation.X, MAX_ANGLE_LOOK_DOWN, MAX_ANGLE_LOOK_UP) };
        }
    }

    public void CheckJumpInput()
    {
        if (IsOnFloor() && Input.IsActionJustPressed("jump"))
        {
            Velocity = Velocity with { Y = JumpForce };
        }
    }

    public void ProcessGravity()
    {
        if (!IsOnFloor())
        {
            Velocity = Velocity with { Y = Velocity.Y - Gravity };
        }
    }

    public void CheckSelectRaycast()
    {
        var targetNode = SelectRaycast.GetCollider() as PickableItem;
        if (targetNode != CurrentPickableFocusedItem)
        {
            CurrentPickableFocusedItem?.Unhightlight();
            targetNode?.Hightlight();
            CurrentPickableFocusedItem = targetNode;
        }
    }

    public void ProcessMovement(double delta)
    {
        var input3DSpece = new Vector3(InputDir.X, 0f, -InputDir.Y);
        var targetSpeed = Input.IsActionPressed("run") ? RunSpeed : WalkSpeed;
        var desiredVelocity = Transform.Basis * input3DSpece * targetSpeed;
        Velocity = Velocity with
        {
            X = Mathf.MoveToward(Velocity.X, desiredVelocity.X, Acceleration * (float)delta),
            Z = Mathf.MoveToward(Velocity.Z, desiredVelocity.Z, Acceleration * (float)delta)
        };
    }

    public bool HasWeapon()
    {
        return EquipmentComponent.HasWeapon();
    }

    public void ThrowWeapon()
    {
        EquipmentComponent.ThrowWeapon(3, Velocity);
    }

    public bool HasOffhandEquitment()
    {
        return EquipmentComponent.HasOffhandEquipment();
    }
}
