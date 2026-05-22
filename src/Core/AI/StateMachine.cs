// src/Core/AI/StateMachine.cs
// Pure C# State Machine - No Godot dependencies

namespace Game.Core.AI;

public class StateMachine
{
    private IState _currentState;

    public IState CurrentState => _currentState;

    public void Initialize(IState startingState)
    {
        if (startingState == null) return;

        _currentState = startingState;
        _currentState.Enter();
    }

    public void ChangeState(IState newState)
    {
        if (newState == null || newState == _currentState) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public void Update(double delta)
    {
        _currentState?.Update(delta);
    }

    public void PhysicsUpdate(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
    }
}