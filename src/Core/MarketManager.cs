using Godot;
using Game.Resources;

namespace Game.Core;

public class MarketManager
{
    public static MarketManager Instance { get; private set; } = new MarketManager();

    /// <summary>
    /// Price the player receives when selling this item to the shop.
    /// </summary>
    public int GetSellPrice(ItemResource item)
{
    if (item == null) return 0;

    return item.Id switch
    {
        "wood"  => 8,
        "rock"  => 12,
        "melon" => 20,
        _       => 5
    };
}

public int GetBuyPrice(ItemResource item)
{
    if (item == null) return 0;

    return item.Id switch
    {
        "wood"  => 15,
        "rock"  => 25,
        "melon" => 40,
        _       => 10
    };
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

    public bool BuyItem(string itemId, int quantity = 1)
    {
        var item = InventoryManager.Instance.GetItemData(itemId);
        if (item == null) return false;

        int totalPrice = GetBuyPrice(item) * quantity;

        if (!PlayerStatsManager.Instance.TrySpend(totalPrice))
            return false;

        InventoryManager.Instance.AddItem(item, quantity);
        GD.Print($"[Market] Bought {quantity}x {item.Name} for {totalPrice} money");
        return true;
    }
}