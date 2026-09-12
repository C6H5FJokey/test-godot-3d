using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class StateMachine : Node
{
    private const int MAX_TRANSITION_PER_TICK = 8;

    [Export] private State _initialState;

    private Dictionary<Type, State> _states = [];
    [Export] private State _currentState;

    private State _pendingState;
    private object _pendingData;
    private bool _isApplyingTransition;

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

    public void Tick(double delta)
    {
        _currentState?.Update(delta);
        ApplyPendingTransition();
    }

    public void TickPhysics(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
        ApplyPendingTransition();
    }

    public void ChangeState(State nextState, object data = null)
    {
        if (nextState == null)
        {
            return;
        }

        _pendingState = nextState;
        _pendingData = data;
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

    private void ApplyPendingTransition()
    {
        if (_isApplyingTransition)
        {
            return;
        }

        _isApplyingTransition = true;
        try
        {
            var transitionCount = 0;
            while (_pendingState != null)
            {
                if (++transitionCount > MAX_TRANSITION_PER_TICK)
                {
                    GD.PushError($"{Actor?.Name} keeps changing state without settling, last state is {_pendingState.GetType().Name}.");
                    _pendingState = null;
                    _pendingData = null;
                    break;
                }

                var nextState = _pendingState;
                var data = _pendingData;
                _pendingState = null;
                _pendingData = null;

                var previousState = _currentState;

                previousState?.Exit(nextState);

                _currentState = nextState;

                _currentState.Enter(previousState, data);
            }
        }
        finally
        {
            _isApplyingTransition = false;
        }
    }

}
