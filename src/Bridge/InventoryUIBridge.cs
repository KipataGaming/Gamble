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

        EventBroker.OnItemCollected += HandleItemCollected;   // ← This line was missing
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
        GD.Print($"[InventoryUI] Received: {itemId} x{quantity}");

        if (InventoryGrid == null)
        {
            GD.PrintErr("[InventoryUI] InventoryGrid is NOT assigned in Inspector!");
            return;
        }

        // Big, bright green slot so we can see it clearly
        var slot = new PanelContainer();
        slot.CustomMinimumSize = new Vector2(110, 110);

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0, 1, 0, 0.7f);
        style.BorderWidthBottom = 8;
        style.BorderColor = Colors.White;
        slot.AddThemeStyleboxOverride("panel", style);

        var label = new Label();
        label.Text = $"{itemId}\nx{quantity}";
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.AddThemeFontSizeOverride("font_size", 18);
        slot.AddChild(label);

        InventoryGrid.AddChild(slot);
        _slots[itemId] = slot;

        GD.Print($"[InventoryUI] Created visible slot for {itemId}");
    }
}