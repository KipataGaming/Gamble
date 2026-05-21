using Godot;
using Game.Core;
using Game.Resources;

namespace Game.Bridge
{
    public partial class PlayerController : CharacterBody3D
    {
        [Export] public Camera3D PlayerCamera = null!;
        [Export] public RayCast3D InteractionRay = null!;
        [Export] public WeaponBridge CurrentWeapon = null!; // Link this in the inspector
        
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

            // Interaction (F Key)
            if (Input.IsActionJustPressed("interact"))
            {
                HandleInteraction();
            }

            // Attack (Left Click)
            if (Input.IsActionJustPressed("attack"))
            {
                if (CurrentWeapon != null)
                {
                    CurrentWeapon.PerformAttack();
                }
                else
                {
                    GD.Print("[Player] Punching!");
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (GetTree().Paused) return;

            if (@event is InputEventMouseMotion mouseMotion && PlayerCamera != null)
            {
                RotateY(-mouseMotion.Relative.X * MouseSensitivity);
                PlayerCamera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
                Vector3 rot = PlayerCamera.Rotation;
                rot.X = Mathf.Clamp(rot.X, -Mathf.Pi / 2, Mathf.Pi / 2);
                PlayerCamera.Rotation = rot;
            }
        }

        private void HandleInteraction()
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
                        GD.Print($"[Player] Interaction successful: {item.Name}");
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
    }
}