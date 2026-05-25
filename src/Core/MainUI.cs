using Godot;

namespace Game.Core;

public partial class MainUI : CanvasLayer
{
    [Export] public Control EscapeMenu;

    private bool _isEscapeMenuOpen = false;

    public override void _Ready()
    {
        if (EscapeMenu != null)
            EscapeMenu.Visible = false;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            ToggleEscapeMenu();
            GetViewport().SetInputAsHandled();
        }
    }

    public void ToggleEscapeMenu()
    {
        if (EscapeMenu == null) return;

        _isEscapeMenuOpen = !_isEscapeMenuOpen;
        EscapeMenu.Visible = _isEscapeMenuOpen;
        GetTree().Paused = _isEscapeMenuOpen;

        if (_isEscapeMenuOpen)
            Input.MouseMode = Input.MouseModeEnum.Visible;
        else
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public void ResumeGame()
    {
        if (EscapeMenu != null)
            EscapeMenu.Visible = false;

        _isEscapeMenuOpen = false;
        GetTree().Paused = false;
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public void QuitToMainMenu()
    {
        GetTree().Paused = false;
        GetTree().ChangeSceneToFile("res://MainMenu.tscn");
    }
}