// src/Core/AI/RetreatState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class RetreatState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private double _retreatTimer = 0;
    private readonly double _retreatDuration = 5.0;
    private readonly float _healPerSecond = 10f;

    public RetreatState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter()
    {
        GD.Print("[Enemy] Retreating!");
        _retreatTimer = 0;

        // Run to a position away from the player
        if (_enemy.PotentialTarget != null)
        {
            Vector3 directionAway = (_enemy.GlobalPosition - _enemy.PotentialTarget.GlobalPosition).Normalized();
            Vector3 retreatPosition = _enemy.GlobalPosition + directionAway * 12f;
            _enemy.SetNavigationTarget(retreatPosition);
        }
        else
        {
            // Fallback: run in a random direction
            Vector3 randomDir = new Vector3((float)GD.RandRange(-1, 1), 0, (float)GD.RandRange(-1, 1)).Normalized();
            Vector3 retreatPosition = _enemy.GlobalPosition + randomDir * 10f;
            _enemy.SetNavigationTarget(retreatPosition);
        }
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        _retreatTimer += delta;

        // Simple heal over time
        _enemy.Health = Mathf.Min(_enemy.Health + (int)(_healPerSecond * delta), 100);

        // End retreat after duration
        if (_retreatTimer >= _retreatDuration)
        {
            if (_enemy.PotentialTarget != null && _enemy.CanSeeTarget())
            {
                _stateMachine.ChangeState(new ChaseState(_enemy, _stateMachine, _enemy.PotentialTarget));
            }
            else
            {
                _stateMachine.ChangeState(new PatrolState(_enemy, _stateMachine));
            }
        }
    }

    public void Exit() { }
}