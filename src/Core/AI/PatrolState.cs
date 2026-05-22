// src/Core/AI/PatrolState.cs
using Godot;
using Game.Bridge.AI;

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

    public void Enter()
    {
        GD.Print("Patrolling...");
        
        // Pick a random location within 5 meters
        Vector3 randomOffset = new Vector3((float)GD.RandRange(-5, 5), 0, (float)GD.RandRange(-5, 5));
        Vector3 targetPosition = _enemy.GlobalPosition + randomOffset;
        
        // Tell the GPS where we want to go
        _enemy.NavAgent.TargetPosition = targetPosition;
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta) 
    { 
        // 1. If we reached the destination, go back to Idle
        if (_enemy.NavAgent.IsNavigationFinished())
        {
            _stateMachine.ChangeState(new IdleState(_enemy, _stateMachine));
            return;
        }

        // 2. Calculate the direction to the next step on the path
        Vector3 currentPos = _enemy.GlobalPosition;
        Vector3 nextPos = _enemy.NavAgent.GetNextPathPosition();
        Vector3 direction = (nextPos - currentPos).Normalized();

        // 3. Move the physical body
        _enemy.Velocity = direction * _enemy.WalkSpeed;
        _enemy.MoveAndSlide();
    }

    public void Exit() { }
}