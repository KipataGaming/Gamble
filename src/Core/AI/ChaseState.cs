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
        if (_target == null || !GodotObject.IsInstanceValid(_target))
        {
            _stateMachine.ChangeState(new PatrolState(_enemy, _stateMachine));
            return;
        }

        // Continuously update navigation target to player's position
        _enemy.SetNavigationTarget(_target.GlobalPosition);

        // Optional: Add simple distance check to give up chase
        float distance = _enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);
        if (distance > 15f)
        {
            _stateMachine.ChangeState(new PatrolState(_enemy, _stateMachine));
        }
    }

    public void Exit() { }
}