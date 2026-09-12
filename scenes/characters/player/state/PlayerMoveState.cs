using Godot;
using System;

public partial class PlayerMoveState : PlayerState
{
    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);
        var intent = Player.Intent;
        Player.CheckJumpInput();
        Player.ProcessMovement(delta);
        var horizontalVelocity = Player.Velocity with { Y = 0 };
        if (horizontalVelocity.LengthSquared() > 0.1 && Player.IsOnFloor())
        {
            Player.Anim.Play("run");
        }
        else
        {
            Player.Anim.Play("idle");
        }
        if (intent.Interact && Player.CurrentPickableFocusedItem != null)
        {
            StateMachine.ChangeState<PlayerPickUpState>();
            return;
        }
        if (intent.Throw && Player.HasWeapon())
        {
            StateMachine.ChangeState<PlayerThrowState>();
            return;
        }
        if (intent.Attack && Player.HasWeapon())
        {
            StateMachine.ChangeState<PlayerAttackState>();
            return;
        }
        if (intent.Kick)
        {
            StateMachine.ChangeState<PlayerKickState>();
            return;
        }
        if (intent.Defend && Player.HasOffhandEquitment())
        {
            StateMachine.ChangeState<PlayerDefendState>();
            return;
        }
    }

}
