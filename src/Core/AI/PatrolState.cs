// src/Core/AI/PatrolState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class PatrolState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private double _patrolTimer = 0;
    private readonly double _patrolInterval = 4.0;

    public PatrolState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter()
    {
        GD.Print("Patrolling...");
        _patrolTimer = 0;
        PickNewPatrolPoint();
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        _patrolTimer += delta;

        // === DEBUG: Check for player every frame ===
        if (_enemy.PotentialTarget != null && _enemy.CanSeeTarget())
        {
            GD.Print("[PatrolState] → Player seen! Switching to Chase");
            _stateMachine.ChangeState(new ChaseState(_enemy, _stateMachine, _enemy.PotentialTarget));
            return;
        }
        else if (_enemy.PotentialTarget != null)
        {
            GD.Print("[PatrolState] Player nearby but not visible");
        }

        // Pick new point every few seconds
        if (_patrolTimer >= _patrolInterval)
        {
            PickNewPatrolPoint();
            _patrolTimer = 0;
        }
    }

    private void PickNewPatrolPoint()
    {
        Vector3 randomOffset = new Vector3(
            (float)GD.RandRange(-8, 8),
            0,
            (float)GD.RandRange(-8, 8)
        );

        Vector3 targetPos = _enemy.GlobalPosition + randomOffset;
        _enemy.SetNavigationTarget(targetPos);
    }

    public void Exit() { }
}