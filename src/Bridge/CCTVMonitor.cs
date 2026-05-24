using Godot;
using System.Collections.Generic;

public partial class CCTVMonitor : Node3D
{
    [Export] public MeshInstance3D Screen;
    [Export] public Godot.Collections.Array<SecurityCamera> Cameras = new();

    private int _currentIndex = 0;

    public override void _Ready()
    {
        UpdateDisplay();
    }

    public void OnPlayerInteract()
    {
        CycleCamera();
    }

    public void CycleCamera()
    {
        if (Cameras.Count == 0) return;

        _currentIndex = (_currentIndex + 1) % Cameras.Count;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (Screen == null || Cameras.Count == 0) return;

        var currentCam = Cameras[_currentIndex];
        if (currentCam == null) return;

        var texture = currentCam.GetCameraTexture();
        if (texture == null) return;

        // Apply the live feed to the screen
        var material = Screen.GetActiveMaterial(0) as StandardMaterial3D;
        if (material != null)
        {
            material.AlbedoTexture = texture;
        }
    }
}