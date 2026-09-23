using Godot;
using System;
using System.Collections.Generic;

public partial class PlayerKickState : PlayerState
{
    [Export] public float Reach = 2f;
    [Export] public float SwingHalfAngle = 45f;

    private bool _isConnectedAnimationFinished = false;
    private double _timeElapsed = 0;
    private bool _hasEmittedDamage = false;
    private readonly List<Node3D> _targets = [];
    public override void Enter(State previousState)
    {
        Player.Anim.Play("kick");
        if (!_isConnectedAnimationFinished)
        {
            Player.Anim.AnimationFinished += _animationFinished;
            _isConnectedAnimationFinished = true;
        }
        _timeElapsed = 0;
        _hasEmittedDamage = false;
    }

    public override void PhysicsUpdate(double delta)
    {
        Player.ProcessMovement(delta);
        _timeElapsed += delta;
        if (_timeElapsed > 0.1 && !_hasEmittedDamage)
        {
            _hasEmittedDamage = true;
            ApplyKick();
        }
    }

    private void ApplyKick()
    {
        var forward = -Player.GlobalBasis.Z;
        MeleeSwing.CollectTargets(Player, forward, Reach, SwingHalfAngle, PhysicsLayers.Enemy | PhysicsLayers.Door, _targets);

        foreach (var target in _targets)
        {
            switch (target)
            {
                case Enemy enemy:
                    enemy.TryReceiveHit(Player, new HurtData()
                    {
                        PushbackIntensity = 2.0f,
                        Stun = true,
                    });
                    GameEvents.Instance.TriggerImpactFelt(GameEvents.ImpactIntensity.LOW);
                    break;
                case DoorBody doorBody:
                    doorBody.TriggerOpenRequested(forward * 2);
                    break;
            }
        }
    }

    public override void Exit(State nextState)
    {
        if (_isConnectedAnimationFinished)
        {
            Player.Anim.AnimationFinished -= _animationFinished;
            _isConnectedAnimationFinished = false;
        }
    }

    private void _animationFinished(StringName animName)
    {
        if (animName == "kick")
        {
            StateMachine.ChangeState<PlayerMoveState>();
        }
    }
}
