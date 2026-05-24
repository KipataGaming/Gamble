using Godot;
using Game.Core;

public partial class BlackjackUI : Control
{
	[Export] public Label MoneyDisplay;
	[Export] public Label PlayerHandText;
	[Export] public Label DealerHandText;
	[Export] public LineEdit BetInput;
	[Export] public Button PlaceBetButton;
	[Export] public Button HitButton;
	[Export] public Button StandButton;
	[Export] public Button DoubleButton;
	[Export] public Button NewRoundButton;
	[Export] public Button RebetButton;
	[Export] public Button ExitButton;
	[Export] public Label ResultLabel;

	private BlackjackGame _game = new BlackjackGame();
	private int _lastBet = 0;

	public override void _Ready()
	{
		// Safe button connections
		if (PlaceBetButton != null) PlaceBetButton.Pressed += OnPlaceBetPressed;
		if (HitButton != null)      HitButton.Pressed += OnHitPressed;
		if (StandButton != null)    StandButton.Pressed += OnStandPressed;
		if (DoubleButton != null)   DoubleButton.Pressed += OnDoublePressed;
		if (NewRoundButton != null) NewRoundButton.Pressed += OnNewRoundPressed;
		if (RebetButton != null)    RebetButton.Pressed += OnRebetPressed;
		if (ExitButton != null)     ExitButton.Pressed += OnExitPressed;
		{
	// === DEBUG: Give player money for testing Blackjack ===
	PlayerStatsManager.Instance.AddMoney(1000);

	// ... rest of your button connections
}
		UpdateUI();
		SetGameActive(false);

		if (NewRoundButton != null) NewRoundButton.Visible = false;
		if (RebetButton != null)    RebetButton.Visible = false;
	}

	private void OnPlaceBetPressed()
	{
		if (!int.TryParse(BetInput.Text, out int bet) || bet <= 0)
		{
			if (ResultLabel != null) ResultLabel.Text = "Please enter a valid bet amount.";
			return;
		}

		if (!PlayerStatsManager.Instance.TrySpend(bet))
		{
			if (ResultLabel != null) ResultLabel.Text = "Not enough money!";
			return;
		}

		_lastBet = bet;
		_game.StartNewRound(bet);
		UpdateUI();
		SetGameActive(true);
		if (ResultLabel != null) ResultLabel.Text = "";
		if (NewRoundButton != null) NewRoundButton.Visible = false;
		if (RebetButton != null)    RebetButton.Visible = false;
	}

	private void OnHitPressed()
	{
		_game.PlayerHand.Add(_game.DrawCard());
		UpdateUI();

		if (_game.IsBust(_game.PlayerHand))
			EndRound();
	}

	private void OnStandPressed()
	{
		_game.DealerPlay();
		UpdateUI();
		EndRound();
	}

	private void OnDoublePressed()
	{
		if (_game.CanDoubleDown && PlayerStatsManager.Instance.TrySpend(_game.CurrentBet))
		{
			_game.DoubleDown();
			UpdateUI();
			EndRound();
		}
		else if (_game.CanDoubleDown)
		{
			if (ResultLabel != null) ResultLabel.Text = "Not enough money to double!";
		}
	}

	private void OnNewRoundPressed()
	{
		if (ResultLabel != null) ResultLabel.Text = "";
		if (PlayerHandText != null) PlayerHandText.Text = "You: ";
		if (DealerHandText != null) DealerHandText.Text = "Dealer: ";
		if (NewRoundButton != null) NewRoundButton.Visible = false;
		if (RebetButton != null)    RebetButton.Visible = false;

		if (PlaceBetButton != null) PlaceBetButton.Disabled = false;
		if (BetInput != null)       BetInput.Editable = true;
		if (DoubleButton != null)   DoubleButton.Disabled = true;
	}

	private void OnRebetPressed()
	{
		if (_lastBet <= 0)
		{
			if (ResultLabel != null) ResultLabel.Text = "No previous bet.";
			return;
		}

		if (!PlayerStatsManager.Instance.TrySpend(_lastBet))
		{
			if (ResultLabel != null) ResultLabel.Text = "Not enough money to rebet!";
			return;
		}

		_game.StartNewRound(_lastBet);
		UpdateUI();
		SetGameActive(true);
		if (ResultLabel != null) ResultLabel.Text = "";
		if (NewRoundButton != null) NewRoundButton.Visible = false;
		if (RebetButton != null)    RebetButton.Visible = false;
	}

	private void OnExitPressed()
	{
		Visible = false;
		_game.Reset();
		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	private void EndRound()
	{
		SetGameActive(false);
		if (NewRoundButton != null) NewRoundButton.Visible = true;
		if (RebetButton != null)    RebetButton.Visible = true;

		var result = _game.DetermineRoundResult();
		int winnings = 0;

		switch (result)
		{
			case BlackjackGame.RoundResult.PlayerWin:
				winnings = _game.CurrentBet;
				if (ResultLabel != null) ResultLabel.Text = "You Win!";
				break;
			case BlackjackGame.RoundResult.DealerWin:
				winnings = -_game.CurrentBet;
				if (ResultLabel != null) ResultLabel.Text = "Dealer Wins.";
				break;
			case BlackjackGame.RoundResult.Push:
				winnings = 0;
				if (ResultLabel != null) ResultLabel.Text = "Push.";
				break;
		}

		if (winnings != 0)
			PlayerStatsManager.Instance.AddMoney(winnings);

		UpdateUI();
	}

	private void UpdateUI()
	{
		if (MoneyDisplay != null)
			MoneyDisplay.Text = $"Money: {PlayerStatsManager.Instance.CurrentMoney}";

		if (PlayerHandText != null)
			PlayerHandText.Text = $"You: {string.Join(", ", _game.PlayerHand)} ({_game.GetHandValue(_game.PlayerHand)})";

		if (DealerHandText != null)
			DealerHandText.Text = $"Dealer: {string.Join(", ", _game.DealerHand)} ({_game.GetHandValue(_game.DealerHand)})";

		if (DoubleButton != null)
			DoubleButton.Disabled = !_game.CanDoubleDown;
	}

	private void SetGameActive(bool active)
	{
		if (HitButton != null)      HitButton.Disabled = !active;
		if (StandButton != null)    StandButton.Disabled = !active;
		if (DoubleButton != null)   DoubleButton.Disabled = !active;
		if (PlaceBetButton != null) PlaceBetButton.Disabled = active;
		if (BetInput != null)       BetInput.Editable = !active;
	}
}
