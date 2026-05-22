using Godot;

namespace Game.Core;

public partial class PlayerStatsManager : Node
{
    public static PlayerStatsManager Instance { get; private set; } = null!;

    public float MaxStamina { get; private set; } = 100f;
    public float CurrentStamina { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
        CurrentStamina = MaxStamina;
    }

    public bool TryConsumeStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            
            // Tell the UI to update the bar
            EventBroker.TriggerStaminaChanged(CurrentStamina, MaxStamina);
            
            GD.Print($"[Stats] Stamina consumed: {amount}. Remaining: {CurrentStamina}");
            return true;
        }
        
        GD.Print("[Stats] Not enough stamina! You are exhausted.");
        return false;
    }

    public void RestoreStamina(float amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, MaxStamina);
        EventBroker.TriggerStaminaChanged(CurrentStamina, MaxStamina);
    }
}