using Godot;

public partial class TV : Node3D
{
    [Export] public VideoStreamPlayer VideoPlayer;

    public override void _Ready()
    {
        if (VideoPlayer == null)
        {
            // Try to find it automatically if not assigned
            VideoPlayer = GetNodeOrNull<VideoStreamPlayer>("SubViewport/VideoStreamPlayer");
        }
    }

    /// <summary>
    /// Play a video from a given path (must be .ogv file)
    /// </summary>
    public void PlayVideo(string videoPath)
    {
        if (VideoPlayer == null) return;

        var stream = GD.Load<VideoStream>(videoPath);
        if (stream != null)
        {
            VideoPlayer.Stream = stream;
            VideoPlayer.Play();
            GD.Print($"[TV] Playing: {videoPath}");
        }
        else
        {
            GD.PrintErr($"[TV] Could not load video: {videoPath}");
        }
    }

    public void Play()
    {
        if (VideoPlayer != null)
            VideoPlayer.Play();
    }

    public void Pause()
    {
        if (VideoPlayer != null)
            VideoPlayer.Paused = !VideoPlayer.Paused;
    }

    public void Stop()
    {
        if (VideoPlayer != null)
            VideoPlayer.Stop();
    }

    /// <summary>
    /// Call this when the player interacts with the TV
    /// </summary>
    public void Interact()
    {
        // For now just toggle pause/play
        if (VideoPlayer != null && VideoPlayer.IsPlaying())
        {
            Pause();
        }
        else
        {
            Play();
        }
    }
    /// <summary>
/// Called when the player interacts with this TV (press E)
/// </summary>
public void OnPlayerInteract()
{
    if (VideoPlayer == null) return;

    if (VideoPlayer.IsPlaying())
    {
        VideoPlayer.Paused = !VideoPlayer.Paused; // Toggle pause
    }
    else
    {
        VideoPlayer.Play();
    }

    GD.Print("[TV] Player interacted with TV");
}

}