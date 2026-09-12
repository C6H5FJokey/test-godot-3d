using Godot;
using System;

public partial class Player : CharacterBody3D
{
    [Export] public float Acceleration;
    [Export] public float JumpForce;
    [Export] public float Gravity;
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

    public PickableItem CurrentPickableFocusedItem;

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
        Controller.UpdateIntent(delta);
        StateMachine.TickPhysics(delta);
        ProcessGravity();
        MoveAndSlide();
        CheckSelectRaycast();
    }

    public void CheckJumpInput()
    {
        if (Intent.Jump && IsOnFloor())
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
        var desiredVelocity = Intent.MoveDirection * Intent.Speed;
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
