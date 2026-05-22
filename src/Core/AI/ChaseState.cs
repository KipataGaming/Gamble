// src/Core/AI/ChaseState.cs
using Godot;
using Game.Bridge.AI;

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

    public void Enter()
    {
        GD.Print("[AI] -> Target acquired! Engaging pursuit.");
    }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta)
    {
        // CRITICAL FIX: Change state BEFORE returning
        if (!GodotObject.IsInstanceValid(_target) || _target.IsQueuedForDeletion())
        {
            GD.Print("[AI] -> Target lost (Destroyed). Returning to Idle.");
            _stateMachine.ChangeState(new IdleState(_enemy, _stateMachine));
            return;
        }

        float distance = _enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);
        if (distance <= 10.0f && _enemy.CanSeeTarget())
        {
            _stateMachine.ChangeState(new CombatState(_enemy, _stateMachine, _target));
            return;
        }

        _enemy.NavAgent.TargetPosition = _target.GlobalPosition;

        Vector3 currentPos = _enemy.GlobalPosition;
        Vector3 nextPos = _enemy.NavAgent.GetNextPathPosition();
        Vector3 direction = (nextPos - currentPos).Normalized();

        _enemy.Velocity = direction * (_enemy.WalkSpeed * 1.5f);
        _enemy.MoveAndSlide();
    }

    public void Exit()
    {
        GD.Print("[AI] -> Dropping pursuit.");
    }
}