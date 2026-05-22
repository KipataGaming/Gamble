using Godot;
using Game.Core;

namespace Game.Bridge
{
    public partial class ResourceNodeBridge : Node3D
    {
        [Export] public int MaxHealth { get; set; } = 3;
        [Export] public string ResourceType { get; set; } = "Wood"; // Future proofing for Stone

        private ResourceData _logicCore;

        public override void _Ready()
        {
            // Initialize the pure logic
            _logicCore = new ResourceData(MaxHealth);
            
            // Generate the Godot visuals
            GenerateGreyBoxVisuals();
        }

        private void GenerateGreyBoxVisuals()
        {
            // 1. Trunk
            MeshInstance3D trunk = new MeshInstance3D();
            CylinderMesh trunkMesh = new CylinderMesh();
            trunkMesh.TopRadius = 0.2f; trunkMesh.BottomRadius = 0.2f; trunkMesh.Height = 1.0f;
            trunk.Mesh = trunkMesh;
            trunk.Position = new Vector3(0, 0.5f, 0);
            StandardMaterial3D trunkMat = new StandardMaterial3D { AlbedoColor = Colors.SaddleBrown };
            trunk.MaterialOverride = trunkMat;
            AddChild(trunk);

            // 2. Leaves
            MeshInstance3D leaves = new MeshInstance3D();
            BoxMesh leafMesh = new BoxMesh { Size = new Vector3(1.5f, 1.5f, 1.5f) };
            leaves.Mesh = leafMesh;
            leaves.Position = new Vector3(0, 1.5f, 0);
            StandardMaterial3D leafMat = new StandardMaterial3D { AlbedoColor = Colors.ForestGreen };
            leaves.MaterialOverride = leafMat;
            AddChild(leaves);

            // 3. Collider
            StaticBody3D staticBody = new StaticBody3D();
            CollisionShape3D shape = new CollisionShape3D();
            BoxShape3D physicsBox = new BoxShape3D { Size = new Vector3(1.5f, 2.5f, 1.5f) };
            shape.Shape = physicsBox;
            shape.Position = new Vector3(0, 1.25f, 0); // Center the hitbox over the tree
            
            staticBody.AddChild(shape);
            AddChild(staticBody);
        }

        public void Interact(string actionType)
        {
            if (actionType == "Chop")
            {
                bool hitSuccessful = _logicCore.TryHit(1);
                
                if (hitSuccessful)
                {
                    GD.Print($"[ResourceNode] Chopped! Health remaining: {_logicCore.Health}");
                    
                    // Visually wobble or scale down here if you want extra juice
                    
                    if (_logicCore.State == ResourceState.Destroyed)
                    {
                        GD.Print("[ResourceNode] Tree Destroyed!");
                        QueueFree(); // Instantly removes the tree from the world
                    }
                }
            }
        }
    }
}