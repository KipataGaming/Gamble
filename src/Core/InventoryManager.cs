using System.Collections.Generic;
using Godot;
using Game.Resources;

namespace Game.Core;

public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; } = null!;

    private readonly Dictionary<string, int> _inventory = new();
    private readonly Dictionary<string, ItemResource> _itemDatabase = new();

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void AddItem(ItemResource item, int amount = 1)
    {
        if (item == null || amount <= 0) return;

        if (!_itemDatabase.ContainsKey(item.Id))
        {
            _itemDatabase[item.Id] = item;
        }

        if (_inventory.ContainsKey(item.Id))
        {
            _inventory[item.Id] += amount;
        }
        else
        {
            _inventory[item.Id] = amount;
        }

        GD.Print($"[Inventory] Added {amount}x {item.Name}. Total: {_inventory[item.Id]}");
        
        // Pass the updated total inventory amount to the EventBroker
        EventBroker.TriggerItemCollected(item.Id, _inventory[item.Id]);
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

    // Allows the UI Bridge to ask the core for an item's data (like its Icon)
    public ItemResource GetItemData(string itemId)
    {
        if (_itemDatabase.TryGetValue(itemId, out var itemResource))
        {
            return itemResource;
        }
        return null;
    }
}