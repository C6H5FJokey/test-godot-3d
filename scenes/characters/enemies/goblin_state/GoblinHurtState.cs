using Godot;
using System;

public partial class GoblinHurtState : GoblinState
{
    private bool _isConnectedAnimationFinished = false;
    private bool _isStun = false;
    public override void Enter(State previousState, object data)
    {
        var stateData = data as GoblinHurtStateData;
        if (stateData != null)
        {
            Goblin.HealthComponent.TakeDamage(stateData.Damage);
            Goblin.PushbackForce += stateData.ImpactDirection * 8;
            _isStun = stateData.Stun && previousState is GoblinAttackState;
        }
        if (Goblin.HealthComponent.IsDead())
        {
            if (stateData != null)
            {
                StateMachine.ChangeState<GoblinDyingState>(new GoblinDyingStateData
                {
                    Impulse = stateData.ImpactDirection * 20 + Vector3.Up * 20
                });
            }
            else
            {
                StateMachine.ChangeState<GoblinDyingState>();
            }
        }
        else
        {
            Goblin.Anim.Play("hurt");
            if (!_isConnectedAnimationFinished)
            {
                Goblin.Anim.AnimationFinished += _animationFinished;
                _isConnectedAnimationFinished = true;
            }
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
        if (animName == "hurt")
        {
            if (_isStun)
            {
                _isStun = false;
                StateMachine.ChangeState<GoblinStunState>();
            }
            else
            {
                StateMachine.ChangeState<GoblinMoveState>();
            }
        }
    }
}

public partial class GoblinHurtStateData
{
    public int Damage { get; set; }
    public Vector3 ImpactDirection { get; set; }
    public bool Stun {get; set;}
}