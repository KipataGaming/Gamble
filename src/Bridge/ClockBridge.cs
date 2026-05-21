using Godot;
using Game.Core;

namespace Game.Bridge;

public partial class ClockBridge : Label
{
	public override void _Process(double delta)
	{
		// 1. Get active location from WeatherManager
		string locKey = WeatherManager.Instance.CurrentLocationKey;
		
		// 2. Get the calculated local time
		float localTime = TimeManager.Instance.GetLocalTime(locKey);
		
		// 3. Convert to HH:MM string
		int hours = (int)(localTime * 24);
		int minutes = (int)((localTime * 24 - hours) * 60);
		
		this.Text = $"{hours:00}:{minutes:00}";
	}
}
