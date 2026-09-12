using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;

public partial class PlayerAttackState : PlayerState
{
    private bool _activeAttack = false;
    private HashSet<Enemy> _enemies = [];

    public override void Enter(State previousState)
    {
        Player.Anim.Play("attack_startup");
        Player.Anim.AnimationFinished += _animationFinished;
        _activeAttack = false;
        _enemies.Clear();
    }

    public override void PhysicsUpdate(double delta)
    {
        Player.CheckJumpInput();
        Player.ProcessMovement(delta);
        if (_activeAttack)
        {
            if (Player.WeaponReachRaycast.IsColliding())
            {
                if (Player.WeaponReachRaycast.GetCollider() is Enemy enemy)
                {
                    if (_enemies.Contains(enemy))
                    {
                        return;
                    }
                    _enemies.Add(enemy);
                    var damage = GD.RandRange(Player.EquipmentComponent.WeaponData.DamageMin, Player.EquipmentComponent.WeaponData.DamageMax);
                    enemy.TryReceiveHit(Player, new HurtData()
                    {
                        Damage = damage,
                        PushbackIntensity = 1.0f,
                    });
                    GameEvents.Instance.TriggerImpactFelt(GameEvents.ImpactIntensity.LOW);
                }
            }
        }
    }


    public override void Exit(State nextState)
    {
        Player.Anim.AnimationFinished -= _animationFinished;
    }

    private void _animationFinished(StringName animName)
    {
        switch (animName)
        {
            case "attack_startup":
                Player.Anim.Play("attack_active");
                _activeAttack = true;
                break;
            case "attack_active":
                Player.Anim.Play("attack_recovery");
                _activeAttack = false;
                break;
            case "attack_recovery":
                StateMachine.ChangeState<PlayerMoveState>();
                break;
        }
    }
}
