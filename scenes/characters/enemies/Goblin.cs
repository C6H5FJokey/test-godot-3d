using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class Goblin : Enemy, IImpaleable
{

    private const float TURN_SPEED = 4f;

    [Export] public float Gravity { get; set; } = 9.8f;
    [Export] public float AirFriction { get; set; } = 1f;
    [Export] public float Speed { get; set; } = 3f;
    [Export] public ulong DurationBetweenAttacks { get; set; }

    public ImpaleComponent ImpaleComponent {get; set;}
    public PhysicalBoneSimulator3D PhysicalBoneSimulator { get; private set; }
    public AnimationPlayer Anim {get; set; }
    public CollisionShape3D CollisionShape;
    public EquipmentComponent EquipmentComponent;
    public HealthComponent HealthComponent;
    public PhysicalBone3D PhysicalBoneBody;
    public StateMachine StateMachine;
    public GoblinAIController Controller;
    private RayCast3D _weaponReachRaycast;
    public NavigationAgent3D NavigationAgent3D;

    public ulong TimeSinceLastAttack;
    public Vector3 PushbackForce;

    public bool IsRagdolling { get; private set; }

    public CharacterIntent Intent => Controller.Intent;

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
        Controller = GetNode<GoblinAIController>("%Controller");
        _weaponReachRaycast = GetNode<RayCast3D>("%WeaponReachRaycast");
        NavigationAgent3D = GetNode<NavigationAgent3D>("%NavigationAgent3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        Controller.UpdateIntent(delta);
        StateMachine.TickPhysics(delta);

        if (IsRagdolling)
        {
            return;
        }

        ProcessGravity(delta);
        ProcessPushback(delta);
        Velocity = Velocity with
        {
            X = Mathf.MoveToward(Velocity.X, 0, (float)delta * AirFriction),
            Z = Mathf.MoveToward(Velocity.Z, 0, (float)delta * AirFriction)
        };
        MoveAndSlide();
    }

    public void StartRagdoll()
    {
        IsRagdolling = true;
        Velocity = Vector3.Zero;
        CollisionShape.Disabled = true;
        Anim.Stop();
        PhysicalBoneSimulator.PhysicalBonesStartSimulation();
    }

    public override void _Process(double delta)
    {
        StateMachine.Tick(delta);
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

    public bool IsPlayerWithinReach(Player target)
    {
        if (target == null || !IsInstanceValid(target) || !EquipmentComponent.HasWeapon())
        {
            return false;
        }
        return _weaponReachRaycast.IsColliding() && _weaponReachRaycast.GetCollider() == target;
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

    public bool ApplyIntentMovement(double delta)
    {
        if (Intent.Speed <= 0f || Intent.FacingDirection.IsZeroApprox())
        {
            return false;
        }

        var targetBasis = GlobalTransform.LookingAt(GlobalPosition + Intent.FacingDirection).Basis.Orthonormalized();
        GlobalBasis = GlobalBasis.Orthonormalized().Slerp(targetBasis, (float)delta * TURN_SPEED);

        var forward = -GlobalBasis.Z;
        forward.Y = 0f;
        forward = forward.Normalized();

        Velocity = Velocity with { X = forward.X * Intent.Speed, Z = forward.Z * Intent.Speed };
        return true;
    }

    public void ProcessGravity(double delta)
    {
        if (!IsOnFloor())
        {
            Velocity = Velocity with { Y = Velocity.Y - Gravity * (float)delta };
        }
    }

    public void ProcessPushback(double delta)
    {
        Velocity += PushbackForce;
        PushbackForce = Vector3.Zero;
    }

    public override void OnScreamHeard(Player player)
    {
        Controller.SetTargetIfEmpty(player);
    }
}
