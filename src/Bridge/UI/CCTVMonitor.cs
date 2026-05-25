using Godot;
using System.Collections.Generic;

public partial class CCTVMonitor : Node3D
{
    [Export] public MeshInstance3D Screen;

    private List<SecurityCamera> _cameras = new();
    private int _currentIndex = 0;

    public override void _Ready()
    {
        FindAllCameras();
        UpdateDisplay();
    }

    private void FindAllCameras()
    {
        _cameras.Clear();
        var nodes = GetTree().GetNodesInGroup("security_camera");

        foreach (Node node in nodes)
        {
            if (node is SecurityCamera cam)
                _cameras.Add(cam);
        }

        GD.Print($"[CCTVMonitor] Found {_cameras.Count} security cameras");
    }

    public void OnPlayerInteract()
    {
        CycleCamera();
    }

    public void CycleCamera()
    {
        if (_cameras.Count == 0) return;

        _currentIndex = (_currentIndex + 1) % _cameras.Count;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (Screen == null)
        {
            GD.PrintErr("[CCTV] Screen is null!");
            return;
        }

        if (_cameras.Count == 0)
        {
            GD.PrintErr("[CCTV] No cameras found!");
            return;
        }

        var currentCam = _cameras[_currentIndex];
        if (currentCam == null)
        {
            GD.PrintErr("[CCTV] Current camera is null!");
            return;
        }

        var texture = currentCam.GetCameraTexture();
        if (texture == null)
        {
            GD.PrintErr("[CCTV] Texture from camera is null!");
            return;
        }

        var material = Screen.GetActiveMaterial(0) as StandardMaterial3D;
        if (material == null)
        {
            GD.PrintErr("[CCTV] Material on screen is null!");
            return;
        }

        material.AlbedoTexture = texture;
        GD.Print("[CCTV] Successfully updated monitor display");
    }
}