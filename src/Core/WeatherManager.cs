using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Game.Core.Models;

namespace Game.Core
{
    public partial class WeatherManager : Node
    {
        public static WeatherManager Instance { get; private set; } = null!;
        public string CurrentLocationKey { get; set; } = "GRAND_FORKS";
        
        private const string API_KEY = "zpka_7b1cc380725f403e9dfa6d86b93cfabb_bbe19fb2";
        private readonly System.Net.Http.HttpClient _client = new();

        // Updated to use standard IANA Timezone IDs for cross-platform reliability
        public readonly Dictionary<string, Dictionary<string, string>> Locations = new() {
            { "GRAND_FORKS", new() {{"key", "334978"}, {"name", "Grand Forks, ND"}, {"tz", "America/Chicago"}} },
            { "PRIPYAT", new() {{"key", "324505"}, {"name", "Chernobyl Zone"}, {"tz", "Europe/Kiev"}} },
            { "NYC", new() {{"key", "349727"}, {"name", "New York, NY"}, {"tz", "America/New_York"}} },
            { "LA", new() {{"key", "347625"}, {"name", "Los Angeles, CA"}, {"tz", "America/Los_Angeles"}} },
            { "MEXICO_CITY", new() {{"key", "242560"}, {"name", "Mexico City, MX"}, {"tz", "America/Mexico_City"}} },
            { "LONDON", new() {{"key", "328328"}, {"name", "London, UK"}, {"tz", "Europe/London"}} },
            { "PARIS", new() {{"key", "623"}, {"name", "Paris, FR"}, {"tz", "Europe/Paris"}} },
            { "BERLIN", new() {{"key", "178087"}, {"name", "Berlin, DE"}, {"tz", "Europe/Berlin"}} },
            { "MOSCOW", new() {{"key", "294021"}, {"name", "Moscow, RU"}, {"tz", "Europe/Moscow"}} },
            { "TOKYO", new() {{"key", "226396"}, {"name", "Tokyo, JP"}, {"tz", "Asia/Tokyo"}} },
            { "BEIJING", new() {{"key", "101924"}, {"name", "Beijing, CN"}, {"tz", "Asia/Shanghai"}} },
            { "MUMBAI", new() {{"key", "204842"}, {"name", "Mumbai, IN"}, {"tz", "Asia/Kolkata"}} },
            { "SYDNEY", new() {{"key", "22889"}, {"name", "Sydney, AU"}, {"tz", "Australia/Sydney"}} },
            { "RIO", new() {{"key", "45449"}, {"name", "Rio de Janeiro, BR"}, {"tz", "America/Sao_Paulo"}} },
            { "CAIRO", new() {{"key", "127164"}, {"name", "Cairo, EG"}, {"tz", "Africa/Cairo"}} },
            { "CAPE_TOWN", new() {{"key", "306633"}, {"name", "Cape Town, ZA"}, {"tz", "Africa/Johannesburg"}} }
        };

        public WeatherData CurrentWeather { get; private set; } = new();
        public float BodyTemp { get; private set; } = 98.6f;
        public float Wetness { get; private set; } = 0.0f;
        public float GameTime { get; private set; } = 12.0f; 
        public bool DebugForceRain { get; set; } = false;

        [Signal] public delegate void WeatherUpdatedEventHandler(WeatherData weather, float bodyTemp, float wetness, float gameTime);

        public override void _Ready()
        {
            Instance = this;
            _ = FetchWeather("GRAND_FORKS");
        }

        public void Tick(float delta)
        {
            try {
                var tzId = Locations[CurrentLocationKey]["tz"];
                // TimeZoneInfo.FindSystemTimeZoneById works differently on Windows vs Linux/macOS
                // Using a try-catch pattern ensures we don't crash and fallback gracefully
                TimeZoneInfo timeZone;
                try {
                    timeZone = TimeZoneInfo.FindSystemTimeZoneById(tzId);
                } catch {
                    // Fallback for systems where IANA names aren't mapped
                    timeZone = TimeZoneInfo.Local;
                }
                
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
                GameTime = localTime.Hour + (localTime.Minute / 60.0f) + (localTime.Second / 3600.0f);
            } catch {
                GameTime = DateTime.Now.Hour + (DateTime.Now.Minute / 60.0f);
            }

            bool isRaining = CurrentWeather.IsRaining || DebugForceRain;
            Wetness = isRaining ? Mathf.MoveToward(Wetness, 100.0f, delta * 2.0f) : Mathf.MoveToward(Wetness, 0.0f, delta * 0.5f);   

            float environmentTargetTemp = CurrentWeather.Temperature;
            if (Wetness > 0) environmentTargetTemp -= (Wetness / 10.0f) * 2.0f; 
            BodyTemp = (environmentTargetTemp < 60.0f) ? BodyTemp - (delta * (60.0f - environmentTargetTemp) * 0.01f) : Mathf.MoveToward(BodyTemp, 98.6f, delta * 0.05f);

            EmitSignal(SignalName.WeatherUpdated, Variant.From(CurrentWeather), BodyTemp, Wetness, GameTime);
        }

        public void ChangeLocation(string locId)
        {
            if (!Locations.ContainsKey(locId)) return;
            CurrentLocationKey = locId;
            _ = FetchWeather(locId);
        }

        public async Task FetchWeather(string locId)
        {
            if (!Locations.ContainsKey(locId)) return;
            var key = Locations[locId]["key"];
            var url = $"https://dataservice.accuweather.com/currentconditions/v1/{key}?apikey={API_KEY}&details=true";

            try {
                var response = await _client.GetStringAsync(url);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<AccuWeatherResponse[]>(response, options);

                if (results != null && results.Length > 0)
                {
                    var data = results[0];
                    CurrentWeather = new WeatherData {
                        CityName = Locations[locId]["name"],
                        Temperature = data.Temperature.Imperial.Value,
                        IsRaining = data.HasPrecipitation,
                        IsDaytime = data.IsDayTime
                    };
                    EmitSignal(SignalName.WeatherUpdated, Variant.From(CurrentWeather), BodyTemp, Wetness, GameTime);
                }
            } catch (System.Exception e) {
                GD.PrintErr($"[Core] Network error: {e.Message}");
            }
        }
    }

    public class AccuWeatherResponse { public TemperatureContainer Temperature { get; set; } = new(); public bool HasPrecipitation { get; set; } public bool IsDayTime { get; set; } }
    public class TemperatureContainer { public ImperialData Imperial { get; set; } = new(); }
    public class ImperialData { public float Value { get; set; } }
}