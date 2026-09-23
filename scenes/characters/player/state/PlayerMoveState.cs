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

        TryEnterActionState();
    }

}
