using Godot;
using Game.Core;

namespace Game.Bridge;

public partial class LocationSetter : Node
{
    // These enum values must match your WeatherManager.Locations keys exactly
    public enum CityLocation
    {
        GRAND_FORKS, PRIPYAT, NYC, LA, MEXICO_CITY, LONDON, 
        PARIS, BERLIN, MOSCOW, TOKYO, BEIJING, MUMBAI, 
        SYDNEY, RIO, CAIRO, CAPE_TOWN
    }

    [Export] public CityLocation SelectedCity = CityLocation.GRAND_FORKS;

    public override void _Ready()
    {
        // On startup, set the location based on the Inspector selection
        if (WeatherManager.Instance != null)
        {
            string locationKey = SelectedCity.ToString();
            WeatherManager.Instance.ChangeLocation(locationKey);
        }
    }
}