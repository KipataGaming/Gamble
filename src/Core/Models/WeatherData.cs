using Godot;

namespace Game.Core.Models;

public partial class WeatherData : RefCounted
{
    public string CityName { get; set; } = string.Empty;
    public float Temperature { get; set; }
    public bool IsRaining { get; set; }
    public bool IsDaytime { get; set; }
    public int CloudCover { get; set; }
}