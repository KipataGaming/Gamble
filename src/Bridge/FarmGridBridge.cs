// src/Bridge/FarmGridBridge.cs
using Godot;
using System;
using System.Collections.Generic;
using Game.Core;
using Game.Resources;

namespace Game.Bridge
{
    public partial class FarmGridBridge : Node3D
    {
        [Export] public int GridWidth { get; set; } = 5;
        [Export] public int GridHeight { get; set; } = 5;
        [Export] public float TileSpacing { get; set; } = 2.0f;

        private FarmingCore _farmingCore;

        private Dictionary<Vector2I, MeshInstance3D> _visualTiles = new();
        private Dictionary<Vector2I, MeshInstance3D> _visualCrops = new();

        public override void _Ready()
        {
            _farmingCore = new FarmingCore();

            _farmingCore.OnSoilStateChanged += HandleSoilStateChanged;
            _farmingCore.OnCropStateChanged += HandleCropStateChanged;

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
                    
                    MeshInstance3D tileMesh = new MeshInstance3D();
                    BoxMesh boxMesh = new BoxMesh();
                    boxMesh.Size = new Vector3(TileSpacing * 0.9f, 0.2f, TileSpacing * 0.9f); 
                    tileMesh.Mesh = boxMesh;
                    tileMesh.Position = new Vector3(x * TileSpacing, 0, y * TileSpacing);
                    
                    StandardMaterial3D mat = new StandardMaterial3D { AlbedoColor = Colors.SaddleBrown };
                    tileMesh.MaterialOverride = mat;

                    StaticBody3D staticBody = new StaticBody3D();
                    CollisionShape3D collisionShape = new CollisionShape3D();
                    BoxShape3D physicsBox = new BoxShape3D { Size = boxMesh.Size };
                    collisionShape.Shape = physicsBox;
                    
                    staticBody.AddChild(collisionShape);
                    tileMesh.AddChild(staticBody);

                    AddChild(tileMesh);
                    _visualTiles[coord] = tileMesh;
                }
            }
        }

        private void HandleSoilStateChanged(Vector2I coordinate, SoilState newState)
        {
            if (_visualTiles.TryGetValue(coordinate, out MeshInstance3D tile))
            {
                StandardMaterial3D mat = new StandardMaterial3D();
                switch (newState)
                {
                    case SoilState.Untilled: mat.AlbedoColor = Colors.SaddleBrown; break;
                    case SoilState.Tilled:   mat.AlbedoColor = Colors.DarkKhaki; break;
                    case SoilState.Watered:  mat.AlbedoColor = Colors.SaddleBrown.Darkened(0.5f); break;
                }
                tile.MaterialOverride = mat;
            }
        }

        private void HandleCropStateChanged(Vector2I coordinate, GrowthStage newStage, string cropId)
        {
            if (!_visualTiles.TryGetValue(coordinate, out MeshInstance3D tile)) return;

            if (newStage == GrowthStage.Empty)
            {
                if (_visualCrops.TryGetValue(coordinate, out MeshInstance3D existing))
                {
                    existing.QueueFree();
                    _visualCrops.Remove(coordinate);
                }
                return;
            }

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

        // ==================== PUBLIC API ====================
        public void Interact(Vector2I coordinate, string actionType, string cropId = "")
        {
            if (actionType == "Harvest")
{
    if (_farmingCore.TryHarvest(coordinate, out string harvestedCropId))
    {
        if (string.IsNullOrEmpty(harvestedCropId))
            harvestedCropId = "melon";   // ← fallback so it works even if CurrentCropId is missing

        InventoryManager.Instance.AddItemById(harvestedCropId, 1);
        GD.Print($"[FarmGrid] Harvested {harvestedCropId}");
    }
}
            else
            {
                _farmingCore.InteractWithPlot(coordinate, actionType, cropId);
            }
        }

        public Vector2I WorldToGridCoordinates(Vector3 worldPosition)
        {
            int x = Mathf.RoundToInt(worldPosition.X / TileSpacing);
            int y = Mathf.RoundToInt(worldPosition.Z / TileSpacing);
            return new Vector2I(x, y);
        }

        // Random quality helper (same as trees/rocks)
        private ItemRarity GetRandomRarity()
        {
            int roll = GD.RandRange(1, 100);
            return roll switch
            {
                < 60  => ItemRarity.RoyalBlue,
                < 85  => ItemRarity.Silver,
                < 95  => ItemRarity.Gold,
                < 98  => ItemRarity.RoyalPurple,
                < 99  => ItemRarity.Emerald,
                _     => ItemRarity.Crimson
            };
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.T)
            {
                GD.Print("[FarmGridBridge] Simulating next day...");
                _farmingCore?.SimulateDayPassing();
            }
        }
    }
}