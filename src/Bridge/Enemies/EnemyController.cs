// src/Core/EnemyController.cs
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

    public int Health = 100;

    private StateMachine _brain;
    private double _staggerTimer = 0.0;           // How long the enemy is "staggered" after being shot
    private bool _isStaggered => _staggerTimer > 0.0;

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

        _staggerTimer -= delta;

        if (NavAgent == null) return;

        if (_isStaggered)
        {
            // During stagger: stop all navigation movement
            NavAgent.TargetPosition = GlobalPosition;
            Velocity = Vector3.Zero;
        }
        else if (NavAgent.IsNavigationFinished())
        {
            Velocity = Vector3.Zero;
        }
        else
        {
            Vector3 nextPos = NavAgent.GetNextPathPosition();
            if (GlobalPosition.DistanceSquaredTo(nextPos) > 0.5f)
            {
                Vector3 direction = GlobalPosition.DirectionTo(nextPos);
                Velocity = direction * WalkSpeed;
            }
            else
            {
                Velocity = Vector3.Zero;
            }
        }

        MoveAndSlide();
    }

    public void SetNavigationTarget(Vector3 targetPosition)
    {
        if (NavAgent != null && !_isStaggered)
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
        if (PotentialTarget == null || !GodotObject.IsInstanceValid(PotentialTarget))
            return false;

        float distance = GlobalPosition.DistanceTo(PotentialTarget.GlobalPosition);
        Vector3 directionToTarget = GlobalPosition.DirectionTo(PotentialTarget.GlobalPosition);
        Vector3 forward = GlobalTransform.Basis.Z;

        float angle = Mathf.RadToDeg(forward.AngleTo(directionToTarget));

        if (angle > FieldOfView / 2f)
            return false;

        if (VisionRay == null)
            return true;

        VisionRay.TargetPosition = ToLocal(PotentialTarget.GlobalPosition);
        VisionRay.ForceRaycastUpdate();

        return VisionRay.GetCollider() == PotentialTarget;
    }

    public void TakeDamage(int amount)
    {
        if (Health <= 0) return;

        Health -= amount;
        GD.Print($"[Enemy] OUCH! Took {amount} damage. Health: {Health}/100");

        // === STRONG HIT REACTION ===
        if (PotentialTarget != null)
        {
            // Face the shooter instantly (only once)
            LookAt(PotentialTarget.GlobalPosition, Vector3.Up);
            RotateY(Mathf.Pi);
        }

        // Stagger the enemy so navigation doesn't fight the rotation
        _staggerTimer = 0.85;   // 0.85 seconds of "hit stun"

        // Force immediate retreat
        _brain?.ChangeState(new RetreatState(this, _brain));

        if (Health <= 0)
            Die();
    }

    private void Die()
    {
        GD.Print("[Enemy] UNIT ELIMINATED.");
        QueueFree();
    }
}