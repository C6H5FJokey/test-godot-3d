using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Goblin : Enemy, IImpaleable
{

    [Export] public float Gravity { get; set; } = 0.98f;
    [Export] public float AirFriction { get; set; } = 1f;
    [Export] public float Speed { get; set; } = 3f;
    [Export] public ulong DurationBetweenAttacks { get; set; }
    [Export] public Player Player { get; set; }

    public ImpaleComponent ImpaleComponent {get; set;}
    public PhysicalBoneSimulator3D PhysicalBoneSimulator { get; private set; }
    public AnimationPlayer Anim {get; set; }
    public CollisionShape3D CollisionShape;
    public EquipmentComponent EquipmentComponent;
    public HealthComponent HealthComponent;
    public PhysicalBone3D PhysicalBoneBody;
    public StateMachine StateMachine;
    private RayCast3D _weaponReachRaycast;
    private Area3D _playerDetectArea;
    public NavigationAgent3D NavigationAgent3D;

    public ulong TimeSinceLastAttack;
    public Vector3 PushbackForce;

    public override void _Ready()
    {
        base._Ready();
        ImpaleComponent = GetNode<ImpaleComponent>("%ImpaleComponent");
        ImpaleComponent.Impaled += OnImpaled;
        PhysicalBoneSimulator = GetNode<PhysicalBoneSimulator3D>("%PhysicalBoneSimulator3D");
        Anim = GetNode<AnimationPlayer>("character/AnimationPlayer");
        CollisionShape = GetNode<CollisionShape3D>("%CollisionShape3D");
        EquipmentComponent = GetNode<EquipmentComponent>("%EquipmentComponent");
        HealthComponent = GetNode<HealthComponent>("%HealthComponent");
        PhysicalBoneBody = GetNode<PhysicalBone3D>("%Physical Bone Body");
        StateMachine = GetNode<StateMachine>("%StateMachine");
        _weaponReachRaycast = GetNode<RayCast3D>("%WeaponReachRaycast");
        _playerDetectArea = GetNode<Area3D>("%PlayerDetectArea");
        _playerDetectArea.BodyEntered += OnBodyEntered;
        NavigationAgent3D = GetNode<NavigationAgent3D>("%NavigationAgent3D");
    }

    public bool CanImpale(WeaponData weaponData)
    {
        return true;
    }

    public void Impale(ThrowableItem throwableItem)
    {
        ImpaleComponent.Impale(throwableItem);
    }

    public void DropWeapon()
    {
        EquipmentComponent.DropWeapon();
    }

    private void OnImpaled(Vector3 impulse)
    {
        HealthComponent.TakeDamage(HealthComponent.MaxHPValue);
        StateMachine.ChangeState<GoblinDyingState>(new GoblinDyingStateData
        {
            Impulse = impulse,
        });
    }

    public bool HasRegisteredPlayer()
    {
        return Player != null && IsInstanceValid(Player);
    }

    public bool IsPlayerWithinReach()
    {
        if (HasRegisteredPlayer() && EquipmentComponent.HasWeapon())
        {
            return _weaponReachRaycast.IsColliding();
        }
        return false;
    }

    public override void TryReceiveHit(Player player, HurtData hurtData)
    {
        var hitDirection = player.GlobalPosition.DirectionTo(GlobalPosition);
        StateMachine.ChangeState<GoblinHurtState>(new GoblinHurtStateData
        {
            Damage = hurtData.Damage,
            ImpactDirection = hitDirection * hurtData.PushbackIntensity,
            Stun = hurtData.Stun,
        });
        EmitSignalScreamed(player);
    }

    public void ProcessMovement(double delta)
    {
        ProcessGravity();
        ProcessPushback(delta);
        MoveAndSlide();
        Velocity = Velocity with { X = Mathf.MoveToward(Velocity.X, 0, (float)delta * AirFriction), Z = Mathf.MoveToward(Velocity.Z, 0, (float)delta * AirFriction) };
    }

    public void ProcessGravity()
    {
        if (!IsOnFloor())
        {
            Velocity = Velocity with { Y = Velocity.Y - Gravity };
        }
    }

    public void ProcessPushback(double delta)
    {
        Velocity += PushbackForce;
        PushbackForce = Vector3.Zero;
    }

    public void OnBodyEntered(Node3D node3D)
    {
        if (node3D is Player player)
        {
            Player = player;
        }
    }

    public override void OnScreamHeard(Player player)
    {
        Player ??= player;
    }
}
