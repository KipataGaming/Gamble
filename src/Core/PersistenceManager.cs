using System;
using System.IO;
using System.Text.Json;
using Godot;

namespace Game.Core;

public static class PersistenceManager
{
    private static readonly string SavePath = "user://savegame.json";

    public static void SaveGame()
    {
        // Extract ONLY the raw data, not the WeatherManager node
        var saveData = new
        {
            CityKey = WeatherManager.Instance.CurrentLocationKey,
            Temp = WeatherManager.Instance.CurrentWeather.Temperature,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            string json = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ProjectSettings.GlobalizePath(SavePath), json);
            GD.Print("[Persistence] Game Saved!");
        }
        catch (Exception e)
        {
            GD.PrintErr($"[Persistence] Save failed: {e.Message}");
        }
    }
    public static void LoadGame()
    {
        try
        {
            if (File.Exists(ProjectSettings.GlobalizePath(SavePath)))
            {
                string json = File.ReadAllText(ProjectSettings.GlobalizePath(SavePath));
                var saveData = JsonSerializer.Deserialize<dynamic>(json);

                WeatherManager.Instance.CurrentLocationKey = saveData.CityKey;
                WeatherManager.Instance.CurrentWeather.Temperature = saveData.Temp;
                GD.Print("[Persistence] Game Loaded!");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"[Persistence] Load failed: {e.Message}");
        }
    }
}