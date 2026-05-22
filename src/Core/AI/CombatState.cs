// src/Core/AI/CombatState.cs
using Godot;
using Game.Bridge.AI;
using Game.Core;

namespace Game.Core.AI;

public class CombatState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private readonly Node3D _target;
    
    private float _fireTimer = 0f;
    private readonly float _fireRate = 1.5f; 

    public CombatState(EnemyController enemy, StateMachine stateMachine, Node3D target)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
        _target = target;
    }

    public void Enter()
    {
        GD.Print("[AI] -> Enemy has stopped moving and is ENGAGING!");
        _fireTimer = _fireRate; 
    }

    public void Update(double delta)
    {
        // 1. SAFETY: Stop ticking the gun timer if the target is dead!
        if (!GodotObject.IsInstanceValid(_target) || _target.IsQueuedForDeletion()) return;

        _fireTimer += (float)delta;

        if (_fireTimer >= _fireRate)
        {
            _fireTimer = 0f;
            GD.Print("[AI] -> BANG! Enemy fired weapon.");
            
            if (_target is IDamageable damageableTarget)
            {
                damageableTarget.TakeDamage(15);
            }
        }
    }

    public void PhysicsUpdate(double delta)
    {
        // 2. CRITICAL FIX: Change state BEFORE returning
        if (!GodotObject.IsInstanceValid(_target) || _target.IsQueuedForDeletion())
        {
            GD.Print("[AI] -> Target destroyed. Returning to Idle.");
            _stateMachine.ChangeState(new IdleState(_enemy, _stateMachine));
            return;
        }

        float distance = _enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);
        
        if (distance > 10.0f || !_enemy.CanSeeTarget())
        {
            _stateMachine.ChangeState(new ChaseState(_enemy, _stateMachine, _target));
            return;
        }

        _enemy.LookAt(_target.GlobalPosition, Vector3.Up);
        _enemy.Rotation = new Vector3(0, _enemy.Rotation.Y, 0); 
    }

    public void Exit()
    {
        GD.Print("[AI] -> Cease fire.");
    }
}