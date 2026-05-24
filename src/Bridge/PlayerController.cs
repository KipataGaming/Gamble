// src/Bridge/PlayerController.cs
using Godot;
using Game.Core;
using Game.Resources;
using Game.Bridge;

namespace Game.Bridge
{
    public partial class PlayerController : CharacterBody3D, IDamageable
    {
        [Export] public Camera3D PlayerCamera = null!;
        [Export] public RayCast3D InteractionRay = null!;
        
        [Export] public WeaponBridge CurrentWeapon = null!; 
        [Export] public Godot.Collections.Array<WeaponBridge> Equipment = new Godot.Collections.Array<WeaponBridge>();

        [Export] public float Speed = 5.0f;
        [Export] public float JumpVelocity = 4.5f;
        [Export] public float MouseSensitivity = 0.005f;
        [Export] public float Gravity = 9.8f;

        [Export] public Label InteractionPrompt;

        private TV _currentTV;
        private Node _currentInteractable;

        private bool _isDead = false;
        private Vector3 _spawnPosition;

        public override void _Ready()
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _spawnPosition = GlobalPosition;
        }

        public override void _Process(double delta)
        {
            if (GetTree().Paused || _isDead) return;

            InteractionRay.ForceRaycastUpdate();

            string promptText = "";
            _currentInteractable = null;

            if (InteractionRay.IsColliding())
            {
                Node collider = InteractionRay.GetCollider() as Node;

                Node current = collider;
                while (current != null)
                {
                    if (current is TV tv)
                    {
                        _currentInteractable = tv;
                        promptText = "Press F to use TV";
                        break;
                    }

                    if (current.IsInGroup("blackjack_table") || 
                        current.Name.ToString().ToLower().Contains("blackjack"))
                    {
                        _currentInteractable = current;
                        promptText = "Press F to play Blackjack";
                        break;
                    }

                    current = current.GetParent();
                }
            }

            if (InteractionPrompt != null)
            {
                if (!string.IsNullOrEmpty(promptText))
                {
                    InteractionPrompt.Text = promptText;
                    InteractionPrompt.Visible = true;
                }
                else
                {
                    InteractionPrompt.Visible = false;
                }
            }

            if (Input.IsActionJustPressed("interact"))
            {
                if (_currentInteractable != null && _currentInteractable.HasMethod("OnPlayerInteract"))
                {
                    _currentInteractable.Call("OnPlayerInteract");
                }
                else
                {
                    HandlePickup();
                }
            }

            if (Input.IsActionJustPressed("attack"))
                HandleToolUse();
        }

        public override void _Input(InputEvent @event)
        {
            if (GetTree().Paused) return;

            if (_isDead && @event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo && keyEvent.Keycode == Key.R)
            {
                Respawn();
                return;
            }

            if (_isDead) return;

            // === CAMERA LOCK WHEN BLACKJACK UI IS OPEN ===
            if (@event is InputEventMouseMotion mouseMotion && PlayerCamera != null)
            {
                var blackjackUI = GetTree().Root.GetNodeOrNull<BlackjackUI>("/root/BlackjackUI");
                if (blackjackUI != null && blackjackUI.Visible)
                    return; // Lock camera

                RotateY(-mouseMotion.Relative.X * MouseSensitivity);
                PlayerCamera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);

                Vector3 rot = PlayerCamera.Rotation;
                rot.X = Mathf.Clamp(rot.X, -Mathf.Pi / 2, Mathf.Pi / 2);
                PlayerCamera.Rotation = rot;
            }

            if (@event is InputEventKey keyEvent2 && keyEvent2.Pressed && !keyEvent2.Echo)
            {
                if (keyEvent2.Keycode == Key.Key1) EquipTool(0);
                if (keyEvent2.Keycode == Key.Key2) EquipTool(1);
                if (keyEvent2.Keycode == Key.Key3) EquipTool(2);
                if (keyEvent2.Keycode == Key.Key4) EquipTool(3);
                if (keyEvent2.Keycode == Key.Key5) EquipTool(4);
                if (keyEvent2.Keycode == Key.Key6) EquipTool(5);
                if (keyEvent2.Keycode == Key.Key7) EquipTool(6);

                if (keyEvent2.Keycode == Key.M)
                {
                    bool sold = MarketManager.Instance.SellItem("wood", 1);
                    GD.Print(sold ? "[TEST] Sold 1 wood!" : "[TEST] Could not sell wood");
                }

                if (keyEvent2.Keycode == Key.B)
                {
                    var market = GetTree().Root.GetNodeOrNull<MarketUI>("/root/MarketUI");
                    if (market != null)
                        market.Visible = !market.Visible;
                }
            }
        }

        public override void _PhysicsProcess(double delta)
        {
            if (GetTree().Paused || _isDead) return;

            Vector3 velocity = Velocity;

            if (!IsOnFloor())
                velocity.Y -= Gravity * (float)delta;

            if (Input.IsActionJustPressed("jump") && IsOnFloor())
                velocity.Y = JumpVelocity;

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

        public void TakeDamage(int amount)
        {
            if (_isDead || PlayerStatsManager.Instance.CurrentHealth <= 0) return;
            GD.Print($"[Player] -> HIT! Took {amount} damage.");
            PlayerStatsManager.Instance.DecreaseHealth(amount);
            if (PlayerStatsManager.Instance.CurrentHealth <= 0)
                Die();
        }

        private void Die()
        {
            _isDead = true;
            GD.Print("[Player] → DEAD. Press R to respawn.");
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        private void Respawn()
        {
            _isDead = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
            GlobalPosition = _spawnPosition;
            Velocity = Vector3.Zero;
            PlayerStatsManager.Instance.ResetStats();
            GD.Print("[Player] Respawned!");
        }

        private void EquipTool(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= Equipment.Count) return;
            WeaponBridge selectedTool = Equipment[slotIndex];
            if (selectedTool == null) return;

            foreach (var tool in Equipment)
                if (tool != null) tool.Visible = false;

            CurrentWeapon = selectedTool;
            CurrentWeapon.Visible = true;
        }

        private void HandlePickup()
        {
            if (InteractionRay == null) return;
            InteractionRay.ForceRaycastUpdate();
            if (InteractionRay.IsColliding())
            {
                Node collider = InteractionRay.GetCollider() as Node;
                if (collider != null && collider.HasMethod("InteractAndPickup"))
                {
                    Variant result = collider.Call("InteractAndPickup");
                    if (result.VariantType != Variant.Type.Nil)
                    {
                        ItemResource item = (ItemResource)result.AsGodotObject();
                        InventoryManager.Instance.AddItem(item, 1);
                    }
                }
            }
        }

        private void HandleToolUse()
        {
            if (CurrentWeapon == null) return;
            string toolCategory = CurrentWeapon.ToolCategory.Trim().ToLower();
            float staminaCost = 0f;

            if (toolCategory == "axe" || toolCategory == "pickaxe") staminaCost = 5f;
            else if (toolCategory == "shovel" || toolCategory == "wateringcan") staminaCost = 3f;
            else if (toolCategory == "scythe" || toolCategory == "weapon") staminaCost = 2f;
            else if (toolCategory == "seed") staminaCost = 1f;

            if (toolCategory == "weapon")
            {
                if (PlayerStatsManager.Instance.TryConsumeStamina(staminaCost))
                    CurrentWeapon.PerformAttack();
                return;
            }

            if (InteractionRay == null) return;
            InteractionRay.ForceRaycastUpdate();
            if (InteractionRay.IsColliding())
            {
                Node collider = InteractionRay.GetCollider() as Node;
                ResourceNodeBridge hitResource = FindResourceBridgeParent(collider);
                if (hitResource != null)
                {
                    if (toolCategory == "axe" && PlayerStatsManager.Instance.TryConsumeStamina(staminaCost))
                        hitResource.Interact("Chop");
                    else if (toolCategory == "pickaxe" && PlayerStatsManager.Instance.TryConsumeStamina(staminaCost))
                        hitResource.Interact("Mine");
                    return;
                }

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

                    if (!string.IsNullOrEmpty(actionToSend) && PlayerStatsManager.Instance.TryConsumeStamina(staminaCost))
                        hitGarden.Interact(gridCoord, actionToSend, cropIdToPlant);
                }
            }
        }

        private FarmGridBridge FindFarmBridgeParent(Node node)
        {
            Node current = node;
            while (current != null)
            {
                if (current is FarmGridBridge bridge) return bridge;
                current = current.GetParent();
            }
            return null;
        }

        private ResourceNodeBridge FindResourceBridgeParent(Node node)
        {
            Node current = node;
            while (current != null)
            {
                if (current is ResourceNodeBridge bridge) return bridge;
                current = current.GetParent();
            }
            return null;
        }

        private TV FindTVParent(Node node)
        {
            Node current = node;
            while (current != null)
            {
                if (current is TV tv) return tv;
                current = current.GetParent();
            }
            return null;
        }
    }
}