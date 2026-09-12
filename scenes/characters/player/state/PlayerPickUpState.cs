using Godot;
using System;

public partial class PlayerPickUpState : PlayerState
{
    public override void Enter(State previousState)
    {
        base.Enter(previousState);
        var pickableItem = Player.CurrentPickableFocusedItem;
        if (pickableItem != null)
        {
            Player.EquipmentComponent.Equip(pickableItem.EquipmentData, pickableItem.GlobalTransform);
            pickableItem.QueueFree();
            Player.CurrentPickableFocusedItem = null;
        }
        StateMachine.ChangeState<PlayerMoveState>();
    }

}
