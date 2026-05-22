// src/Core/AI/IState.cs
namespace Game.Core.AI;

public interface IState
{
    void Enter(); 
    void Update(double delta); 
    void PhysicsUpdate(double delta); 
    void Exit(); 
}