using System.Collections.Generic;
using Godot;
using Game.Resources;

namespace Game.Core;

public partial class InventoryManager : Node
{
    public static InventoryManager Instance { get; private set; } = null!;

    private readonly Dictionary<string, int> _inventory = new();
    private readonly Dictionary<string, ItemResource> _itemDatabase = new();
public float MaxStamina { get; private set; } = 100f;
public float CurrentStamina { get; private set; }

public int MaxHealth { get; private set; } = 100;
public int CurrentHealth { get; private set; }     // ← Add this if missing

public int CurrentMoney { get; private set; } = 0; // ← You already added this
    public override void _EnterTree()
{
    Instance = this;

    CurrentStamina = MaxStamina;
    CurrentHealth = MaxHealth;
    CurrentMoney = 0;

    // Load all ItemResource .tres files from src/Resources/
    var dir = DirAccess.Open("res://src/Resources");
    if (dir != null)
    {
        dir.ListDirBegin();
        string fileName = dir.GetNext();

        while (fileName != "")
        {
            if (!dir.CurrentIsDir() && fileName.EndsWith(".tres"))
            {
                string path = "res://src/Resources/" + fileName;
                var resource = GD.Load<ItemResource>(path);

                if (resource != null && !string.IsNullOrEmpty(resource.Id))
                {
                    _itemDatabase[resource.Id] = resource;
                    GD.Print($"[InventoryManager] Loaded item: {resource.Id}");
                }
            }
            fileName = dir.GetNext();
        }
        dir.ListDirEnd();
    }
    else
    {
        GD.PrintErr("[InventoryManager] Could not open res://src/Resources folder!");
    }
}
public Dictionary<string, int> GetAllItems()
{
    return new Dictionary<string, int>(_inventory);
}
    public void AddItem(ItemResource item, int amount = 1)
{
    if (item == null || amount <= 0) return;

    if (!_inventory.ContainsKey(item.Id))
        _inventory[item.Id] = 0;

    _inventory[item.Id] += amount;

    GD.Print($"[Inventory] Added {amount}x {item.Name}. Total: {_inventory[item.Id]}");

    EventBroker.TriggerItemCollected(item.Id, _inventory[item.Id]);
}
     public void AddItemById(string itemId, int amount = 1)
{
    ItemResource item = GetItemData(itemId);

    if (item == null)
    {
        // Create fallback AND add it to the database so GetItemData can find it later
        item = new ItemResource
        {
            Id = itemId,
            Name = itemId.Capitalize()
        };

        _itemDatabase[itemId] = item;   // ← This line was missing

        GD.PrintErr($"[InventoryManager] WARNING: No .tres found for '{itemId}'. Using fallback.");
    }

    
    AddItem(item, amount);
}
        public bool RemoveItem(string itemId, int amount = 1)
{
    if (!_inventory.ContainsKey(itemId)) return false;
    if (_inventory[itemId] < amount) return false;

    _inventory[itemId] -= amount;

    if (_inventory[itemId] <= 0)
        _inventory.Remove(itemId);

    GD.Print($"[Inventory] Removed {amount}x {itemId}. Remaining: {_inventory.GetValueOrDefault(itemId, 0)}");
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
    // First try the loaded dictionary
    if (_itemDatabase.TryGetValue(itemId, out var itemResource))
    {
        return itemResource;
    }

    // Fallback: try to load it directly from disk
    string path = $"res://src/Resources/{itemId}.tres";
    var loaded = GD.Load<ItemResource>(path);
    if (loaded != null)
    {
        _itemDatabase[itemId] = loaded;
        return loaded;
    }

    return null;
}
}