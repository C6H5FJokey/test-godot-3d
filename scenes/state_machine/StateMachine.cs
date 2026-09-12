using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class StateMachine : Node
{
    [Export] private State _initialState;

    private Dictionary<Type, State> _states = [];
    [Export] private State _currentState;

    public Node Actor { get; private set; }

    public State CurrentState => _currentState;

    public override void _Ready()
    {
        Actor = GetParent();

        foreach (Node child in GetChildren())
        {
            if (child is State state)
            {
                state.Initialize(this);
                if (_states.ContainsKey(state.GetType()))
                {
                    GD.PushError($"State {state.GetType()} exist repeatly.");
                    return;
                }
                _states[state.GetType()] = state;
            }
        }

        if (_initialState != null)
        {  
            Actor.Ready += () => ChangeState(_initialState);
        }
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
    }

    public void ChangeState(State nextState, object data = null)
    {
        if (nextState == null || nextState == _currentState)
            return;

        State previousState = _currentState;

        _currentState?.Exit(nextState);

        _currentState = nextState;

        _currentState.Enter(previousState, data);
    }

    public void ChangeState<T>(object data = null) where T : State
    {
        if (!_states.TryGetValue(typeof(T), out var nextState))
        {
            GD.PushError($"State {typeof(T).Name} does not exist.");
            return;
        }
        ChangeState(nextState, data);
    }

}