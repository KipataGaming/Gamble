// src/Core/AI/ChaseState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class ChaseState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private readonly Node3D _target;

    public ChaseState(EnemyController enemy, StateMachine stateMachine, Node3D target)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
        _target = target;
    }

    public void Enter() { GD.Print("Chasing target..."); }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        // TODO: Implement chase logic
    }

    public void Exit() { }
}