// src/Bridge/AI/EnemyController.cs
using Godot;
using Game.Core.AI;
using Game.Core; // For IDamageable

namespace Game.Bridge.AI;

public partial class EnemyController : CharacterBody3D, IDamageable
{
    [Export] public NavigationAgent3D NavAgent = null!;
    [Export] public StateMachine Brain = null!;
    [Export] public Area3D DetectionZone = null!;
    [Export] public RayCast3D VisionRay = null!;
    
    // NEW: The Field of View in degrees. 90 means 45 degrees left and 45 degrees right.
    [Export] public float FieldOfView = 90.0f; 
    public float WalkSpeed = 3.0f;
    
    public Node3D PotentialTarget { get; private set; }

    // --- HEALTH PIPELINE ---
    public int Health = 100;

    public override void _Ready()
    {
        if (VisionRay != null) VisionRay.AddException(this);

        DetectionZone.BodyEntered += OnBodyEntered;
        DetectionZone.BodyExited += OnBodyExited;

        Brain.Initialize(new IdleState(this, Brain));
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.IsInGroup("Player")) PotentialTarget = body;
    }

    private void OnBodyExited(Node3D body)
    {
        if (body == PotentialTarget) PotentialTarget = null;
    }

    // --- THE UPGRADED SENSORS ---
    public bool CanSeeTarget()
    {
        if (PotentialTarget == null || !GodotObject.IsInstanceValid(PotentialTarget) || PotentialTarget.IsQueuedForDeletion()) 
            return false;

        // 1. VISION CONE MATH: Are they in front of us?
        // Get the direction from the enemy to the player
        Vector3 directionToTarget = GlobalPosition.DirectionTo(PotentialTarget.GlobalPosition);
        
        // Get the direction the enemy's 3D model is currently facing (-Z is forward in Godot)
        Vector3 forwardDirection = -GlobalTransform.Basis.Z; 

        // Calculate the angle between where we are looking and where the player is
        float angleToTarget = Mathf.RadToDeg(forwardDirection.AngleTo(directionToTarget));

        // If the angle is greater than half our FOV, they are outside our peripheral vision!
        if (angleToTarget > (FieldOfView / 2.0f))
        {
            return false; // We can't see them, even if there are no walls
        }

        // 2. LINE OF SIGHT MATH: Is there a wall in the way?
        VisionRay.TargetPosition = ToLocal(PotentialTarget.GlobalPosition);
        VisionRay.ForceRaycastUpdate(); 

        GodotObject hitObj = VisionRay.GetCollider();
        if (hitObj != null && hitObj == PotentialTarget) return true;
        
        return false;
    }

    // --- DAMAGE PIPELINE ---
    public void TakeDamage(int amount)
    {
        if (Health <= 0) return; 

        Health -= amount;
        GD.Print($"[Enemy] OUCH! Took {amount} damage. Health: {Health}/100");

        if (Health <= 0) Die();
    }

    private void Die()
    {
        GD.Print("[Enemy] UNIT ELIMINATED. Destroying body.");
        QueueFree(); 
    }
}