using Godot;
using System.Collections.Generic;

namespace Game.Core;

public class MarketManager
{
    public static MarketManager Instance { get; private set; } = new MarketManager();

    // Base sell prices (we can move this to a config/resource later)
    private readonly Dictionary<string, int> _baseSellPrices = new()
    {
        ["wood"]  = 5,
        ["rock"]  = 8,
        ["melon"] = 12
    };

    public int GetSellPrice(ItemResource item)
    {
        if (item == null) return 0;

        int basePrice = _baseSellPrices.GetValueOrDefault(item.Id, 5);
        float multiplier = item.GetValueMultiplier(); // uses your existing rarity system

        return Mathf.RoundToInt(basePrice * multiplier);
    }

    public bool SellItem(string itemId, int quantity = 1)
    {
        var item = InventoryManager.Instance.GetItemData(itemId);
        if (item == null) return false;

        if (!InventoryManager.Instance.RemoveItem(itemId, quantity))
            return false;

        int totalPrice = GetSellPrice(item) * quantity;
        PlayerStatsManager.Instance.AddMoney(totalPrice);

        GD.Print($"[Market] Sold {quantity}x {item.Name} for {totalPrice} money");
        return true;
    }
}