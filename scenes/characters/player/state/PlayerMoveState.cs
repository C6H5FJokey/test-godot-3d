using Godot;
using System;

public partial class PlayerMoveState : PlayerState
{
    public override void PhysicsUpdate(double delta)
    {
        base.PhysicsUpdate(delta);
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
        if (Input.IsActionJustPressed("use") && Player.CurrentPickableFocusedItem != null)
        {
            StateMachine.ChangeState<PlayerPickUpState>();
            return;
        }
        if (Input.IsActionJustPressed("throw") && Player.HasWeapon())
        {
            StateMachine.ChangeState<PlayerThrowState>();
            return;
        }
        if (Input.IsActionJustPressed("action") && Player.HasWeapon())
        {
            StateMachine.ChangeState<PlayerAttackState>();
            return;
        }
        if (Input.IsActionJustPressed("kick"))
        {
            StateMachine.ChangeState<PlayerKickState>();
            return;
        }
        if (Input.IsActionJustPressed("offhand_action") && Player.HasOffhandEquitment())
        {
            StateMachine.ChangeState<PlayerDefendState>();
            return;
        }
    }

}
