// src/Core/AI/IdleState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class IdleState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private double _timer = 0;
    private readonly double _idleDuration = 3.0;

    public IdleState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter() 
    { 
        GD.Print("Idling..."); 
        _timer = 0; 
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta) 
    {
        _timer += delta;

        if (_timer >= _idleDuration)
        {
            _stateMachine.ChangeState(new PatrolState(_enemy, _stateMachine));
            return;
        }

        // Check for player while idling
        if (_enemy.PotentialTarget != null && _enemy.CanSeeTarget())
        {
            _stateMachine.ChangeState(new ChaseState(_enemy, _stateMachine, _enemy.PotentialTarget));
        }
    }

    public void Exit() { }
}