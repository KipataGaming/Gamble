using Godot;
using Game.Core;

namespace Game.Bridge;

public partial class SunBridge : Node3D 
{
    [Export] public DirectionalLight3D SunLight = null!;

    public override void _Process(double delta)
    {
        if (WeatherManager.Instance == null) return;
        WeatherManager.Instance.Tick((float)delta);

        if (SunLight != null)
{
    float gameTime = WeatherManager.Instance.GameTime;
    float timeNormalized = gameTime / 24.0f;
    float rotationAngle = timeNormalized * 360.0f;

    SunLight.RotationDegrees = new Vector3(rotationAngle - 90, 0, 0);
    
    // Use Cosine so the sun peaks at exactly High Noon (0.5) and drops to 0 at night
    float energy = Mathf.Cos((timeNormalized - 0.5f) * Mathf.Pi * 2.0f);
    
    // Clamp to 0.0f so the light turns completely OFF below the horizon
    SunLight.LightEnergy = Mathf.Clamp(energy, 0.0f, 1.0f);
}
    }
    
}