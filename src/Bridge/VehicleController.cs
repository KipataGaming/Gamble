using Godot;
using Game.Core;
using Game.Resources;
using Game.Bridge;

namespace Game.Bridge
{
    public partial class VehicleController : VehicleBody3D
    {
        [Export] public float EnginePower = 300f;
        [Export] public float SteeringPower = 0.8f;
        [Export] public Camera3D VehicleCamera;

        public bool HasDriver { get; set; } = false;

        public override void _PhysicsProcess(double delta)
        {
            if (!HasDriver)
            {
                EngineForce = 0;
                Steering = 0;
                return;
            }

            float steering = Input.GetAxis("move_left", "move_right") * SteeringPower;
            float throttle = Input.GetAxis("move_backward", "move_forward");

            Steering = steering;
            EngineForce = throttle * EnginePower;
        }

        public void EnterVehicle(Node3D player)
        {
            HasDriver = true;

            if (VehicleCamera != null)
                VehicleCamera.Current = true;

            GD.Print("[Vehicle] Player entered vehicle");
        }

        public void ExitVehicle(Vector3 exitPosition)
        {
            HasDriver = false;

            if (VehicleCamera != null)
                VehicleCamera.Current = false;

            GD.Print($"[Vehicle] ExitVehicle called. Exit position received: {exitPosition}");
        }
    }
}