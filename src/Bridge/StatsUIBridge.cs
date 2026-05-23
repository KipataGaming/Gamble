// src/Bridge/StatsUIBridge.cs
using Godot;
using Game.Core;

namespace Game.Bridge;

public partial class StatsUIBridge : Control
{
	[Export] public ProgressBar HealthBar = null!;
	[Export] public ProgressBar StaminaBar = null!;
	[Export] public Label HealthLabel = null!;
	[Export] public Label StaminaLabel = null!;

	public override void _Ready()
	{
		EventBroker.OnStaminaChanged += UpdateStaminaUI;
		UpdateAllUI();
	}

	public override void _ExitTree()
	{
		EventBroker.OnStaminaChanged -= UpdateStaminaUI;
	}

	public override void _Process(double delta)
	{
		UpdateAllUI();
	}

	private void UpdateAllUI()
	{
		if (PlayerStatsManager.Instance == null) return;

		// Health
		if (HealthBar != null)
		{
			HealthBar.MaxValue = PlayerStatsManager.Instance.MaxHealth;
			HealthBar.Value = PlayerStatsManager.Instance.CurrentHealth;
		}
		if (HealthLabel != null)
			HealthLabel.Text = $"HEALTH  {PlayerStatsManager.Instance.CurrentHealth} / {PlayerStatsManager.Instance.MaxHealth}";

		// Stamina
		if (StaminaBar != null)
		{
			StaminaBar.MaxValue = PlayerStatsManager.Instance.MaxStamina;
			StaminaBar.Value = PlayerStatsManager.Instance.CurrentStamina;
		}
		if (StaminaLabel != null)
			StaminaLabel.Text = $"STAMINA  {PlayerStatsManager.Instance.CurrentStamina:F0} / {PlayerStatsManager.Instance.MaxStamina:F0}";
	}

	private void UpdateStaminaUI(float current, float max)
	{
		if (StaminaBar != null)
		{
			StaminaBar.MaxValue = max;
			StaminaBar.Value = current;
		}
	}
}
