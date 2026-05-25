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