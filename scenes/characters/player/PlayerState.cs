using Godot;
using System;

public partial class PlayerState : State
{
	protected Player Player => (Player)Actor;

	// 移动与受击共用：主动作（拾取/投掷/攻击/踢/防御）的切换判定收在一处，
	// 返回 true 表示已经切走了，调用方不用再往下走。
	protected bool TryEnterActionState()
	{
		var intent = Player.Intent;

		if (intent.Interact && Player.GetPickableUnderCrosshair() != null)
		{
			StateMachine.ChangeState<PlayerPickUpState>();
			return true;
		}
		if (intent.Throw && Player.HasWeapon())
		{
			StateMachine.ChangeState<PlayerThrowState>();
			return true;
		}
		if (intent.Attack && Player.HasWeapon())
		{
			StateMachine.ChangeState<PlayerAttackState>();
			return true;
		}
		if (intent.Kick)
		{
			StateMachine.ChangeState<PlayerKickState>();
			return true;
		}
		if (intent.Defend && Player.HasOffhandEquitment())
		{
			StateMachine.ChangeState<PlayerDefendState>();
			return true;
		}

		return false;
	}
}
