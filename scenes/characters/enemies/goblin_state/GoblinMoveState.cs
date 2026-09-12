using Godot;
using System;

public partial class GoblinMoveState : GoblinState
{
    public override void Enter(State previousState)
    {
        base.Enter(previousState);

        Goblin.Anim.Play("idle");
    }

    public override void PhysicsUpdate(double delta)
    {
        var isMoving = Goblin.ApplyIntentMovement(delta);
        Goblin.Anim.Play(isMoving ? "run" : "idle");

        if (Goblin.Intent.Attack)
        {
            StateMachine.ChangeState<GoblinAttackState>();
        }
    }
}
