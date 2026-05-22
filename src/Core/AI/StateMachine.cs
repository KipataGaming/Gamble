// src/Core/AI/StateMachine.cs
using Godot;

namespace Game.Core.AI;

public partial class StateMachine : Node
{
    private IState _currentState;

    // Call this from your Enemy's _Ready() function to kick things off
    public void Initialize(IState startingState)
    {
        _currentState = startingState;
        _currentState?.Enter();
    }

    // The magic method that swaps behaviors
    public void ChangeState(IState newState)
    {
        if (newState == null) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter();
    }

    public override void _Process(double delta)
    {
        _currentState?.Update(delta);
    }

    public override void _PhysicsProcess(double delta)
    {
        _currentState?.PhysicsUpdate(delta);
    }
}