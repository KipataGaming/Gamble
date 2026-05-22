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

        // Peak day = 1.0, Peak night = -1.0
        float dayCycle = Mathf.Cos((timeNormalized - 0.5f) * Mathf.Pi * 2.0f);
        float energy = Mathf.Clamp(dayCycle, 0.02f, 1.0f);

        // 1. Fade the ambient light and skybox glow
        Environment.BackgroundEnergyMultiplier = energy;
        Environment.AmbientLightEnergy = energy;

        // 2. NEW: Fade the stars in and out
        // We grab the procedural sky material to access the Sky Cover
        if (Environment.Sky?.SkyMaterial is ProceduralSkyMaterial skyMat)
        {
            // When energy is low (Night), starAlpha approaches 1.0
            // When energy is high (Day), starAlpha hits 0.0
            float starAlpha = Mathf.Clamp(1.0f - energy, 0.0f, 1.0f);
            
            // SkyCoverModulate tints the star texture and controls its transparency
            skyMat.SkyCoverModulate = new Color(1, 1, 1, starAlpha);
        }
    }
}