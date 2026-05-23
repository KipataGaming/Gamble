using Godot;

namespace Game.Core;

public partial class PlayerStatsManager : Node
{
    public static PlayerStatsManager Instance { get; private set; } = null!;

    // --- STAMINA ---
    public float MaxStamina { get; private set; } = 100f;
    public float CurrentStamina { get; private set; }

    // --- HEALTH (NEW) ---
    public int MaxHealth { get; private set; } = 100;
    public int CurrentHealth { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
        CurrentStamina = MaxStamina;
        CurrentHealth = MaxHealth; // Initialize full health
    }

    // -------------------------------------------------------------------------
    // STAMINA LOGIC (Unchanged)
    // -------------------------------------------------------------------------
    public bool TryConsumeStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            
            // Tell the UI to update the bar (Assuming you have this in EventBroker)
            // EventBroker.TriggerStaminaChanged(CurrentStamina, MaxStamina);
            
            GD.Print($"[Stats] Stamina consumed: {amount}. Remaining: {CurrentStamina}");
            return true;
        }
        
        GD.Print("[Stats] Not enough stamina! You are exhausted.");
        return false;
    }

    public void RestoreStamina(float amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, MaxStamina);
        // EventBroker.TriggerStaminaChanged(CurrentStamina, MaxStamina);
    }

    // -------------------------------------------------------------------------
    // HEALTH LOGIC (NEW)
    // -------------------------------------------------------------------------
    public void DecreaseHealth(int amount)
    {
        if (CurrentHealth <= 0) return; // Prevent taking damage if already dead

        // Mathf.Max ensures health never drops below exactly 0
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0); 
        
        GD.Print($"[Stats] Health reduced by {amount}. Current Health: {CurrentHealth}");

        // If you want to update a Health Bar later, uncomment this line:
        // EventBroker.TriggerHealthChanged(CurrentHealth, MaxHealth);

        if (CurrentHealth <= 0)
        {
            Die();
        }
    }

    public void RestoreHealth(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
        // EventBroker.TriggerHealthChanged(CurrentHealth, MaxHealth);
    }

    private void Die()
    {
        GD.Print("[Stats] FATAL DAMAGE! Player has died.");
        // Later, we can tell GameStateManager to pause the game and show a "You Died" screen
    }
    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        GD.Print("[Stats] Player stats reset to maximum.");
    }

}