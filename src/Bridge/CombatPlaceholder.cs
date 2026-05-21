using Godot;

namespace Game.Bridge
{
    public partial class CombatPlaceholder : WeaponBridge
    {
        [Export] public float AttackRange = 2.0f;
        [Export] public float Damage = 10.0f;
        [Export] public uint CollisionMask = 1; // Update this to match your Enemy layer

        public void PerformAttack()
        {
            GD.Print("[Combat] Grey-box attack performed.");
            
            // Access the direct physics state to perform a frame-perfect raycast
            var spaceState = GetWorld3D().DirectSpaceState;
            
            var query = PhysicsRayQueryParameters3D.Create(
                GlobalPosition, 
                GlobalPosition - GlobalTransform.Basis.Z * AttackRange
            );
            
            query.CollisionMask = CollisionMask;

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                Node collider = (Node)result["collider"];
                GD.Print($"[Combat] You hit: {collider.Name}");

                // Decoupled damage delivery
                if (collider.HasMethod("TakeDamage"))
                {
                    collider.Call("TakeDamage", Damage);
                }
            }
            else
            {
                GD.Print("[Combat] Attack missed.");
            }
        }
    }
}