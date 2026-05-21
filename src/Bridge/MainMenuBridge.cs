using Godot;

namespace Game.Bridge;

public partial class MainMenuBridge : Control
{
    [Export] public Button PlayButton = null!;
    [Export] public Button SettingsButton = null!;
    [Export] public Button ExitButton = null!;
    
    // Reference to your Settings UI node
    [Export] public Control SettingsMenu = null!;
    
    // Path to your main game scene
    [Export(PropertyHint.File, "*.tscn")] public string GameScenePath = "res://src/Main.tscn";

    public override void _Ready()
    {
        // Wire buttons safely
        if (PlayButton != null) PlayButton.Pressed += OnPlayPressed;
        if (SettingsButton != null) SettingsButton.Pressed += OnSettingsPressed;
        if (ExitButton != null) ExitButton.Pressed += OnExitPressed;

        // Ensure settings are hidden on load
        if (SettingsMenu != null) SettingsMenu.Visible = false;
    }

    private void OnPlayPressed()
    {
        // 1. Grab the tree reference immediately while the node is definitively active
        SceneTree tree = GetTree();
        
        if (tree == null)
        {
            GD.PrintErr("[MainMenu] Cannot change scene: SceneTree is null.");
            return;
        }

        if (string.IsNullOrEmpty(GameScenePath))
        {
            GD.PrintErr("[MainMenu] GameScenePath is not set in the Inspector!");
            return;
        }

        // 2. Instruct the Godot engine to handle the scene change safely at the end of the frame.
        // This is much safer than wrapping GetTree() inside a C# Callable lambda.
        tree.CallDeferred(SceneTree.MethodName.ChangeSceneToFile, GameScenePath);
    }

    private void OnSettingsPressed()
    {
        if (SettingsMenu != null)
        {
            SettingsMenu.Visible = !SettingsMenu.Visible;
        }
    }

    private void OnExitPressed()
    {
        GetTree()?.Quit();
    }
}