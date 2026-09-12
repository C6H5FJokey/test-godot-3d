using Godot;
using System;

public partial class GoblinAttackState : GoblinState
{
    private bool _isConnectedAnimationFinished = false;
    public override void Enter(State previousState)
    {
        Goblin.TimeSinceLastAttack = Time.GetTicksMsec();
        Goblin.Anim.Play("attack");
        if (!_isConnectedAnimationFinished)
        {
            Goblin.Anim.AnimationFinished += _animationFinished;
            _isConnectedAnimationFinished = true;
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        Goblin.ProcessMovement(delta);
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
