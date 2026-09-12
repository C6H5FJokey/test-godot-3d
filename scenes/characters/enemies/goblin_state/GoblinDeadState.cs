using Godot;
using System;

public partial class GoblinDeadState : GoblinState
{
    public override void Enter(State previousState)
    {
        FreezeRagdoll();
    }

    private void FreezeRagdoll()
    {
        foreach (var node in Goblin.PhysicalBoneSimulator.GetChildren())
        {
            if (node is PhysicalBone3D physicalBone3D)
            {
                var boneRid = physicalBone3D.GetRid();
                PhysicsServer3D.BodySetState(boneRid, PhysicsServer3D.BodyState.Sleeping, true);
            }
        }
    }
}
