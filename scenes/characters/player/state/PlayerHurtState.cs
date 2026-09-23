using Godot;
using System;

// 玩家受击：不夺走操作权——照常能移动、跳跃、攻击/踢/投掷/拾取/防御，
// 副作用只是行动变慢（Player 上按 HurtSlowDuration 衰减的 SpeedMultiplier）。
// 播完受击动画回到移动状态；期间接手了主动作就直接切走。
// 伤害已经在 Player.TryReceiveHit 里结算过了。
public partial class PlayerHurtState : PlayerState
{
    private bool _isConnectedAnimationFinished = false;

    public override void Enter(State previousState)
    {
        Player.ApplyHurtSlow();
        Player.Anim.Play("hurt");
        if (!_isConnectedAnimationFinished)
        {
            Player.Anim.AnimationFinished += _animationFinished;
            _isConnectedAnimationFinished = true;
        }
    }

    public override void PhysicsUpdate(double delta)
    {
        Player.CheckJumpInput();
        Player.ProcessMovement(delta);
        TryEnterActionState();
    }

    public override void Exit(State nextState)
    {
        if (_isConnectedAnimationFinished)
        {
            Player.Anim.AnimationFinished -= _animationFinished;
            _isConnectedAnimationFinished = false;
        }
    }

    private void _animationFinished(StringName animName)
    {
        if (animName == "hurt")
        {
            StateMachine.ChangeState<PlayerMoveState>();
        }
    }
}
