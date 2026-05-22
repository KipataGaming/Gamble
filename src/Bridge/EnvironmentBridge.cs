using Godot;
using Game.Core;

namespace Game.Bridge;

public partial class EnvironmentBridge : WorldEnvironment
{
    public override void _Process(double delta)
    {
        if (WeatherManager.Instance == null || Environment == null) return;

        float gameTime = WeatherManager.Instance.GameTime;
        float timeNormalized = gameTime / 24.0f;

        // Same math as the SunBridge: Peaks at Noon (1.0), drops to -1.0 at Midnight
        float dayCycle = Mathf.Cos((timeNormalized - 0.5f) * Mathf.Pi * 2.0f);

        // Map the day cycle to an energy multiplier.
        // We clamp the bottom at 0.02f so it is pitch black, but leaves just enough 
        // ambient "moonlight" so the player can still barely see their character.
        float energy = Mathf.Clamp(dayCycle, 0.02f, 1.0f);

        // Fade the skybox glow and the ambient light cast on 3D models
        Environment.BackgroundEnergyMultiplier = energy;
        Environment.AmbientLightEnergy = energy;
    }
}