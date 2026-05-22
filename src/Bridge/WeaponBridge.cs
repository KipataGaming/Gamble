using Godot;
using Game.Core; // Required to find the IDamageable interface

namespace Game.Bridge
{
	public partial class WeaponBridge : Node3D
	{
		[Export] public string ToolCategory { get; set; } = "Weapon";
		
		// NEW: The physical laser beam the gun fires to detect hits
		[Export] public RayCast3D AimRay = null!; 
		
		// NEW: How much damage this specific weapon does
		[Export] public int Damage = 25; 

		public void PerformAttack()
		{
			GD.Print($"[{ToolCategory}] Firing!");

			if (AimRay == null) 
			{
				GD.PrintErr("AimRay is missing from the WeaponBridge!");
				return;
			}

			// Force the raycast to check exactly what it is pointing at right now
			AimRay.ForceRaycastUpdate();
			
			if (AimRay.IsColliding())
			{
				// What did we hit?
				GodotObject hitObject = AimRay.GetCollider();
				
				// THE CONTRACT: Does the thing we hit know how to take damage?
				if (hitObject is IDamageable damageableTarget)
				{
					// Hit! Deal the damage!
					damageableTarget.TakeDamage(Damage);
					GD.Print($"[{ToolCategory}] Hit confirmed! Dealt {Damage} damage.");
				}
				else
				{
					// We hit a wall, the floor, etc.
					GD.Print($"[{ToolCategory}] Hit environment.");
				}
			}
		}
	}
}
