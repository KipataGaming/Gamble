// src/Core/AI/IdleState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class IdleState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private float _timer = 0f;
    private readonly float _idleDuration = 3.0f;

    public IdleState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter() { GD.Print("Idling..."); _timer = 0f; }

    public void Update(double delta)
    {
        _timer += (float)delta;
        if (_timer >= _idleDuration)
        {
            _stateMachine.ChangeState(new PatrolState(_enemy, _stateMachine));
        }
    }

    public void PhysicsUpdate(double delta) 
    {
        // NEW: Check our sensors before we do anything else
        if (_enemy.PotentialTarget != null && _enemy.CanSeeTarget())
        {
            _stateMachine.ChangeState(new ChaseState(_enemy, _stateMachine, _enemy.PotentialTarget));
            return;
        }
        
        // ... (keep the rest of your existing PhysicsUpdate code below this)
    }
    public void Exit() { }
}