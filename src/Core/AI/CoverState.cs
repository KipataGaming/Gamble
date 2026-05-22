// src/Core/AI/CoverState.cs
using Godot;
using Game.Bridge.AI;
using System.Linq;

namespace Game.Core.AI;

public class CoverState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;
    private readonly Node3D _attacker;
    private float _coverDuration = 3.0f;
    private float _timer = 0f;

    public CoverState(EnemyController enemy, StateMachine stateMachine, Node3D attacker)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
        _attacker = attacker;
    }

    public void Enter()
    {
        GD.Print("[AI] -> Taking cover!");
        
        // Find all cover objects in the level
        var coverNodes = _enemy.GetTree().GetNodesInGroup("Cover").Cast<Node3D>();
        
        // Find the closest cover to the enemy
        Node3D closestCover = coverNodes.OrderBy(c => c.GlobalPosition.DistanceTo(_enemy.GlobalPosition)).FirstOrDefault();

        if (closestCover != null)
        {
            _enemy.NavAgent.TargetPosition = closestCover.GlobalPosition;
        }
    }

    public void Update(double delta) 
    {
        _timer += (float)delta;
        if (_timer >= _coverDuration)
        {
            _stateMachine.ChangeState(new CombatState(_enemy, _stateMachine, _attacker));
        }
    }

    public void PhysicsUpdate(double delta) 
    {
        // Simple movement to cover
        Vector3 direction = (_enemy.NavAgent.GetNextPathPosition() - _enemy.GlobalPosition).Normalized();
        _enemy.Velocity = direction * _enemy.WalkSpeed;
        _enemy.MoveAndSlide();
    }

    public void Exit() { }
}