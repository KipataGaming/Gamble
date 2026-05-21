using System.Collections.Generic;
using Godot;
using Game.Resources;

namespace Game.Core;

public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; } = null!;

    // The mathematical state of the inventory: Item ID -> Quantity
    private readonly Dictionary<string, int> _inventory = new();
    
    // A cache of the actual item data so we can look up weights/names
    private readonly Dictionary<string, ItemResource> _itemDatabase = new();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void AddItem(ItemResource item, int amount = 1)
    {
        if (item == null || amount <= 0) return;

        // Ensure the item is tracked in the database
        if (!_itemDatabase.ContainsKey(item.Id))
        {
            _itemDatabase[item.Id] = item;
        }

        // Add to the internal math
        if (_inventory.ContainsKey(item.Id))
        {
            _inventory[item.Id] += amount;
        }
        else
        {
            _inventory[item.Id] = amount;
        }

        GD.Print($"[Inventory] Added {amount}x {item.Name}. Total: {_inventory[item.Id]}");
        
        // Notify the rest of the game (e.g., the UI Bridge, the Quest System)
        EventBroker.TriggerItemCollected(item.Id, amount);
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (!_inventory.ContainsKey(itemId) || _inventory[itemId] < amount)
        {
            GD.PrintErr($"[Inventory] Failed to remove {amount}x {itemId}. Not enough in stock.");
            return false; 
        }

        _inventory[itemId] -= amount;
        
        if (_inventory[itemId] <= 0)
        {
            _inventory.Remove(itemId);
        }

        GD.Print($"[Inventory] Removed {amount}x {itemId}.");
        return true;
    }
    
    public int GetItemCount(string itemId)
    {
        return _inventory.ContainsKey(itemId) ? _inventory[itemId] : 0;
    }

    public float GetTotalWeight()
    {
        float totalWeight = 0;
        foreach (var kvp in _inventory)
        {
            if (_itemDatabase.TryGetValue(kvp.Key, out var itemResource))
            {
                totalWeight += itemResource.Weight * kvp.Value;
            }
        }
        return totalWeight;
    }
}