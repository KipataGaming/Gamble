using Godot;
using System;
using System.Collections.Generic;
using Game.Bridge;
using Game.Core;
public partial class BlackjackTable : Node3D
{
    [Export] public PackedScene BlackjackUI;

    public void OnPlayerInteract()
    {
        if (BlackjackUI == null)
        {
            GD.PrintErr("[BlackjackTable] BlackjackUI scene is not assigned in the Inspector!");
            return;
        }

        // Try to find existing UI in the scene
        var ui = GetTree().Root.GetNodeOrNull<BlackjackUI>("/root/BlackjackUI");

        if (ui == null)
        {
            // Create it if it doesn't exist
            ui = BlackjackUI.Instantiate<BlackjackUI>();
            ui.Name = "BlackjackUI";
            GetTree().Root.AddChild(ui);
        }

        // Open the UI
        ui.Visible = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
}