using Godot;

namespace Game.Bridge
{
    public partial class WeaponBridge : Node3D
    {
        // This allows you to attach different weapon types (melee, gun) 
        // to this node in the editor without touching the player controller.
        public void PerformAttack()
        {
            // Implementation logic for your specific weapon
            GD.Print("[WeaponBridge] Attacking...");
            
            // Example: Play animation
            // var anim = GetNode<AnimationPlayer>("AnimationPlayer");
            // anim.Play("Attack");
        }
    }
}