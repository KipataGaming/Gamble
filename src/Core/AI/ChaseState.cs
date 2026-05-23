// src/Core/AI/ChaseState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class ChaseState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private readonly Node3D _target;

    private double _attackTimer = 0.0;
    private readonly double _attackCooldown = 0.5;   // attacks every 0.5 seconds for testing
    private readonly float _attackRange = 3.5f;
    private readonly int _attackDamage = 25;

    public ChaseState(EnemyController enemy, StateMachine stateMachine, Node3D target)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
        _target = target;
    }

    public void Enter()
    {
        GD.Print("[ChaseState] ENTERED - Engaging target!");
        _attackTimer = 0.0;
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        if (_target == null || !GodotObject.IsInstanceValid(_target))
        {
            _stateMachine.ChangeState(new PatrolState(_enemy, _stateMachine));
            return;
        }

        float distance = _enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);
        bool canSee = _enemy.CanSeeTarget();

        // Debug print every frame so we can see the state is actually running
        GD.Print($"[ChaseState] Dist: {distance:F1}m | CanSee: {canSee} | Timer: {_attackTimer:F2}");

        _enemy.SetNavigationTarget(_target.GlobalPosition);

        if (distance <= _attackRange && canSee)
        {
            _attackTimer += delta;

            if (_attackTimer >= _attackCooldown)
            {
                GD.Print($"[Enemy] ATTACKING! Dealing {_attackDamage} damage to player");
                PerformAttack();
                _attackTimer = 0.0;
            }
        }
        else
        {
            _attackTimer = 0.0;
        }

        if (distance > 15f)
        {
            _stateMachine.ChangeState(new PatrolState(_enemy, _stateMachine));
        }
    }

    private void PerformAttack()
    {
        if (_target is IDamageable damageable)
            damageable.TakeDamage(_attackDamage);
        else if (_target.HasMethod("TakeDamage"))
            _target.Call("TakeDamage", _attackDamage);
    }

    public void Exit() { }
}