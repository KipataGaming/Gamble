// src/Bridge/MarketUI.cs
using Godot;
using Game.Core;
using Game.Resources;

namespace Game.Bridge;

public partial class MarketUI : Control
{
    [Export] public VBoxContainer SellList = null!;
    [Export] public VBoxContainer BuyList = null!;

    [Export] public Godot.Collections.Array<string> BuyableItemIds = new() { "wood", "rock", "melon" };

    public override void _Ready()
{
    Visible = false;

    // Make sure mouse is captured again if this UI is somehow closed another way
    VisibilityChanged += () =>
    {
        if (!Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    };

    RefreshLists();
}

    public override void _UnhandledInput(InputEvent @event)
{
    if (@event.IsActionPressed("ui_cancel") || 
        (@event is InputEventKey k && k.Pressed && k.Keycode == Key.B))
    {
        Visible = !Visible;

        if (Visible)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;   // Show mouse
            RefreshLists();
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;  // Hide mouse + return to game
        }
    }
}

    public void RefreshLists()
    {
        foreach (Node child in SellList.GetChildren()) child.QueueFree();
        foreach (Node child in BuyList.GetChildren()) child.QueueFree();

        // ===================== SELL LIST =====================
        var inventory = InventoryManager.Instance.GetAllItems();

        foreach (var kvp in inventory)
        {
            string itemId = kvp.Key;
            int quantity = kvp.Value;
            if (quantity <= 0) continue;

            var item = InventoryManager.Instance.GetItemData(itemId);
            if (item == null) continue;

            int sellPrice = MarketManager.Instance.GetSellPrice(item);

            var row = CreateRow(
                $"{item.Name} x{quantity}",
                $"Sell for {sellPrice} money",
                () =>
                {
                    if (MarketManager.Instance.SellItem(itemId, 1))
                        RefreshLists();
                }
            );

            SellList.AddChild(row);
        }

        // ===================== BUY LIST =====================
        foreach (string itemId in BuyableItemIds)
        {
            var item = InventoryManager.Instance.GetItemData(itemId);
            if (item == null) continue;

            int buyPrice = MarketManager.Instance.GetBuyPrice(item);

            var row = CreateRow(
                item.Name,
                $"Buy for {buyPrice} money",
                () =>
                {
                    if (MarketManager.Instance.BuyItem(itemId, 1))
                        RefreshLists();
                }
            );

            BuyList.AddChild(row);
        }
    }

    private HBoxContainer CreateRow(string itemName, string priceText, System.Action onPressed)
    {
        var row = new HBoxContainer();

        var nameLabel = new Label();
        nameLabel.Text = itemName;
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var priceLabel = new Label();
        priceLabel.Text = priceText;
        priceLabel.HorizontalAlignment = HorizontalAlignment.Right;
        priceLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;

        var button = new Button();
        button.Text = "Sell";
        button.Pressed += onPressed;

        row.AddChild(nameLabel);
        row.AddChild(priceLabel);
        row.AddChild(button);

        return row;
    }
}