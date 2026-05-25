using Godot;
using System;
using System.Collections.Generic;

namespace Game.Bridge;

public partial class WeatherBridge : CanvasLayer
{
	[Export] public Label TemperatureLabel;
	[Export] public Label TimeLabel;
	[Export] public OptionButton LocationDropdown;
	[Export] public NodePath Sky3DPath;

	private Node _sky;

	private readonly Dictionary<string, (string Name, double Lat, double Lon)> _locations = new()
	{
		{ "congo",      ("Congo Jungle",     0.0,   23.0) },
		{ "newyork",    ("New York",        40.7,  -74.0) },
		{ "tokyo",      ("Tokyo",           35.7,  139.7) },
		{ "grandforks", ("Grand Forks",     47.9,  -97.0) },
		{ "sydney",     ("Sydney",         -33.9, 151.2) }
	};

	public override void _Ready()
	{
		if (Sky3DPath != null)
			_sky = GetNode(Sky3DPath);

		InitializeLocationDropdown();
		UpdateTimeDisplay();
	}

	private void InitializeLocationDropdown()
	{
		if (LocationDropdown == null) return;

		LocationDropdown.Clear();

		foreach (var kvp in _locations)
		{
			LocationDropdown.AddItem(kvp.Value.Name);
			LocationDropdown.SetItemMetadata(LocationDropdown.ItemCount - 1, kvp.Key);
		}

		LocationDropdown.ItemSelected += OnLocationSelected;

		if (LocationDropdown.ItemCount > 0)
		{
			LocationDropdown.Selected = 0;
			SetSkyLocation("congo");
		}
	}

	private void OnLocationSelected(long index)
	{
		string key = (string)LocationDropdown.GetItemMetadata((int)index);
		SetSkyLocation(key);
	}

	private void SetSkyLocation(string key)
	{
		if (_sky == null || !_locations.ContainsKey(key)) return;

		var loc = _locations[key];

		_sky.Set("latitude", loc.Lat);
		_sky.Set("longitude", loc.Lon);
		_sky.Set("datetime", Time.GetDatetimeDictFromSystem());

		if (TemperatureLabel != null)
			TemperatureLabel.Text = $"[{loc.Name}] Real-time Sky";
	}

	private void UpdateTimeDisplay()
	{
		if (TimeLabel == null) return;

		var now = DateTime.Now;
		TimeLabel.Text = $"{now.Hour:00}:{now.Minute:00}";
	}

	public override void _Process(double delta)
	{
		UpdateTimeDisplay();
	}
}
