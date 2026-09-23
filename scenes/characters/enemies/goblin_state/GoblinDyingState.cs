using Godot;
using System;

public partial class GoblinDyingState : GoblinState
{
    const double DURATION_RAGDOLL_SIMULATION = 3.0;

    public override void Enter(State previousState, object data)
    {

        Goblin.StartRagdoll();
        if (data is GoblinDyingStateData stateData)
        {
            Goblin.PhysicalBoneBody.ApplyImpulse(stateData.Impulse);
        }
        Goblin.DropWeapon();

        HitSpurt.CreateHitSpurt(Goblin.PhysicalBoneBody.GlobalPosition);

        var timer = GetTree().CreateTimer(DURATION_RAGDOLL_SIMULATION);
        timer.Timeout += () => StateMachine.ChangeState<GoblinDeadState>();
    }

}

public partial class GoblinDyingStateData
{
    public Vector3 Impulse {get; set;}

}
