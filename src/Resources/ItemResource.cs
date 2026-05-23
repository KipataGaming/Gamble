using Godot;

namespace Game.Resources

{
	public enum ItemRarity
	{
		RoyalBlue = 1,    // Tier 1 - Blue
		Silver = 2,       // Tier 2
		Gold = 3,         // Tier 3
		RoyalPurple = 4,  // Tier 4 - Purple
		Emerald = 5,      // Tier 5
		Crimson = 6       // Tier 6
	}

	public partial class ItemResource : Resource
	{
		[Export] public string Id { get; set; } = "";
		[Export] public string Name { get; set; } = "";
		[Export] public Texture2D Icon { get; set; }
		[Export] public float Weight { get; set; } = 1.0f;

		[Export] public ItemRarity Rarity { get; set; } = ItemRarity.RoyalBlue;

		public float GetValueMultiplier()
		{
			return Rarity switch
			{
				ItemRarity.RoyalBlue => 1.0f,
				ItemRarity.Silver => 1.5f,
				ItemRarity.Gold => 2.5f,
				ItemRarity.RoyalPurple => 4.0f,
				ItemRarity.Emerald => 6.0f,
				ItemRarity.Crimson => 10.0f,
				_ => 1.0f
			};
		}
		public ItemResource GetItemData(string itemId)
{
	return itemId switch
	{
		"wood"  => GD.Load<ItemResource>("res://Resources/wood.tres"),
		"rock"  => GD.Load<ItemResource>("res://Resources/rock.tres"),
		"melon" => GD.Load<ItemResource>("res://Resources/melon.tres"),
		_       => null
	};
}
	}
}
