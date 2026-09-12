using Godot;
using System;

public partial class GoblinStunState : GoblinState
{
    private bool _isRecover;
    private float _stunDuration = 0;
    public override void Enter(State previousState)
    {
        Goblin.Anim.AnimationFinished += _animationFinished;
        Goblin.Anim.Play("stun");
        _stunDuration = 1;
        _isRecover = true;
    }

    public override void Exit(State nextState)
    {
        Goblin.Anim.AnimationFinished -= _animationFinished;
        _isRecover = false;
    }

    public override void PhysicsUpdate(double delta)
    {
        _stunDuration -= (float)delta;
        if (_isRecover && _stunDuration <= 0)
        {
            Goblin.Anim.PlayBackwards("stun");
            _isRecover = false;
        }
    }


    private void _animationFinished(StringName animName)
    {
        if (animName == "stun" && _stunDuration <= 0)
        {
            StateMachine.ChangeState<GoblinMoveState>();
        }
    }
}
