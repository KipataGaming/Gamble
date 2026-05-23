using Godot;
using Godot.Collections;

[Tool]
public partial class RandomTreeMesh : Node3D
{
    [Export] public bool RandomizeOnReady = true;
    [Export] public bool GenerateCollision = true;

    [ExportGroup("Meshes")]
    [Export] public Array<ArrayMesh> TrunkMeshes = new();
    [Export] public Array<ArrayMesh> LeafMeshes = new();

    [ExportGroup("Randomization")]
    [Export] public float MinScale = 0.85f;
    [Export] public float MaxScale = 1.25f;
    [Export] public float LeafDensityMin = 0.85f;
    [Export] public float LeafDensityMax = 1.4f;

    private MeshInstance3D _trunkInstance;
    private MeshInstance3D _leafInstance;

    public override void _Ready()
    {
        if (RandomizeOnReady)
        {
            GenerateRandomTree();
        }

        if (GenerateCollision)
        {
            GenerateCollisions();
        }
    }

    public void GenerateRandomTree()
    {
        if (TrunkMeshes.Count == 0 || LeafMeshes.Count == 0)
        {
            GD.PrintErr("RandomTreeMesh: No trunk or leaf meshes assigned!");
            return;
        }

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        // === Trunk ===
        _trunkInstance = GetNodeOrNull<MeshInstance3D>("Trunk");
        if (_trunkInstance == null)
        {
            _trunkInstance = new MeshInstance3D { Name = "Trunk" };
            AddChild(_trunkInstance);
        }

        int trunkIndex = rng.RandiRange(0, TrunkMeshes.Count - 1);
        _trunkInstance.Mesh = TrunkMeshes[trunkIndex];

        // === Leaves ===
        _leafInstance = GetNodeOrNull<MeshInstance3D>("Leaves");
        if (_leafInstance == null)
        {
            _leafInstance = new MeshInstance3D { Name = "Leaves" };
            AddChild(_leafInstance);
        }

        int leafIndex = rng.RandiRange(0, LeafMeshes.Count - 1);
        _leafInstance.Mesh = LeafMeshes[leafIndex];

        // Random leaf density
        float leafScale = rng.RandfRange(LeafDensityMin, LeafDensityMax);
        _leafInstance.Scale = Vector3.One * leafScale;

        // Random overall scale + rotation
        float scale = rng.RandfRange(MinScale, MaxScale);
        Scale = Vector3.One * scale;

        RotationDegrees = new Vector3(0, rng.RandfRange(0, 360), 0);
    }

    private void GenerateCollisions()
    {
        if (_trunkInstance?.Mesh != null)
            CreateConvexCollision(_trunkInstance, "TrunkCollision");

        if (_leafInstance?.Mesh != null)
            CreateConvexCollision(_leafInstance, "LeavesCollision");
    }

    private void CreateConvexCollision(MeshInstance3D meshInstance, string name)
    {
        var staticBody = meshInstance.GetNodeOrNull<StaticBody3D>(name);
        if (staticBody == null)
        {
            staticBody = new StaticBody3D { Name = name };
            meshInstance.AddChild(staticBody);
            if (Engine.IsEditorHint()) staticBody.Owner = GetTree().EditedSceneRoot;
        }

        var shapeNode = staticBody.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        if (shapeNode == null)
        {
            shapeNode = new CollisionShape3D { Name = "CollisionShape3D" };
            staticBody.AddChild(shapeNode);
            if (Engine.IsEditorHint()) shapeNode.Owner = GetTree().EditedSceneRoot;
        }

        shapeNode.Shape = meshInstance.Mesh.CreateConvexShape();
    }
}