// src/Bridge/InventoryUIBridge.cs
using Godot;
using System.Collections.Generic;
using Game.Core;
using Game.Resources;

namespace Game.Bridge;

public partial class InventoryUIBridge : Control
{
    [Export] public GridContainer InventoryGrid = null!;

    private Dictionary<string, Control> _slots = new Dictionary<string, Control>();

    public override void _Ready()
    {
        GD.Print("[InventoryUI] Ready - Grid assigned? " + (InventoryGrid != null));
        if (InventoryGrid != null)
            InventoryGrid.Columns = 5;

        EventBroker.OnItemCollected += HandleItemCollected;
    }

    public override void _ExitTree()
    {
        EventBroker.OnItemCollected -= HandleItemCollected;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("inventory"))
            Visible = !Visible;
    }

    private void HandleItemCollected(string itemId, int quantity)
    {
        GD.Print($"[InventoryUI] Received item: {itemId} x{quantity}");

        if (InventoryGrid == null)
        {
            GD.PrintErr("[InventoryUI] InventoryGrid is null!");
            return;
        }

        ItemResource item = InventoryManager.Instance.GetItemData(itemId);
        string displayName = item?.Name ?? itemId.Capitalize();

        var slot = new PanelContainer();
        slot.CustomMinimumSize = new Vector2(100, 100);

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
        style.BorderWidthBottom = 8;

        if (item != null)
            style.BorderColor = GetRarityColor(item.Rarity);
        else
            style.BorderColor = Colors.Gray;

        slot.AddThemeStyleboxOverride("panel", style);

        var vbox = new VBoxContainer();
        vbox.Alignment = BoxContainer.AlignmentMode.Center;

        if (item?.Icon != null)
        {
            var icon = new TextureRect();
            icon.Texture = item.Icon;
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            icon.CustomMinimumSize = new Vector2(64, 64);
            vbox.AddChild(icon);
        }

        var label = new Label();
        label.Text = $"{displayName}\nx{quantity}";
        label.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(label);

        slot.AddChild(vbox);
        InventoryGrid.AddChild(slot);
        _slots[itemId] = slot;

        GD.Print($"[InventoryUI] SUCCESS: Created slot for {displayName}");
    }

    private Color GetRarityColor(ItemRarity rarity)
    {
        return rarity switch
        {
            ItemRarity.RoyalBlue   => new Color(0.2f, 0.4f, 1.0f),
            ItemRarity.Silver      => new Color(0.75f, 0.75f, 0.75f),
            ItemRarity.Gold        => new Color(1.0f, 0.84f, 0.0f),
            ItemRarity.RoyalPurple => new Color(0.7f, 0.2f, 1.0f),
            ItemRarity.Emerald     => new Color(0.0f, 0.8f, 0.4f),
            ItemRarity.Crimson     => new Color(1.0f, 0.1f, 0.2f),
            _ => Colors.White
        };
    }
}