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
    [Export] public float HurtSpeedMultiplier = 0.5f;
    [Export] public float HurtSlowDuration = 0.5f;

    public AnimationPlayer Anim;
    public Camera3D Camera;
    public RayCast3D SelectRaycast;
    public EquipmentComponent EquipmentComponent;
    public HealthComponent HealthComponent;
    public ControllerComponent Controller;
    public StateMachine StateMachine;

    private PickableItem _highlightedPickable;
    private float _hurtSlowRemaining;

    public CharacterIntent Intent => Controller.Intent;

    // 受击减速：只是临时压低移速，操作权还在玩家手上
    public float SpeedMultiplier => _hurtSlowRemaining > 0f ? HurtSpeedMultiplier : 1f;

    public void ApplyHurtSlow()
    {
        _hurtSlowRemaining = HurtSlowDuration;
    }

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
        GameState.Instance.RegisterPlayer(this);
        HealthComponent.Damaged += OnDamaged;
        HealthComponent.Died += OnDied;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
        StateMachine.Tick(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _hurtSlowRemaining = Mathf.Max(_hurtSlowRemaining - (float)delta, 0f);
        UpdatePickableHighlight();
        Controller.UpdateIntent(delta);
        StateMachine.TickPhysics(delta);

        // 死亡后状态机照常跑（好让死亡状态生效），但不再有重力与位移
        if (HealthComponent.IsDead())
        {
            return;
        }

        ProcessGravity(delta);
        MoveAndSlide();
    }

    // 由攻击者调用（目前只有敌人近战）。伤害当场结算：多个敌人同一帧命中时不会互相顶掉。
    public void TryReceiveHit(Enemy attacker, HurtData hurtData)
    {
        if (HealthComponent.IsDead())
        {
            return;
        }

        HealthComponent.TakeDamage(hurtData.Damage, attacker);
        if (HealthComponent.IsDead())
        {
            return; // 死亡由 HealthComponent.Died -> OnDied 接管
        }

        StateMachine.ChangeState<PlayerHurtState>();
    }

    private void OnDamaged(int damage, Node3D source)
    {
        GameEvents.Instance.TriggerImpactFelt(GameEvents.ImpactIntensity.MEDIUM);
        GameEvents.Instance.TriggerPlayerHurtEvent(this, source);
    }

    private void OnDied()
    {
        GameEvents.Instance.TriggerPlayerDiedEvent(this);
        StateMachine.ChangeState<PlayerDyingState>();
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

        var desiredVelocity = Intent.MoveDirection * Intent.Speed * SpeedMultiplier;
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
