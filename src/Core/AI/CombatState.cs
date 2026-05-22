// src/Core/AI/CombatState.cs
using Godot;
using Game.Core;

namespace Game.Core.AI;

public class CombatState : IState
{
    private readonly EnemyController _enemy;
    private readonly StateMachine _stateMachine;

    public CombatState(EnemyController enemy, StateMachine stateMachine)
    {
        _enemy = enemy;
        _stateMachine = stateMachine;
    }

    public void Enter() { GD.Print("Entering combat..."); }

    public void Update(double delta) { }

    public void PhysicsUpdate(double delta) { }

    public void Exit() { }
}