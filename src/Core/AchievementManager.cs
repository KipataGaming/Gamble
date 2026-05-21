using Godot;
using System.Linq;
using Game.Resources;

namespace Game.Core;

public partial class AchievementManager : Node
{
    public static AchievementManager Instance { get; private set; } = null!;

    [Export] public Godot.Collections.Array<AchievementResource> Achievements = new();

    public override void _EnterTree()
    {
        Instance = this;
    }
    // Add this to src/Core/AchievementManager.cs
    public object GetSaveData()
    {
        // Return whatever data structure you want to save
        return new { UnlockedAchievements = "ExampleData" }; 
    }
    public override void _Ready()
    {
        GD.Print("AchievementManager: System Online.");
    }

    public void UpdateProgress(string achievementId, int amount)
    {
        var achievement = Achievements.FirstOrDefault(a => a.Id == achievementId);
        
        if (achievement != null && !achievement.IsComplete)
        {
            achievement.CurrentProgress += amount;
            GD.Print($"[Achievement] {achievement.Title}: {achievement.CurrentProgress}/{achievement.Goal}");
            
            if (achievement.IsComplete)
            {
                GD.Print($"[Achievement] Unlocked: {achievement.Title}");
            }
        }
    }
}