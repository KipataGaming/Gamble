using Godot;
using Game.Core;
using Game.Core.Models;

namespace Game.Bridge;

public partial class HudBridge : CanvasLayer
{
	[Export] public Label TemperatureLabel = null!;
	[Export] public Label TimeLabel = null!;
	[Export] public Label BodyTempLabel = null!;

	public override void _Ready()
	{
		if (WeatherManager.Instance != null)
		{
			WeatherManager.Instance.WeatherUpdated += OnWeatherUpdated;
		}
	}

	private void OnWeatherUpdated(WeatherData weather, float bodyTemp, float wetness, float gameTime)
	{
		if (TemperatureLabel != null) 
			TemperatureLabel.Text = $"[{weather.CityName}] Temp: {weather.Temperature:F1}°F";
			
		if (BodyTempLabel != null)
			BodyTempLabel.Text = $"Body Temp: {bodyTemp:F1}°F";
			
		if (TimeLabel != null)
		{
			int hours = (int)gameTime;
			int minutes = (int)((gameTime - hours) * 60);
			TimeLabel.Text = $"{hours:00}:{minutes:00}";
		}
	}

	public override void _ExitTree()
	{
		if (WeatherManager.Instance != null)
			WeatherManager.Instance.WeatherUpdated -= OnWeatherUpdated;
	}
}
