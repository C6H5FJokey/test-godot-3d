using Godot;
using System;

// 玩家死亡：停住身体、放下武器，画面交给 UI 的死亡界面（GameEvents.PlayerDied）。
public partial class PlayerDyingState : PlayerState
{
    public override void Enter(State previousState)
    {
        Player.Velocity = Vector3.Zero;
        Player.EquipmentComponent.DropWeapon();
        Player.Anim.Play("hurt"); // TODO(接口预留)：死亡动画到位后改成 Play("death")
    }
}
