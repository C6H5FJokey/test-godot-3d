using Godot;
using System;

public partial class GoblinAttackState : GoblinState
{
    [Export] public float DamageFrameTime = 0.15f;

    private bool _isConnectedAnimationFinished = false;
    private double _timeElapsed = 0;
    private bool _hasDealtDamage = false;

    public override void Enter(State previousState)
    {
        Goblin.TimeSinceLastAttack = Time.GetTicksMsec();
        _timeElapsed = 0;
        _hasDealtDamage = false;
        Goblin.Anim.Play("attack");
        if (!_isConnectedAnimationFinished)
        {
            Goblin.Anim.AnimationFinished += _animationFinished;
            _isConnectedAnimationFinished = true;
        }
    }

    // "attack" 是一条完整动画（起手+挥砍+收招，0.417s），伤害判定落在起手之后。
    public override void PhysicsUpdate(double delta)
    {
        _timeElapsed += delta;
        if (_hasDealtDamage || _timeElapsed < DamageFrameTime)
        {
            return;
        }

        _hasDealtDamage = true;

        var weaponData = Goblin.EquipmentComponent.WeaponData;
        var target = Goblin.Controller.Target;
        if (weaponData == null || target == null || !Goblin.IsPlayerWithinReach(target))
        {
            return;
        }

        target.TryReceiveHit(Goblin, new HurtData
        {
            Damage = weaponData.GetDamageDealt(),
        });
    }

    public override void Exit(State nextState)
    {
        if (_isConnectedAnimationFinished)
        {
            Goblin.Anim.AnimationFinished -= _animationFinished;
            _isConnectedAnimationFinished = false;
        }
    }


    private void _animationFinished(StringName animName)
    {
        if (animName == "attack")
        {
            StateMachine.ChangeState<GoblinMoveState>();
        }
    }
}
