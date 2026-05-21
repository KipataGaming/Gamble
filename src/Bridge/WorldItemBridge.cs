using Godot;
using Game.Resources;

namespace Game.Bridge
{
	public partial class WorldItemBridge : RigidBody3D
	{
		// Removed private set to allow Inspector injection
		[Export] public ItemResource ItemData;

		public ItemResource InteractAndPickup()
		{
			if (ItemData == null)
			{
				GD.PrintErr("[WorldItem] Interaction failed: No ItemData assigned in the inspector!");
				return null;
			}

			GD.Print($"[WorldItem] Player picked up: {ItemData.Name}");

			QueueFree();
			return ItemData;
		}
	}
}
