using Godot;
using Game.Core;
using Game.Resources;

namespace Game.Bridge
{
    // 1. ADDED the IDamageable contract here
    public partial class PlayerController : CharacterBody3D, IDamageable
    {
        [Export] public Camera3D PlayerCamera = null!;
        [Export] public RayCast3D InteractionRay = null!;
        
        [Export] public WeaponBridge CurrentWeapon = null!; 
        // Add this new array to hold your hotbar items
        [Export] public Godot.Collections.Array<WeaponBridge> Equipment = new Godot.Collections.Array<WeaponBridge>();

        [Export] public float Speed = 5.0f;
        [Export] public float JumpVelocity = 4.5f;
        [Export] public float MouseSensitivity = 0.005f;
        [Export] public float Gravity = 9.8f;

        public override void _Ready()
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        public override void _Process(double delta)
        {
            if (GetTree().Paused) return;

            // Interaction (F Key) - ONLY for picking things up
            if (Input.IsActionJustPressed("interact"))
            {
                HandlePickup();
            }

            // Use Tool / Attack (Left Click)
            if (Input.IsActionJustPressed("attack"))
            {
                HandleToolUse();
            }
        }

        private void EquipTool(int slotIndex)
        {
            // Make sure the slot actually exists in our array
            if (slotIndex < 0 || slotIndex >= Equipment.Count) return;
            
            WeaponBridge selectedTool = Equipment[slotIndex];
            if (selectedTool == null) return;

            // Hide all tools in the array
            foreach (WeaponBridge tool in Equipment)
            {
                if (tool != null)
                {
                    tool.Visible = false;
                }
            }

            // Equip and show the selected tool
            CurrentWeapon = selectedTool;
            CurrentWeapon.Visible = true;
            
            GD.Print($"[Player] Equipped Slot {slotIndex + 1}: {CurrentWeapon.ToolCategory}");
        }

        public override void _Input(InputEvent @event)
        {
            if (GetTree().Paused) return;

            // --- Camera Rotation Logic (Existing) ---
            if (@event is InputEventMouseMotion mouseMotion && PlayerCamera != null)
            {
                RotateY(-mouseMotion.Relative.X * MouseSensitivity);
                PlayerCamera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
                Vector3 rot = PlayerCamera.Rotation;
                rot.X = Mathf.Clamp(rot.X, -Mathf.Pi / 2, Mathf.Pi / 2);
                PlayerCamera.Rotation = rot;
            }

            // --- NEW: Weapon Swapping Logic ---
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.Key1) EquipTool(0); // Slot 1 (Weapon)
                if (keyEvent.Keycode == Key.Key2) EquipTool(1); // Slot 2 (Shovel)
                if (keyEvent.Keycode == Key.Key3) EquipTool(2); // Slot 3 (Seed)
                if (keyEvent.Keycode == Key.Key4) EquipTool(3); // Slot 4 (Watering Can)
                if (keyEvent.Keycode == Key.Key5) EquipTool(4); // Slot 5 (Scythe)
                if (keyEvent.Keycode == Key.Key6) EquipTool(5); // Slot 6 (Axe)
                if (keyEvent.Keycode == Key.Key7) EquipTool(6); // Slot 7 (Pickaxe)
            }
        }

        private void HandlePickup()
        {
            if (InteractionRay == null) return;

            InteractionRay.ForceRaycastUpdate();
            if (InteractionRay.IsColliding())
            {
                Node collider = InteractionRay.GetCollider() as Node;
                
                // Strictly for picking up items on the ground
                if (collider != null && collider.HasMethod("InteractAndPickup"))
                {
                    Variant result = collider.Call("InteractAndPickup");
                    if (result.VariantType != Variant.Type.Nil)
                    {
                        ItemResource item = (ItemResource)result.AsGodotObject();
                        InventoryManager.Instance.AddItem(item, 1);
                        GD.Print($"[Player] Picked up: {item.Name}");
                    }
                }
            }
        }

        private void HandleToolUse()
        {
            if (CurrentWeapon == null) return;

            string toolCategory = CurrentWeapon.ToolCategory.Trim().ToLower();
            
            // --- STAMINA COSTS ---
            float staminaCost = 0f;
            if (toolCategory == "axe" || toolCategory == "pickaxe") staminaCost = 5f;
            else if (toolCategory == "shovel" || toolCategory == "wateringcan") staminaCost = 3f;
            else if (toolCategory == "scythe" || toolCategory == "weapon") staminaCost = 2f;
            else if (toolCategory == "seed") staminaCost = 1f;

            // 1. Combat Logic
            if (toolCategory == "weapon")
            {
                if (PlayerStatsManager.Instance.TryConsumeStamina(staminaCost))
                {
                    CurrentWeapon.PerformAttack();
                }
                return;
            }

            // 2. Interaction Raycast Update
            if (InteractionRay == null) return;
            InteractionRay.ForceRaycastUpdate();
            
            if (InteractionRay.IsColliding())
            {
                Node collider = InteractionRay.GetCollider() as Node;

                // Resource Gathering Logic (Trees / Rocks)
                ResourceNodeBridge hitResource = FindResourceBridgeParent(collider);
                if (hitResource != null)
                {
                    if (toolCategory == "axe")
                    {
                        if (PlayerStatsManager.Instance.TryConsumeStamina(staminaCost)) hitResource.Interact("Chop");
                        return; 
                    }
                    else if (toolCategory == "pickaxe")
                    {
                        if (PlayerStatsManager.Instance.TryConsumeStamina(staminaCost)) hitResource.Interact("Mine");
                        return; 
                    }
                }

                // Farming Logic
                FarmGridBridge hitGarden = FindFarmBridgeParent(collider);
                if (hitGarden != null)
                {
                    Vector3 hitPoint = InteractionRay.GetCollisionPoint();
                    Vector2I gridCoord = hitGarden.WorldToGridCoordinates(hitPoint);
                    
                    string actionToSend = "";
                    string cropIdToPlant = "";

                    if (toolCategory == "shovel") actionToSend = "Till";
                    else if (toolCategory == "wateringcan") actionToSend = "Water";
                    else if (toolCategory == "seed") { actionToSend = "Plant"; cropIdToPlant = "Carrot"; }
                    else if (toolCategory == "scythe") actionToSend = "Harvest";

                    if (!string.IsNullOrEmpty(actionToSend))
                    {
                        // Only interact with the farm if we have the stamina to do it
                        if (PlayerStatsManager.Instance.TryConsumeStamina(staminaCost))
                        {
                            hitGarden.Interact(gridCoord, actionToSend, cropIdToPlant);
                        }
                    }
                }
            }
        }
        
        public override void _PhysicsProcess(double delta)
        {
            if (GetTree().Paused) return;
            Vector3 velocity = Velocity;
            if (!IsOnFloor()) velocity.Y -= Gravity * (float)delta;
            if (Input.IsActionJustPressed("jump") && IsOnFloor()) velocity.Y = JumpVelocity;

            Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
            Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
            
            if (direction != Vector3.Zero)
            {
                velocity.X = direction.X * Speed;
                velocity.Z = direction.Z * Speed;
            }
            else
            {
                velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
                velocity.Z = Mathf.MoveToward(Velocity.Z, 0, Speed);
            }
            Velocity = velocity;
            MoveAndSlide();
        }

        // --- HELPER METHODS ---

        private FarmGridBridge FindFarmBridgeParent(Node node)
        {
            // Climb up the scene tree to find the FarmGridBridge
            Node current = node;
            while (current != null)
            {
                if (current is FarmGridBridge bridge)
                {
                    return bridge;
                }
                current = current.GetParent();
            }
            return null;
        }

        private ResourceNodeBridge FindResourceBridgeParent(Node node)
        {
            // Climb up the scene tree to find the ResourceNodeBridge (Trees/Rocks)
            Node current = node;
            while (current != null)
            {
                if (current is ResourceNodeBridge bridge)
                {
                    return bridge;
                }
                current = current.GetParent();
            }
            return null;
        }

        // 2. NEW: THE DAMAGE PIPELINE (IDamageable Contract)
        public void TakeDamage(int amount)
        {
            GD.Print($"[Player] -> HIT! Took {amount} damage.");
            
            
             PlayerStatsManager.Instance.DecreaseHealth(amount);
        }
    }
}