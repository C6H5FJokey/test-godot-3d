using Godot;
using System;

public abstract partial class State : Node
{
    protected StateMachine StateMachine { get; private set; }

    protected Node Actor => StateMachine.Actor;

    public void Initialize(StateMachine stateMachine)
    {
        StateMachine = stateMachine;
    }

    public virtual void Enter(State previousState) { }

    public virtual void Enter(State previousState, object data)
    {
        Enter(previousState);
    }

    public virtual void Exit(State nextState) { }

    public virtual void Update(double delta) { }

    public virtual void PhysicsUpdate(double delta) { }
}

public interface IStateData
{
    
}