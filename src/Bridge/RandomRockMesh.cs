using Godot;
using Godot.Collections;

[Tool]
public partial class RandomRockMesh : MeshInstance3D
{
    [Export] public bool RandomizeOnReady = true;
    [Export] public bool GenerateCollision = true;

    [Export] public Array<ArrayMesh> RockMeshes = new();

    public override void _Ready()
    {
        if (RandomizeOnReady && Mesh == null)
        {
            SetRandomMesh();
        }

        if (GenerateCollision)
        {
            GenerateConvexCollision();
        }
    }

    public void SetRandomMesh()
    {
        if (RockMeshes.Count == 0)
        {
            GD.PrintErr("RandomRockMesh: No rock meshes assigned in inspector!");
            return;
        }

        var rng = new RandomNumberGenerator();
        rng.Randomize();

        int index = rng.RandiRange(0, RockMeshes.Count - 1);
        Mesh = RockMeshes[index];
    }

    private void GenerateConvexCollision()
    {
        if (Mesh == null) return;

        var staticBody = GetNodeOrNull<StaticBody3D>("StaticBody3D");
        if (staticBody == null)
        {
            staticBody = new StaticBody3D { Name = "StaticBody3D" };
            AddChild(staticBody);
            if (Engine.IsEditorHint()) staticBody.Owner = GetTree().EditedSceneRoot;
        }

        var collisionShape = staticBody.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        if (collisionShape == null)
        {
            collisionShape = new CollisionShape3D { Name = "CollisionShape3D" };
            staticBody.AddChild(collisionShape);
            if (Engine.IsEditorHint()) collisionShape.Owner = GetTree().EditedSceneRoot;
        }

        collisionShape.Shape = Mesh.CreateConvexShape();
    }
}