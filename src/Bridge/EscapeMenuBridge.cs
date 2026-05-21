using Godot;

namespace Game.Bridge;

public partial class EscapeMenuBridge : CanvasLayer
{
    [Export] public Control MenuContainer = null!;
    [Export] public Button ResumeButton = null!;
    [Export] public Button ExitButton = null!;

    public override void _Ready()
    {
        // FORCE this script to keep running even when the game is paused
        ProcessMode = ProcessModeEnum.Always;
        
        MenuContainer.Visible = false;
        
        ResumeButton.Pressed += ToggleMenu;
        ExitButton.Pressed += () => GetTree().Quit();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        bool isPaused = !GetTree().Paused;
        GetTree().Paused = isPaused;
        
        MenuContainer.Visible = isPaused;
        
        // Ensure mouse visibility toggles correctly
        Input.MouseMode = isPaused ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }
}