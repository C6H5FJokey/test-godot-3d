using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float Acceleration;
    [Export] public float AirAcceleration = 12f;
    [Export] public float JumpForce;
    [Export] public float Gravity;
    [Export] public float FallGravityMultiplier = 1.6f;
    [Export] public float MaxFallSpeed = 20f;
    [Export] public float RunSpeed;
    [Export] public float WalkSpeed;

    public AnimationPlayer Anim;
    public Camera3D Camera;
    public RayCast3D SelectRaycast;
    public EquipmentComponent EquipmentComponent;
    public HealthComponent HealthComponent;
    public ControllerComponent Controller;
    public StateMachine StateMachine;
    public RayCast3D WeaponReachRaycast;
    public RayCast3D KickReachRaycast;

    private PickableItem _highlightedPickable;

    public CharacterIntent Intent => Controller.Intent;

    public override void _Ready()
    {
        base._Ready();
        Anim = GetNode<AnimationPlayer>("character/AnimationPlayer");
        Camera = GetNode<Camera3D>("%MainCamera");
        SelectRaycast = GetNode<RayCast3D>("%SelectRaycast");
        EquipmentComponent = GetNode<EquipmentComponent>("%EquipmentComponent");
        HealthComponent = GetNode<HealthComponent>("%HealthComponent");
        Controller = GetNode<ControllerComponent>("%Controller");
        StateMachine = GetNode<StateMachine>("%StateMachine");
        WeaponReachRaycast = GetNode<RayCast3D>("%WeaponReachRaycast");
        KickReachRaycast = GetNode<RayCast3D>("%KickReachRaycast");
        GameState.Instance.RegisterPlayer(this);
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        StateMachine.Tick(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdatePickableHighlight();
        Controller.UpdateIntent(delta);
        StateMachine.TickPhysics(delta);
        ProcessGravity(delta);
        MoveAndSlide();
    }

    public override void _ExitTree()
    {
        if (_highlightedPickable == null || !IsInstanceValid(_highlightedPickable))
        {
            return;
        }
        _highlightedPickable.TreeExiting -= OnHighlightedPickableExiting;
    }

    public void CheckJumpInput()
    {
        if (Intent.Jump && IsOnFloor())
        {
            Velocity = Velocity with { Y = JumpForce };
        }
    }

    public void ProcessGravity(double delta)
    {
        if (IsOnFloor())
        {
            return;
        }

        var gravity = Velocity.Y > 0f ? Gravity : Gravity * FallGravityMultiplier;
        Velocity = Velocity with { Y = Mathf.Max(Velocity.Y - gravity * (float)delta, -MaxFallSpeed) };
    }

    public PickableItem GetPickableUnderCrosshair()
    {
        return SelectRaycast.GetCollider() is PickableItem pickableItem && IsInstanceValid(pickableItem) ? pickableItem : null;
    }

    private void UpdatePickableHighlight()
    {
        var pickableItem = GetPickableUnderCrosshair();
        if (pickableItem == _highlightedPickable)
        {
            return;
        }

        if (_highlightedPickable != null && IsInstanceValid(_highlightedPickable))
        {
            _highlightedPickable.SetFocused(false);
            _highlightedPickable.TreeExiting -= OnHighlightedPickableExiting;
        }

        _highlightedPickable = pickableItem;

        if (_highlightedPickable != null)
        {
            _highlightedPickable.SetFocused(true);
            _highlightedPickable.TreeExiting += OnHighlightedPickableExiting;
        }
    }

    private void OnHighlightedPickableExiting()
    {
        _highlightedPickable = null;
    }

    public void ProcessMovement(double delta)
    {
        var isOnFloor = IsOnFloor();

        if (!isOnFloor && Intent.MoveDirection.IsZeroApprox())
        {
            return;
        }

        var desiredVelocity = Intent.MoveDirection * Intent.Speed;
        var acceleration = isOnFloor ? Acceleration : AirAcceleration;

        Velocity = Velocity with
        {
            X = Mathf.MoveToward(Velocity.X, desiredVelocity.X, acceleration * (float)delta),
            Z = Mathf.MoveToward(Velocity.Z, desiredVelocity.Z, acceleration * (float)delta)
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
