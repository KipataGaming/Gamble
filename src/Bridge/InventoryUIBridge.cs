using Godot;
using System.Collections.Generic;
using Game.Core;
using Game.Resources; // Required so the bridge can understand ItemResource

namespace Game.Bridge;

public partial class InventoryUIBridge : Control
{
    [Export] public GridContainer InventoryGrid = null!;

    // Tracker to remember the exact Label node for every item on screen
    private Dictionary<string, Label> _inventorySlots = new Dictionary<string, Label>();

    public override void _Ready()
    {
        EventBroker.OnItemCollected += HandleItemCollected;
    }

    public override void _ExitTree()
    {
        EventBroker.OnItemCollected -= HandleItemCollected;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("inventory"))
        {
            Visible = !Visible; 
            Input.MouseMode = Visible ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
        }
    }

    private void HandleItemCollected(string itemId, int quantity)
{
    if (InventoryGrid == null) return;

    // Ask the Core for the item's data to grab the Icon
    ItemResource itemData = InventoryManager.Instance.GetItemData(itemId);
    Texture2D itemIcon = itemData?.Icon;
    string displayName = itemData != null ? itemData.Name : itemId;

    // 1. If slot exists, just update the number
    if (_inventorySlots.TryGetValue(itemId, out Label existingLabel))
    {
        existingLabel.Text = $"{quantity}";
        GD.Print($"[UI Bridge] Updated existing slot for: {displayName} (New Total: {quantity})");
        return; 
    }

    // 2. If it DOES NOT exist, build a brand new slot
    var slot = new Panel();
    slot.CustomMinimumSize = new Vector2(80, 80); 

    // 3. Render the Icon Image
    if (itemIcon != null)
    {
        var iconRect = new TextureRect();
        iconRect.Texture = itemIcon;
        iconRect.ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize; 
        iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered; 
        iconRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        
        // Use Offsets instead of SetMargin (Top, Left, Bottom, Right)
        iconRect.OffsetLeft = 10;
        iconRect.OffsetTop = 10;
        iconRect.OffsetRight = -10;
        iconRect.OffsetBottom = -10;
        
        slot.AddChild(iconRect);
    }

    // 4. Render the Quantity Text (Bottom Right Corner)
    var label = new Label();
    label.Text = $"{quantity}";
    label.HorizontalAlignment = HorizontalAlignment.Right;
    label.VerticalAlignment = VerticalAlignment.Bottom;
    label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
    
    // Use Offsets for the label
    label.OffsetRight = -5;
    label.OffsetBottom = -5;

    slot.AddChild(label);
    InventoryGrid.AddChild(slot);
    
    // 5. Save this label into our tracker
    _inventorySlots[itemId] = label;
    
    GD.Print($"[UI Bridge] Rendered NEW grid slot with Icon for: {displayName}");
}
}