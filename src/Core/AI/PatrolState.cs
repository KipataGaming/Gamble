// src/Core/AI/PatrolState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class PatrolState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;

    public PatrolState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter() { GD.Print("Patrolling..."); }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta) { }

    public void Exit() { }
}