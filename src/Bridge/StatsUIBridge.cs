using Godot;
using Game.Core;

namespace Game.Bridge;

// Notice we changed this from Control to ProgressBar
public partial class StatsUIBridge : ProgressBar 
{
    public override void _Ready()
    {
        EventBroker.OnStaminaChanged += UpdateStaminaUI;
        
        // Force the bar to update the moment the game starts
        if (PlayerStatsManager.Instance != null)
        {
            UpdateStaminaUI(PlayerStatsManager.Instance.CurrentStamina, PlayerStatsManager.Instance.MaxStamina);
        }
    }

    public override void _ExitTree()
    {
        EventBroker.OnStaminaChanged -= UpdateStaminaUI;
    }

    private void UpdateStaminaUI(float current, float max)
    {
        // Because this script IS the progress bar, we just update ourselves!
        MaxValue = max;
        Value = current;
    }
}