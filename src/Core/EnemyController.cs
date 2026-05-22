using Godot;
using Game.Core.AI;

namespace Game.Bridge.AI;

public partial class EnemyController : CharacterBody3D
{
    [Export] public NavigationAgent3D NavAgent = null!;
    [Export] public StateMachine Brain = null!;
    [Export] public Area3D DetectionZone = null!;
    [Export] public RayCast3D VisionRay = null!;
    
    public float WalkSpeed = 3.0f;
    public Node3D PotentialTarget { get; private set; }

    public override void _Ready()
    {
        // Tell the raycast NOT to hit the enemy's own body!
        if (VisionRay != null) VisionRay.AddException(this);

        // Standard event wiring, but now routing to our custom debug methods
        DetectionZone.BodyEntered += OnBodyEntered;
        DetectionZone.BodyExited += OnBodyExited;

        Brain.Initialize(new IdleState(this, Brain));
    }

    private void OnBodyEntered(Node3D body)
    {
        GD.Print($"[Sensors] Contact detected with: {body.Name}");
        
        if (body.IsInGroup("Player"))
        {
            GD.Print("[Sensors] Contact identified as 'Player'. Locking target.");
            PotentialTarget = body;
        }
        else
        {
            GD.Print($"[Sensors] Ignored {body.Name} (Not in 'Player' group).");
        }
    }

    private void OnBodyExited(Node3D body)
    {
        if (body == PotentialTarget) 
        {
            GD.Print("[Sensors] Target left the detection zone.");
            PotentialTarget = null;
        }
    }

    public bool CanSeeTarget()
    {
        if (PotentialTarget == null) return false;

        VisionRay.TargetPosition = ToLocal(PotentialTarget.GlobalPosition);
        VisionRay.ForceRaycastUpdate(); 

        GodotObject hitObj = VisionRay.GetCollider();
        
        // Debug exactly what the raycast is hitting every frame
        if (hitObj != null)
        {
            if (hitObj == PotentialTarget) return true;
            
            // If you want to see what is blocking the view, uncomment the line below:
             GD.Print($"[Vision] Blocked by: {((Node)hitObj).Name}"); 
        }
        
        return false;
    }
}