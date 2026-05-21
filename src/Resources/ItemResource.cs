#nullable enable
using Godot;
namespace Game.Resources;

[GlobalClass]
public partial class ItemResource : Resource
{
	[Export] public string Id { get; private set; } = "";
	[Export] public string Name { get; private set; } = "";
	[Export] public string Description { get; private set; } = "";
	[Export] public int MaxStackSize { get; private set; } = 1;
	[Export] public float Weight { get; private set; } = 0.0f;
	
	// The visual representation for the UI (The Bridge will read this later)
	[Export] public Texture2D? Icon { get; private set; } 
}
