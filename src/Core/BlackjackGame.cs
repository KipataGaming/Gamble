using Godot;
using System.Collections.Generic;

namespace Game.Core
{
	public partial class BlackjackGame
	{
		public List<int> PlayerHand { get; private set; } = new();
		public List<int> DealerHand { get; private set; } = new();

		private List<int> _deck = new();
		private RandomNumberGenerator _rng = new();

		public int CurrentBet { get; private set; }
		public bool HasDoubled { get; private set; }

		public void StartNewRound(int betAmount)
		{
			CurrentBet = betAmount;
			HasDoubled = false;

			PlayerHand.Clear();
			DealerHand.Clear();
			_deck.Clear();

			for (int suit = 0; suit < 4; suit++)
			{
				for (int value = 1; value <= 13; value++)
					_deck.Add(value);
			}

			FisherYatesShuffle(_deck, _rng);

			PlayerHand.Add(DrawCard());
			DealerHand.Add(DrawCard());
			PlayerHand.Add(DrawCard());
			DealerHand.Add(DrawCard());
		}

		public bool CanDoubleDown => PlayerHand.Count == 2 && !HasDoubled;

		public void DoubleDown()
		{
			if (!CanDoubleDown) return;

			HasDoubled = true;
			CurrentBet *= 2;
			PlayerHand.Add(DrawCard());
		}

		public int DrawCard()
		{
			if (_deck.Count == 0) return 0;
			int card = _deck[0];
			_deck.RemoveAt(0);
			return card;
		}

		public int GetHandValue(List<int> hand)
		{
			int value = 0;
			int aces = 0;

			foreach (int card in hand)
			{
				if (card == 1) { aces++; value += 11; }
				else if (card >= 11) { value += 10; }
				else { value += card; }
			}

			while (value > 21 && aces > 0)
			{
				value -= 10;
				aces--;
			}
			return value;
		}

		public bool IsBust(List<int> hand) => GetHandValue(hand) > 21;
		public bool IsBlackjack(List<int> hand) => GetHandValue(hand) == 21 && hand.Count == 2;

		public void DealerPlay()
		{
			while (GetHandValue(DealerHand) < 17)
			{
				DealerHand.Add(DrawCard());
			}
		}

		public enum RoundResult { PlayerWin, DealerWin, Push }

		public RoundResult DetermineRoundResult()
		{
			int playerValue = GetHandValue(PlayerHand);
			int dealerValue = GetHandValue(DealerHand);

			if (IsBust(PlayerHand)) return RoundResult.DealerWin;
			if (IsBust(DealerHand)) return RoundResult.PlayerWin;
			if (playerValue > dealerValue) return RoundResult.PlayerWin;
			if (dealerValue > playerValue) return RoundResult.DealerWin;
			return RoundResult.Push;
		}

		private void FisherYatesShuffle<T>(IList<T> list, RandomNumberGenerator rng)
		{
			for (int i = list.Count - 1; i > 0; i--)
			{
				int j = rng.RandiRange(0, i);
				(list[i], list[j]) = (list[j], list[i]);
			}
		}

		public void Reset()
		{
			PlayerHand.Clear();
			DealerHand.Clear();
			_deck.Clear();
			CurrentBet = 0;
			HasDoubled = false;
		}
	}
}
