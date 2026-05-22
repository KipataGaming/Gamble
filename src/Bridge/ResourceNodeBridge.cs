using Godot;
using Game.Core;

namespace Game.Bridge
{
    public partial class ResourceNodeBridge : Node3D
    {
        [Export] public int MaxHealth { get; set; } = 3;
        [Export] public string ResourceType { get; set; } = "Tree"; // Type "Tree" or "Rock" in the Inspector

        private ResourceData _logicCore;

        public override void _Ready()
        {
            _logicCore = new ResourceData(MaxHealth);
            GenerateGreyBoxVisuals();
        }

        private void GenerateGreyBoxVisuals()
        {
            string type = ResourceType.Trim().ToLower();

            if (type == "tree")
            {
                // 1. Trunk
                MeshInstance3D trunk = new MeshInstance3D();
                CylinderMesh trunkMesh = new CylinderMesh { TopRadius = 0.2f, BottomRadius = 0.2f, Height = 1.0f };
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

                // 3. Tree Collider
                StaticBody3D staticBody = new StaticBody3D();
                CollisionShape3D shape = new CollisionShape3D();
                BoxShape3D physicsBox = new BoxShape3D { Size = new Vector3(1.5f, 2.5f, 1.5f) };
                shape.Shape = physicsBox;
                shape.Position = new Vector3(0, 1.25f, 0);
                
                staticBody.AddChild(shape);
                AddChild(staticBody);
            }
            else if (type == "rock")
            {
                // 1. Rock Mesh (Using a simple sphere)
                MeshInstance3D rock = new MeshInstance3D();
                SphereMesh rockMesh = new SphereMesh { Radius = 0.6f, Height = 1.2f }; 
                rock.Mesh = rockMesh;
                rock.Position = new Vector3(0, 0.6f, 0);
                StandardMaterial3D rockMat = new StandardMaterial3D { AlbedoColor = Colors.SlateGray };
                rock.MaterialOverride = rockMat;
                AddChild(rock);

                // 2. Rock Collider
                StaticBody3D staticBody = new StaticBody3D();
                CollisionShape3D shape = new CollisionShape3D();
                SphereShape3D physicsSphere = new SphereShape3D { Radius = 0.6f };
                shape.Shape = physicsSphere;
                shape.Position = new Vector3(0, 0.6f, 0);
                
                staticBody.AddChild(shape);
                AddChild(staticBody);
            }
        }

        public void Interact(string actionType)
        {
            string type = ResourceType.Trim().ToLower();

            // Only allow Chop for Trees, and Mine for Rocks
            if ((actionType == "Chop" && type == "tree") || (actionType == "Mine" && type == "rock"))
            {
                bool hitSuccessful = _logicCore.TryHit(1);
                
                if (hitSuccessful)
                {
                    GD.Print($"[ResourceNode] {actionType} successful! Health remaining: {_logicCore.Health}");
                    
                    if (_logicCore.State == ResourceState.Destroyed)
                    {
                        GD.Print($"[ResourceNode] {ResourceType} Destroyed!");
                        QueueFree(); // Instantly removes the node from the world
                    }
                }
            }
        }
    }
}