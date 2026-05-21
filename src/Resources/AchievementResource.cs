using Godot;

namespace Game.Resources;

[GlobalClass]
public partial class AchievementResource : Resource
{
    [Export] public string Id { get; set; } = string.Empty;
    [Export] public string Title { get; set; } = string.Empty;
    [Export] public string Description { get; set; } = string.Empty;
    [Export] public int Goal { get; set; } = 0;

    public int CurrentProgress { get; set; } = 0;
    public bool IsComplete => CurrentProgress >= Goal;
}