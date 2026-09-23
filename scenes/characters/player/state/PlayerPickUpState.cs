using Godot;
using System;

public partial class PlayerPickUpState : PlayerState
{
    public override void Enter(State previousState)
    {
        base.Enter(previousState);
        var pickableItem = Player.GetPickableUnderCrosshair();
        if (pickableItem != null)
        {
            Player.EquipmentComponent.Equip(pickableItem.EquipmentData, pickableItem.GlobalTransform);
            pickableItem.QueueFree();
        }
        StateMachine.ChangeState<PlayerMoveState>();
    }

}
