using Godot;
using Game.Bridge.AI;

namespace Game.Core.AI;

public class CombatState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private readonly Node3D _target;
    
    private float _fireTimer = 0f;
    private readonly float _fireRate = 1.5f; // Shoot every 1.5 seconds

    public CombatState(EnemyController enemy, StateMachine stateMachine, Node3D target)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
        _target = target;
    }

    public void Enter()
    {
        GD.Print("[AI] -> Enemy has stopped moving and is ENGAGING!");
        _fireTimer = _fireRate; // Ready to shoot immediately
    }

    public void Update(double delta)
    {
        _fireTimer += (float)delta;

        if (_fireTimer >= _fireRate)
        {
            _fireTimer = 0f;
            GD.Print("[AI] -> BANG! Enemy fired weapon.");
            
            // THE DAMAGE PIPELINE:
            // Check if the target we are aiming at has the IDamageable contract
            if (_target is IDamageable damageableTarget)
            {
                // If it does, deal 15 damage!
                damageableTarget.TakeDamage(15);
            }
            else
            {
                GD.PrintErr("[AI] -> Target is not damageable!");
            }
        }
    }

    public void PhysicsUpdate(double delta)
    {
        if (!GodotObject.IsInstanceValid(_target)) return;

        // 1. If they run behind a wall, or get further than 10 meters away, go back to chasing
        float distance = _enemy.GlobalPosition.DistanceTo(_target.GlobalPosition);
        
        if (distance > 10.0f || !_enemy.CanSeeTarget())
        {
            _stateMachine.ChangeState(new ChaseState(_enemy, _stateMachine, _target));
            return;
        }

        // 2. Tactical aiming: Make the 3D model physically look at the player
        _enemy.LookAt(_target.GlobalPosition, Vector3.Up);
        // Prevent tilting up/down so they don't fall over
        _enemy.Rotation = new Vector3(0, _enemy.Rotation.Y, 0); 
    }

    public void Exit()
    {
        GD.Print("[AI] -> Cease fire. Target lost or moving.");
    }
}