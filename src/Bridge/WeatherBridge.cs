using Godot;
using Game.Core;
using Game.Core.Models;
using System.Collections.Generic;

namespace Game.Bridge;

public partial class WeatherBridge : CanvasLayer
{
	[Export] public Label TemperatureLabel = null!;
	[Export] public Label TimeLabel = null!;
	[Export] public OptionButton LocationDropdown = null!;

	public override void _Ready()
	{
		// 1. Subscribe to the signal immediately (this is safe)
		WeatherManager.Instance.WeatherUpdated += OnStateUpdated;
		
		// 2. Use CallDeferred to ensure the Singleton is fully ready before execution
		Callable.From(InitializeLocationDropdown).CallDeferred();
	}

	private void InitializeLocationDropdown()
	{
		// Safety: If the manager still isn't there, exit
		if (WeatherManager.Instance == null) return;
		if (LocationDropdown == null) return;
		
		LocationDropdown.Clear();
		
		// Populate from the Singleton
		foreach (var loc in WeatherManager.Instance.Locations)
		{
			LocationDropdown.AddItem(loc.Value["name"]);
			LocationDropdown.SetItemMetadata(LocationDropdown.ItemCount - 1, loc.Key);
		}
		
		// Ensure signal is connected only once
		if (!LocationDropdown.IsConnected(OptionButton.SignalName.ItemSelected, Callable.From<long>(OnLocationSelected)))
		{
			LocationDropdown.ItemSelected += OnLocationSelected;
		}
	}

	private void OnLocationSelected(long index)
	{
		if (WeatherManager.Instance == null) return;
		string selectedKey = (string)LocationDropdown.GetItemMetadata((int)index);
		WeatherManager.Instance.ChangeLocation(selectedKey);
	}

	private void OnStateUpdated(WeatherData weather, float bodyTemp, float wetness, float gameTime)
	{
		if (TemperatureLabel != null) 
			TemperatureLabel.Text = $"[{weather.CityName}] {weather.Temperature:F1}°F";
			
		int hours = (int)gameTime;
		int minutes = (int)((gameTime - hours) * 60);
		if (TimeLabel != null)
			TimeLabel.Text = $"{hours:00}:{minutes:00}";
	}
}
