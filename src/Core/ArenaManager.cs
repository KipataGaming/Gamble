// src/Bridge/ArenaManager.cs
using Godot;
using Game.Core; // For accessing PlayerStatsManager

namespace Game.Bridge;

public partial class ArenaManager : Node3D
{
    // The scene file to load (your Player)
    [Export] public PackedScene PlayerScene = null!;
    
    // An optional marker for where to drop the player
    [Export] public Node3D SpawnPoint = null!; 

    private Node3D _currentPlayer;

    public override void _Input(InputEvent @event)
    {
        // Check if the 'R' key was pressed
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Keycode == Key.R)
            {
                RespawnPlayer();
            }
        }
    }

    private void RespawnPlayer()
    {
        // 1. Safety check: Don't spawn a new player if the current one is still alive
        if (GodotObject.IsInstanceValid(_currentPlayer) && !_currentPlayer.IsQueuedForDeletion())
        {
            GD.Print("[System] Cannot respawn: Player is still alive.");
            return;
        }

        if (PlayerScene == null)
        {
            GD.PrintErr("[System] PlayerScene is missing! Check the Inspector.");
            return;
        }

        GD.Print("-----------------------------------");
        GD.Print("[System] Initiating Respawn Sequence...");

        // 2. Reset the global health back to 100
        PlayerStatsManager.Instance.ResetStats();

        // 3. Create a brand new Player body from the saved scene file
        _currentPlayer = PlayerScene.Instantiate<Node3D>();
        
        // 4. Add the player to the arena
        AddChild(_currentPlayer);

        // 5. Teleport them to the spawn point (or just 0,0,0 if no spawn point exists)
        if (SpawnPoint != null)
        {
            _currentPlayer.GlobalPosition = SpawnPoint.GlobalPosition;
        }
        else
        {
            _currentPlayer.GlobalPosition = new Vector3(0, 2, 0); // Drop them slightly above the floor
        }
        
        GD.Print("[System] Player Respawned successfully.");
    }
}