using Godot;
using System;

public partial class PlayerThrowState : PlayerState
{
    public override void Enter(State previousState)
    {
        base.Enter(previousState);

        Player.ThrowWeapon();
        StateMachine.ChangeState<PlayerMoveState>();
    }
}
