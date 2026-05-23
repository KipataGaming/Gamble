using Godot;

namespace Game.Core;

public partial class PlayerStatsManager : Node
{
    public static PlayerStatsManager Instance { get; private set; } = null!;

    public float MaxStamina { get; private set; } = 100f;
    public float CurrentStamina { get; private set; }

    public int MaxHealth { get; private set; } = 100;
    public int CurrentHealth { get; private set; }

    
    public int CurrentMoney { get; private set; } = 0;

    public override void _EnterTree()
    {
        Instance = this;
        CurrentStamina = MaxStamina;
        CurrentHealth = MaxHealth;
        CurrentMoney = 0;
    }

    public bool TryConsumeStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            return true;
        }
        return false;
    }

    public void RestoreStamina(float amount)
    {
        CurrentStamina = Mathf.Min(CurrentStamina + amount, MaxStamina);
    }

    public void DecreaseHealth(int amount)
    {
        if (CurrentHealth <= 0) return;
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
    }

    public void RestoreHealth(int amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, MaxHealth);
    }

    public void ResetStats()
    {
        CurrentHealth = MaxHealth;
        CurrentStamina = MaxStamina;
        CurrentMoney = 0;
    }

    // ===== NEW CURRENCY SYSTEM =====
    public void AddMoney(int amount)
    {
        if (amount <= 0) return;
        CurrentMoney += amount;
        GD.Print($"[Money] +{amount} (Total: {CurrentMoney})");
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0) return false;

        if (CurrentMoney >= amount)
        {
            CurrentMoney -= amount;
            GD.Print($"[Money] -{amount} (Total: {CurrentMoney})");
            return true;
        }

        GD.Print("[Money] Not enough money!");
        return false;
    }
}