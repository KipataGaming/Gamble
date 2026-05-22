// src/Core/AI/CoverState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class CoverState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;

    public CoverState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter() { GD.Print("Taking cover..."); }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta) { }

    public void Exit() { }
}