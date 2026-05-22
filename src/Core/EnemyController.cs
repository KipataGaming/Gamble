// src/Core/EnemyController.cs
// Bridge layer - owns physical representation, sensors, and drives the Brain

using Godot;
using Game.Core.AI;

namespace Game.Core;

public partial class EnemyController : CharacterBody3D, IDamageable
{
    [Export] public NavigationAgent3D NavAgent = null!;
    [Export] public Area3D DetectionZone = null!;
    [Export] public RayCast3D VisionRay = null!;
    
    [Export] public float FieldOfView = 90.0f;
    [Export] public float WalkSpeed = 3.0f;

    public Node3D PotentialTarget { get; private set; }

    // Health
    public int Health = 100;

    private StateMachine _brain;

    public override void _Ready()
    {
        if (VisionRay != null)
            VisionRay.AddException(this);

        DetectionZone.BodyEntered += OnBodyEntered;
        DetectionZone.BodyExited += OnBodyExited;

        _brain = new StateMachine();
        _brain.Initialize(new IdleState(this, _brain));
    }

    public override void _PhysicsProcess(double delta)
    {
        _brain?.PhysicsUpdate(delta);

        if (NavAgent == null)
            return;

        if (NavAgent.IsNavigationFinished())
        {
            Velocity = Vector3.Zero;
        }
        else
        {
            Vector3 nextPos = NavAgent.GetNextPathPosition();
            Vector3 direction = GlobalPosition.DirectionTo(nextPos);
            Velocity = direction * WalkSpeed;
        }

        MoveAndSlide();
    }

    public void SetNavigationTarget(Vector3 targetPosition)
    {
        if (NavAgent != null)
        {
            NavAgent.TargetPosition = targetPosition;
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.IsInGroup("Player"))
            PotentialTarget = body;
    }

    private void OnBodyExited(Node3D body)
    {
        if (body == PotentialTarget)
            PotentialTarget = null;
    }

    public bool CanSeeTarget()
    {
        if (PotentialTarget == null || 
            !GodotObject.IsInstanceValid(PotentialTarget) || 
            PotentialTarget.IsQueuedForDeletion())
            return false;

        Vector3 directionToTarget = GlobalPosition.DirectionTo(PotentialTarget.GlobalPosition);
        Vector3 forwardDirection = -GlobalTransform.Basis.Z;

        float angleToTarget = Mathf.RadToDeg(forwardDirection.AngleTo(directionToTarget));

        if (angleToTarget > (FieldOfView / 2.0f))
            return false;

        VisionRay.TargetPosition = ToLocal(PotentialTarget.GlobalPosition);
        VisionRay.ForceRaycastUpdate();

        GodotObject hitObj = VisionRay.GetCollider();
        return hitObj != null && hitObj == PotentialTarget;
    }

    public void TakeDamage(int amount)
    {
        if (Health <= 0) return;

        Health -= amount;
        GD.Print($"[Enemy] OUCH! Took {amount} damage. Health: {Health}/100");

        if (Health <= 0)
            Die();
    }

    private void Die()
    {
        GD.Print("[Enemy] UNIT ELIMINATED. Destroying body.");
        QueueFree();
    }
}