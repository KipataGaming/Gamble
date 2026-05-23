// src/Bridge/ResourceNodeBridge.cs
using Godot;
using Game.Core;
using Game.Resources;

namespace Game.Bridge
{
    public partial class ResourceNodeBridge : Node3D
    {
        [Export] public int MaxHealth { get; set; } = 3;
        [Export] public string ResourceType { get; set; } = "Tree";

        private ResourceData _logicCore;

        public override void _Ready()
        {
            _logicCore = new ResourceData(MaxHealth);
            CreateVisibleResource();
        }

        private void CreateVisibleResource()
        {
            string type = ResourceType.Trim().ToLower();

            if (type == "tree")
            {
                var trunk = new MeshInstance3D();
                trunk.Mesh = new CylinderMesh { TopRadius = 0.25f, BottomRadius = 0.35f, Height = 2.0f };
                trunk.Position = new Vector3(0, 1.0f, 0);
                trunk.MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.4f, 0.25f, 0.1f) };
                AddChild(trunk);

                var leaves = new MeshInstance3D();
                leaves.Mesh = new SphereMesh { Radius = 1.2f, Height = 1.8f };
                leaves.Position = new Vector3(0, 2.2f, 0);
                leaves.MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.1f, 0.6f, 0.1f) };
                AddChild(leaves);
            }
            else if (type == "rock")
            {
                var rock = new MeshInstance3D();
                rock.Mesh = new SphereMesh { Radius = 0.8f, Height = 1.0f };
                rock.Position = new Vector3(0, 0.5f, 0);
                rock.MaterialOverride = new StandardMaterial3D { AlbedoColor = new Color(0.5f, 0.5f, 0.5f) };
                AddChild(rock);
            }

            var staticBody = new StaticBody3D();
            var collision = new CollisionShape3D();
            collision.Shape = new BoxShape3D { Size = new Vector3(1.5f, 2.5f, 1.5f) };
            staticBody.AddChild(collision);
            AddChild(staticBody);
        }

        public void Interact(string actionType)
        {
            string type = ResourceType.Trim().ToLower();

            if ((actionType == "Chop" && type == "tree") || (actionType == "Mine" && type == "rock"))
            {
                bool hitSuccessful = _logicCore.TryHit(1);

                if (hitSuccessful && _logicCore.State == ResourceState.Destroyed)
                {
                    string itemId = type == "tree" ? "wood" : "rock";
                    int amount = type == "tree" ? 3 : 2;

                    // Random quality (your 6-tier system)
                    ItemRarity rarity = GetRandomRarity();

                    InventoryManager.Instance.AddItemById(itemId, amount);
                    GD.Print($"[ResourceNode] Gave {amount}x {itemId} → {rarity}");

                    QueueFree();
                }
            }
        }

        // Random quality helper (Stardew Valley style)
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
    }
}