#if TOOLS
using Godot;

[Tool]
public partial class ProceduralTreeGenerator : EditorScript
{
	[Export] public int NumberOfTrunks = 4;
	[Export] public int NumberOfLeafSets = 5;

	[Export] public string TrunkFolder = "res://src/Resources/Trees/Trunks/";
	[Export] public string LeafFolder = "res://src/Resources/Trees/Leaves/";

	public override void _Run()
	{
		GD.Print("Generating procedural trees...");

		GenerateTrunks();
		GenerateLeaves();

		GD.Print("Tree generation complete!");
	}

	private void GenerateTrunks()
	{
		for (int i = 0; i < NumberOfTrunks; i++)
		{
			var mesh = CreateTrunkMesh(i);
			string path = $"{TrunkFolder}trunk_{i}.mesh";
			ResourceSaver.Save(mesh, path);
			GD.Print($"Saved trunk: {path}");
		}
	}

	private void GenerateLeaves()
	{
		for (int i = 0; i < NumberOfLeafSets; i++)
		{
			var mesh = CreateLeafMesh(i);
			string path = $"{LeafFolder}leaves_{i}.mesh";
			ResourceSaver.Save(mesh, path);
			GD.Print($"Saved leaves: {path}");
		}
	}

	private ArrayMesh CreateTrunkMesh(int seed)
{
	var st = new SurfaceTool();
	st.Begin(Mesh.PrimitiveType.Triangles);

	var noise = new FastNoiseLite();
	noise.Seed = seed;
	noise.Frequency = 1.1f;
	noise.NoiseType = FastNoiseLite.NoiseTypeEnum.Simplex;

	int heightSegments = 10;
	int radialSegments = 10;
	float height = 4.0f;
	float baseRadius = 0.75f;     // Thicker base
	float topRadius = 0.45f;      // Narrower top

	for (int y = 0; y <= heightSegments; y++)
	{
		float v = y / (float)heightSegments;
		float radius = Mathf.Lerp(baseRadius, topRadius, v);

		for (int x = 0; x <= radialSegments; x++)
		{
			float u = x / (float)radialSegments;
			float angle = u * Mathf.Tau;

			float nx = Mathf.Cos(angle);
			float nz = Mathf.Sin(angle);

			Vector3 pos = new Vector3(nx * radius, v * height, nz * radius);

			// Add noise for organic shape
			float offset = noise.GetNoise3Dv(pos * 2.0f) * 0.2f;
			pos += new Vector3(nx, 0, nz).Normalized() * offset;

			st.SetNormal(new Vector3(nx, 0.3f, nz).Normalized());
			st.AddVertex(pos);
		}
	}

	// Create triangles
	for (int y = 0; y < heightSegments; y++)
	{
		for (int x = 0; x < radialSegments; x++)
		{
			int i0 = y * (radialSegments + 1) + x;
			int i1 = i0 + 1;
			int i2 = (y + 1) * (radialSegments + 1) + x;
			int i3 = i2 + 1;

			st.AddIndex(i0); st.AddIndex(i2); st.AddIndex(i1);
			st.AddIndex(i1); st.AddIndex(i2); st.AddIndex(i3);
		}
	}

	st.GenerateNormals();
	return st.Commit();
}

	private ArrayMesh CreateLeafMesh(int seed)
{
	var st = new SurfaceTool();
	st.Begin(Mesh.PrimitiveType.Triangles);

	var rng = new RandomNumberGenerator();
	rng.Seed = (ulong)seed;

	int clusters = 6;           // How many groups of leaves
	int planesPerCluster = 4;   // Planes per group

	for (int c = 0; c < clusters; c++)
	{
		// Random center for this leaf cluster
		Vector3 center = new Vector3(
			rng.RandfRange(-1.3f, 1.3f),
			3.0f + c * 0.4f,
			rng.RandfRange(-1.3f, 1.3f)
		);

		for (int p = 0; p < planesPerCluster; p++)
		{
			float angle = rng.RandfRange(0, Mathf.Tau);
			float size = rng.RandfRange(1.3f, 2.0f);

			Vector3 right = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * size;
			Vector3 up = Vector3.Up * size * 0.6f;

			Vector3 v0 = center - right * 0.5f - up * 0.5f;
			Vector3 v1 = center + right * 0.5f - up * 0.5f;
			Vector3 v2 = center + right * 0.5f + up * 0.5f;
			Vector3 v3 = center - right * 0.5f + up * 0.5f;

			// Front face
			st.AddVertex(v0); st.AddVertex(v1); st.AddVertex(v2);
			st.AddVertex(v0); st.AddVertex(v2); st.AddVertex(v3);

			// Back face (visible from both sides)
			st.AddVertex(v0); st.AddVertex(v2); st.AddVertex(v1);
			st.AddVertex(v0); st.AddVertex(v3); st.AddVertex(v2);
		}
	}

	st.GenerateNormals();
	return st.Commit();
}
}
#endif
