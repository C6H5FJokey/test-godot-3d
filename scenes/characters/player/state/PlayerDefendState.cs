using Godot;
using System;

public partial class PlayerDefendState : PlayerState
{
    public override void Enter(State previousState)
    {
        Player.Anim.Play("defend");
    }

    public override void PhysicsUpdate(double delta)
    {
        Player.CheckJumpInput();
        var velocity = Player.Velocity;
        Player.Velocity = (velocity * 0.8f) with {Y = velocity.Y};
        if (!Player.Intent.Defend)
        {
            StateMachine.ChangeState<PlayerMoveState>();
        }
    }

}
