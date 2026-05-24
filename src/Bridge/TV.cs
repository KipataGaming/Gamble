using Godot;
using System.Collections.Generic;

public partial class TV : Node3D
{
    [Export] public VideoStreamPlayer VideoPlayer;

    [Export] public string VideoFolderPath = "res://src/Resources/Videos/";

    private List<string> _videoPaths = new List<string>();
    private int _currentChannel = 0;

    public override void _Ready()
    {
        if (VideoPlayer == null)
            VideoPlayer = GetNodeOrNull<VideoStreamPlayer>("SubViewport/VideoStreamPlayer");

        LoadVideosFromFolder(VideoFolderPath);
    }

    private void LoadVideosFromFolder(string folderPath)
    {
        _videoPaths.Clear();

        var dir = DirAccess.Open(folderPath);
        if (dir == null)
        {
            GD.PrintErr($"[TV] Could not open video folder: {folderPath}");
            return;
        }

        dir.ListDirBegin();
        string fileName = dir.GetNext();

        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".ogv"))
            {
                _videoPaths.Add(folderPath + fileName);
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();

        GD.Print($"[TV] Loaded {_videoPaths.Count} video(s) from {folderPath}");
    }

    public void OnPlayerInteract()
    {
        if (_videoPaths.Count == 0) return;

        // Cycle to next channel
        _currentChannel = (_currentChannel + 1) % _videoPaths.Count;
        PlayCurrentChannel();
    }

    private void PlayCurrentChannel()
    {
        if (_videoPaths.Count == 0 || VideoPlayer == null) return;

        string path = _videoPaths[_currentChannel];
        var stream = GD.Load<VideoStream>(path);

        if (stream != null)
        {
            VideoPlayer.Stream = stream;
            VideoPlayer.Play();
            GD.Print($"[TV] Channel {_currentChannel + 1}: {path}");
        }
    }
}