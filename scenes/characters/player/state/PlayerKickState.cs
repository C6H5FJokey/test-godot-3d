using Godot;
using System;

public partial class PlayerKickState : PlayerState
{
    private bool _isConnectedAnimationFinished = false;
    private double _timeElapsed = 0;
    private bool _hasEmittedDamage = false;
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
            if (Player.KickReachRaycast.IsColliding())
            {
                switch(Player.KickReachRaycast.GetCollider())
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
                        doorBody.TriggerOpenRequested(-Player.KickReachRaycast.GlobalBasis.Z * 2);
                        break;
                }
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
