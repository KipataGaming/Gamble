using Godot;
using System; // Added for Actions/Events
using System.Collections.Generic;
using Game.Core; // Added so it can find FarmingCore!
namespace Game.Bridge
{

public partial class FarmGridBridge : Node3D
{
    [Export] public int GridWidth { get; set; } = 5;
    [Export] public int GridHeight { get; set; } = 5;
    [Export] public float TileSpacing { get; set; } = 2.0f;

    // The Logic Core (No Godot Nodes inside this!)
    private FarmingCore _farmingCore;

    // Visual tracking: Maps a coordinate to its visual representation
    private Dictionary<Vector2I, MeshInstance3D> _visualTiles;
    // Visual tracking: Maps a coordinate to the crop mesh growing there
private Dictionary<Vector2I, MeshInstance3D> _visualCrops = new Dictionary<Vector2I, MeshInstance3D>();

    public override void _Ready()
    {
        _visualTiles = new Dictionary<Vector2I, MeshInstance3D>();
        _farmingCore = new FarmingCore();

        // 1. Subscribe to Core Events
        _farmingCore.OnSoilStateChanged += HandleSoilStateChanged;
        _farmingCore.OnCropStateChanged += HandleCropStateChanged;

        // 2. Initialize Logic and Visuals
        _farmingCore.InitializeGrid(GridWidth, GridHeight);
        GenerateGreyBoxVisuals();
    }

    private void GenerateGreyBoxVisuals()
{
    for (int x = 0; x < GridWidth; x++)
    {
        for (int y = 0; y < GridHeight; y++)
        {
            var coord = new Vector2I(x, y);
            
            // 1. Create the Visual Mesh
            MeshInstance3D tileMesh = new MeshInstance3D();
            BoxMesh boxMesh = new BoxMesh();
            // Flatten the box slightly so it looks like a farm plot
            boxMesh.Size = new Vector3(TileSpacing * 0.9f, 0.2f, TileSpacing * 0.9f); 
            tileMesh.Mesh = boxMesh;
            tileMesh.Position = new Vector3(x * TileSpacing, 0, y * TileSpacing);
            
            StandardMaterial3D mat = new StandardMaterial3D();
            mat.AlbedoColor = Colors.SaddleBrown; 
            tileMesh.MaterialOverride = mat;

            // 2. Create the Physics Collider for your Raycast
            StaticBody3D staticBody = new StaticBody3D();
            CollisionShape3D collisionShape = new CollisionShape3D();
            BoxShape3D physicsBox = new BoxShape3D();
            physicsBox.Size = boxMesh.Size; // Match collider to mesh size
            
            collisionShape.Shape = physicsBox;
            
            // Build the hierarchy: Mesh -> StaticBody -> CollisionShape
            staticBody.AddChild(collisionShape);
            tileMesh.AddChild(staticBody);

            AddChild(tileMesh);
            _visualTiles[coord] = tileMesh;
        }
    }
}

    // --- EVENT HANDLERS ---
    private void HandleSoilStateChanged(Vector2I coordinate, SoilState newState)
{
    if (_visualTiles.TryGetValue(coordinate, out MeshInstance3D tile))
    {
        StandardMaterial3D mat = new StandardMaterial3D();
        
        // Update the color based purely on the logic core's state
        switch (newState)
        {
            case SoilState.Untilled:
                mat.AlbedoColor = Colors.SaddleBrown;
                break;
            case SoilState.Tilled:
                mat.AlbedoColor = Colors.DarkKhaki; // Lighter brown for fresh dirt
                break;
            case SoilState.Watered:
                mat.AlbedoColor = Colors.SaddleBrown.Darkened(0.5f); // Dark, wet dirt
                break;
        }
        
        tile.MaterialOverride = mat;
    }
}

    private void HandleCropStateChanged(Vector2I coordinate, GrowthStage newStage, string cropId)
{
    if (!_visualTiles.TryGetValue(coordinate, out MeshInstance3D tile)) return;

    // 1. If the stage is Empty, remove/hide the visual crop
    if (newStage == GrowthStage.Empty)
    {
        if (_visualCrops.TryGetValue(coordinate, out MeshInstance3D existingCropMesh))
        {
            existingCropMesh.QueueFree(); // Removes the node from the scene
            _visualCrops.Remove(coordinate); // Remove from our tracking dictionary
        }
        return; // Exit early since we just cleaned up
    }

    // 2. Otherwise, create or update the existing crop mesh
    if (!_visualCrops.TryGetValue(coordinate, out MeshInstance3D cropMesh))
    {
        cropMesh = new MeshInstance3D();
        cropMesh.Mesh = new BoxMesh();
        tile.AddChild(cropMesh);
        _visualCrops[coordinate] = cropMesh;
    }

    StandardMaterial3D mat = new StandardMaterial3D();
    BoxMesh box = cropMesh.Mesh as BoxMesh;

    switch (newStage)
    {
        case GrowthStage.Seed:
            box.Size = new Vector3(0.3f, 0.3f, 0.3f);
            cropMesh.Position = new Vector3(0, 0.25f, 0);
            mat.AlbedoColor = Colors.Orange;
            break;
        case GrowthStage.Growing:
            box.Size = new Vector3(0.6f, 0.8f, 0.6f);
            cropMesh.Position = new Vector3(0, 0.5f, 0);
            mat.AlbedoColor = Colors.LimeGreen;
            break;
        case GrowthStage.Mature:
            box.Size = new Vector3(0.8f, 1.2f, 0.8f);
            cropMesh.Position = new Vector3(0, 0.7f, 0);
            mat.AlbedoColor = Colors.DarkGreen;
            break;
    }
    cropMesh.MaterialOverride = mat;
}
    // --- PUBLIC API FOR PLAYER CONTROLLER ---
    // The player calls these, and the Bridge passes them down to the purely logical Core
    public void Interact(Vector2I coordinate, string actionType, string cropId = "")
    {
        _farmingCore.InteractWithPlot(coordinate, actionType, cropId);
    }
    public Vector2I WorldToGridCoordinates(Vector3 worldPosition)
{
    // We add half the spacing to ensure we get the center of the tile, 
    // then divide by spacing to get the index.
    int x = Mathf.RoundToInt(worldPosition.X / TileSpacing);
    int y = Mathf.RoundToInt(worldPosition.Z / TileSpacing); 
    
    return new Vector2I(x, y);
}
public override void _Input(InputEvent @event)
{
    // Developer Shortcut: Press 'T' to simulate a day passing
    if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
    {
        if (keyEvent.Keycode == Key.T)
        {
            GD.Print("[FarmGridBridge] Developer Shortcut: Simulating next day...");
            
            // Call the method on your logic core to process growth and drying
            if (_farmingCore != null)
            {
                _farmingCore.SimulateDayPassing(); 
            }
            else
            {
                GD.PrintErr("[FarmGridBridge] Error: FarmingCore is null. Make sure it is initialized in _Ready().");
            }
        }
    }
}
}
}