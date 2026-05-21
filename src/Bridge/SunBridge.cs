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
            SunLight.LightEnergy = Mathf.Clamp(Mathf.Sin(timeNormalized * Mathf.Pi * 2), 0.1f, 1.0f);
        }
    }
}