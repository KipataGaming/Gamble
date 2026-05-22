// src/Core/AI/ChaseState.cs
using Godot;
using Game.Bridge.AI;
using Game.Core.AI;
using Game.Core;
using Game.Bridge;

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
        // Safety check in case the player gets deleted/dies
        if (!GodotObject.IsInstanceValid(_target)) return;

        // 1. NEW: If we are within 10 meters and have a clear shot, open fire!
        float distance = _enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);
        if (distance <= 10.0f && _enemy.CanSeeTarget())
        {
            _stateMachine.ChangeState(new CombatState(_enemy, _stateMachine, _target));
            return;
        }

        // 2. Constantly update the GPS to the target's current position
        _enemy.NavAgent.TargetPosition = _target.GlobalPosition;

        // 3. Calculate the path
        Vector3 currentPos = _enemy.GlobalPosition;
        Vector3 nextPos = _enemy.NavAgent.GetNextPathPosition();
        Vector3 direction = (nextPos - currentPos).Normalized();

        // 4. Move the physical body
        _enemy.Velocity = direction * _enemy.WalkSpeed;
        _enemy.MoveAndSlide();
    }

    public void Exit()
    {
        GD.Print("[AI] -> Target lost. Returning to idle.");
    }
}   