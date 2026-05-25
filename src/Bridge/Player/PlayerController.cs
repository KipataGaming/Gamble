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

        private VehicleController _currentVehicle;
        private bool _isInVehicle = false;
        private TV _currentTV;
        private Node _currentInteractable;

        private bool _isDead = false;
        private Vector3 _spawnPosition;

        public override void _Ready()
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _spawnPosition = GlobalPosition;

            Equipment.Clear();
            var weaponContainer = GetNodeOrNull<Node>("Node3D/WeaponNode");
            if (weaponContainer == null)
                weaponContainer = FindChild("WeaponNode", true, false) as Node;

            if (weaponContainer != null)
            {
                foreach (Node child in weaponContainer.GetChildren())
                {
                    if (child is WeaponBridge weapon)
                    {
                        Equipment.Add(weapon);
                        weapon.Visible = false;
                    }
                }
                GD.Print($"[Player] Loaded {Equipment.Count} tools into Equipment array");
            }
        }

        public override void _Process(double delta)
        {
            if (GetTree().Paused || _isDead) return;

            if (_isInVehicle)
            {
                if (InteractionPrompt != null)
                    InteractionPrompt.Visible = false;

                if (Input.IsActionJustPressed("interact"))
                {
                    if (_currentVehicle != null)
                    {
                        // Improved exit position - behind and to the right
                        Vector3 backDirection = -_currentVehicle.GlobalTransform.Basis.Z;
                        Vector3 rightDirection = _currentVehicle.GlobalTransform.Basis.X;

                        Vector3 exitPos = _currentVehicle.GlobalPosition 
                                          + backDirection * 2.5f 
                                          + rightDirection * 1.5f 
                                          + Vector3.Up * 1.0f;

                        _currentVehicle.ExitVehicle(exitPos);
                        GlobalPosition = exitPos;   // ← Move player to exit position

                    }

                    _isInVehicle = false;
                    Visible = true;
                    SetPhysicsProcess(true);

                    if (PlayerCamera != null)
                        PlayerCamera.Current = true;

                    _currentVehicle = null;
                    GD.Print("[Player] Exited vehicle");
                }
                return;
            }

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

                    if (current is CCTVMonitor monitor)
                    {
                        _currentInteractable = monitor;
                        promptText = "Press F to use CCTV";
                        break;
                    }

                    if (current is VehicleController vehicle)
                    {
                        _currentVehicle = vehicle;
                        promptText = "Press F to enter vehicle";
                        break;
                    }

                    current = current.GetParent();
                }
            }

            if (InteractionPrompt != null)
            {
                InteractionPrompt.Text = promptText;
                InteractionPrompt.Visible = !string.IsNullOrEmpty(promptText);
            }

            if (Input.IsActionJustPressed("interact"))
            {
                if (_currentVehicle != null)
                {
                    _currentVehicle.EnterVehicle(this);
                    _isInVehicle = true;
                    Visible = false;
                    SetPhysicsProcess(false);
                    if (InteractionPrompt != null) InteractionPrompt.Visible = false;
                    GD.Print("[Player] Entered vehicle");
                }
                else if (_currentInteractable != null && _currentInteractable.HasMethod("OnPlayerInteract"))
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

            if (@event is InputEventMouseMotion mouseMotion && PlayerCamera != null)
            {
                var blackjackUI = GetTree().Root.GetNodeOrNull<BlackjackUI>("/root/BlackjackUI");
                if (blackjackUI != null && blackjackUI.Visible)
                    return;

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
                if (keyEvent2.Keycode == Key.Key8) EquipTool(7);

                if (keyEvent2.Keycode == Key.T)
                {
                    var farm = GetTree().Root.GetNodeOrNull<FarmGridBridge>("/root/FarmGridBridge");
                    if (farm != null && farm.HasMethod("SimulateDayPassing"))
                        farm.Call("SimulateDayPassing");
                }

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

            bool inWater = IsInWater();
            Vector3 velocity = Velocity;

            if (inWater)
            {
                velocity.Y -= 1.5f * (float)delta;
                if (Input.IsActionPressed("jump"))
                    velocity.Y = 5.0f;

                Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
                Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
                float swimSpeed = 4.0f;

                if (direction != Vector3.Zero)
                {
                    velocity.X = direction.X * swimSpeed;
                    velocity.Z = direction.Z * swimSpeed;
                }
                else
                {
                    velocity.X = Mathf.MoveToward(Velocity.X, 0, swimSpeed);
                    velocity.Z = Mathf.MoveToward(Velocity.Z, 0, swimSpeed);
                }
            }
            else
            {
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
            }

            Velocity = velocity;
            MoveAndSlide();
        }

        private bool _isInWater = false;

        public void SetInWater(bool value) { _isInWater = value; }
        public bool IsInWater() => _isInWater;

        public void TakeDamage(int amount)
        {
            if (_isDead || PlayerStatsManager.Instance.CurrentHealth <= 0) return;
            GD.Print($"[Player] -> HIT! Took {amount} damage.");
            PlayerStatsManager.Instance.DecreaseHealth(amount);
            if (PlayerStatsManager.Instance.CurrentHealth <= 0) Die();
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

            if (toolCategory == "fishingrod" || toolCategory == "fishing")
            {
                if (IsInWater()) StartFishing();
                else GD.Print("[Fishing] You need to be in water to fish.");
                return;
            }

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

        private async void StartFishing()
        {
            GD.Print("[Fishing] Casting line...");
            float waitTime = (float)GD.RandRange(2.5, 5.0);
            await ToSignal(GetTree().CreateTimer(waitTime), "timeout");

            if (GD.Randf() < 0.75f)
            {
                GD.Print("[Fishing] You got a bite!");
                InventoryManager.Instance.AddItemById("fish", 1);
                GD.Print("[Fishing] You caught a Fish!");
            }
            else
            {
                GD.Print("[Fishing] The fish got away...");
            }
        }

        private void HandleVehicleInteraction()
        {
            // Not used in main flow
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