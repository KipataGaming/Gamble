#if TOOLS
using Godot;
using System.Collections.Generic;

[Tool]
public partial class ProceduralRockGenerator : EditorScript
{
	[Export] public int NumberOfRocks = 5;
	[Export] public string OutputFolder = "res://src/Resources/Rocks/";

	public override void _Run()
	{
		GD.Print("Generating procedural rocks...");

		for (int i = 0; i < NumberOfRocks; i++)
		{
			var mesh = GenerateRockMesh(i);
			string meshPath = $"{OutputFolder}rock_{i}.mesh";
			ResourceSaver.Save(mesh, meshPath);
			GD.Print($"Saved: {meshPath}");
		}

		GD.Print("Done generating rocks!");
	}

	private ArrayMesh GenerateRockMesh(int seed)
	{
		var surfaceTool = new SurfaceTool();
		surfaceTool.Begin(Mesh.PrimitiveType.Triangles);

		var noise = new FastNoiseLite();
		noise.Seed = seed;
		noise.Frequency = 0.8f;
		noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;

		// Create a basic subdivided sphere
		int rings = 12;
		int radialSegments = 16;
		float radius = 1.0f;

		for (int i = 0; i <= rings; i++)
		{
			float v = i / (float)rings;
			float phi = Mathf.Pi * v;

			for (int j = 0; j <= radialSegments; j++)
			{
				float u = j / (float)radialSegments;
				float theta = Mathf.Tau * u;

				float x = Mathf.Sin(phi) * Mathf.Cos(theta);
				float y = Mathf.Cos(phi);
				float z = Mathf.Sin(phi) * Mathf.Sin(theta);

				Vector3 vertex = new Vector3(x, y, z) * radius;

				// Apply noise deformation
				float noiseValue = noise.GetNoise3Dv(vertex * 2.0f);
				vertex += vertex.Normalized() * noiseValue * 0.35f;

				surfaceTool.SetNormal(vertex.Normalized());
				surfaceTool.AddVertex(vertex);
			}
		}

		// Generate indices (simplified)
		for (int i = 0; i < rings; i++)
		{
			for (int j = 0; j < radialSegments; j++)
			{
				int current = i * (radialSegments + 1) + j;
				int next = current + radialSegments + 1;

				surfaceTool.AddIndex(current);
				surfaceTool.AddIndex(next);
				surfaceTool.AddIndex(current + 1);

				surfaceTool.AddIndex(current + 1);
				surfaceTool.AddIndex(next);
				surfaceTool.AddIndex(next + 1);
			}
		}

		surfaceTool.GenerateNormals();
		return surfaceTool.Commit();
	}
}
#endif
