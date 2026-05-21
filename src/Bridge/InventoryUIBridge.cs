using Godot;
using Game.Core;

namespace Game.Bridge;

public partial class InventoryUIBridge : Control
{
    // Changed from VBoxContainer to GridContainer
    [Export] public GridContainer InventoryGrid = null!;

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

        // 1. Create the Slot Background (A dark grey square)
        var slot = new Panel();
        slot.CustomMinimumSize = new Vector2(80, 80); 

        // 2. Create the Text inside the slot
        var label = new Label();
        label.Text = $"{quantity}x\n{itemId}";
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        
        // Make the label stretch to fill the 80x80 slot
        label.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        // 3. Assemble and push to the screen
        slot.AddChild(label);
        InventoryGrid.AddChild(slot);
        
        GD.Print($"[UI Bridge] Rendered grid slot for: {itemId}");
    }
}