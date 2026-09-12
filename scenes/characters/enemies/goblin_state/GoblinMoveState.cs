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
        // foreach (var player in GameState.Instance.Players)
        // {
        //     if (Goblin.Player == null)
        //     {
        //         Goblin.Player = player;
        //     }
        //     else
        //     {
        //         if (player.GlobalPosition.DistanceSquaredTo(Goblin.GlobalPosition) < Goblin.Player.GlobalPosition.DistanceSquaredTo(Goblin.GlobalPosition))
        //         {
        //             Goblin.Player = player;
        //         }
        //     }
        // }
        if (Goblin.Velocity.IsZeroApprox())
        {
            Goblin.Anim.Play("idle");
        }
        else
        {
            Goblin.Anim.Play("run");
        }
        Goblin.ProcessMovement(delta);

        if (Goblin.Player != null)
        {
            if (Goblin.IsPlayerWithinReach() && _canAttack())
            {
                StateMachine.ChangeState<GoblinAttackState>();
                return;
            }
            else
            {
                Goblin.NavigationAgent3D.TargetPosition = Goblin.Player.GlobalPosition;
                var targetPosition = Goblin.NavigationAgent3D.GetNextPathPosition();
                var targetBasis = Goblin.GlobalTransform.LookingAt(targetPosition with { Y = Goblin.GlobalPosition.Y }).Basis.Orthonormalized();
                Goblin.GlobalBasis = Goblin.GlobalBasis.Orthonormalized().Slerp(targetBasis, (float)delta * 4);

                var forward = -Goblin.GlobalBasis.Z;
                forward.Y = 0;
                forward = forward.Normalized();

                var velocity = forward * Goblin.Speed;
                velocity.Y = Goblin.Velocity.Y;

                Goblin.Velocity = velocity;
            }
        }
    }

    private bool _canAttack()
    {
        return Time.GetTicksMsec() - Goblin.TimeSinceLastAttack > Goblin.DurationBetweenAttacks;
    }
}
