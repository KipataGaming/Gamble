using Godot;

public partial class SecurityCamera : Node3D
{
	[Export] public SubViewport Viewport;
	[Export] public Camera3D Camera;

	public override void _Ready()
	{
		if (Viewport == null)
			Viewport = GetNodeOrNull<SubViewport>("Viewport");

		if (Camera == null)
			Camera = GetNodeOrNull<Camera3D>("Viewport/Camera");

		AddToGroup("security_camera");
	}

	public override void _Process(double delta)
	{
		// Force the inner camera to follow the root node's position & rotation
		if (Camera != null)
		{
			Camera.GlobalTransform = GlobalTransform;
		}
	}

	public Texture2D GetCameraTexture()
	{
		return Viewport?.GetTexture();
	}
}
