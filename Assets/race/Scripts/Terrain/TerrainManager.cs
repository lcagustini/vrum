//#define MAIN_THREAD

using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public struct SquareIndexes
{
    public bool valid1;
    public int x1;
    public int y1;
    public int z1;

    public bool valid2;
    public int x2;
    public int y2;
    public int z2;
}

public class TerrainManager : ValidatedMonoBehaviour
{
    const int terrainSize = 80;

    public struct VerticesJob : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;
        public NativeArray<Vector2> uvs;

        public void Execute(int index)
        {
            int i = index / terrainSize;
            int j = index % terrainSize;

            vertices[index] = new Vector3(i, 0, j);
            uvs[index] = new Vector2(i / (float)terrainSize, j / (float)terrainSize);
        }
    }

    public struct IndexesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<Vector3> vertices;
        [ReadOnly] public NativeArray<Vector2> uvs;

        public NativeArray<SquareIndexes> indexes;

        public static int IndexOf<T>(NativeArray<T> array, T value) where T : struct, System.IEquatable<T>
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Equals(value)) return i;
            }
            return -1;
        }

        public void Execute(int index)
        {
            int i = index / terrainSize;
            int j = index % terrainSize;

            SquareIndexes square = new();

            int index1 = IndexOf(vertices, new Vector3(i - 1, 0, j));
            int index2 = IndexOf(vertices, new Vector3(i - 1, 0, j + 1));
            int index3 = IndexOf(vertices, new Vector3(i, 0, j));
            if (index1 != -1 && index2 != -1)
            {
                square.x1 = index1;
                square.y1 = index2;
                square.z1 = index3;
                square.valid1 = true;
            }

            index2 = IndexOf(vertices, new Vector3(i, 0, j - 1));
            if (index1 != -1 && index2 != -1)
            {
                square.x2 = index3;
                square.y2 = index2;
                square.z2 = index1;
                square.valid2 = true;
            }

            indexes[index] = square;
        }
    }

    public struct NoiseJob : IJobParallelFor
    {
        [ReadOnly] public float height;
        [ReadOnly] public float scale;
        public NativeArray<Vector3> vertices;

        public void Execute(int index)
        {
            vertices[index] = new Vector3(vertices[index].x, height * Mathf.PerlinNoise(scale * vertices[index].x, scale * vertices[index].z), vertices[index].z);
        }
    }

    [SerializeField, Self] private MeshFilter meshFilter;
    [SerializeField, Self] private MeshCollider meshCollider;
    [SerializeField] public float scale;
    [SerializeField] public float height;

    private void Start()
    {
        UpdateTerrain();
    }

    private void UpdateTerrain()
    {
        double timer = Time.realtimeSinceStartupAsDouble;
#if MAIN_THREAD
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> indexes = new List<int>();

        for (int i = 0; i < terrainSize; i++)
        {
            for (int j = 0; j < terrainSize; j++)
            {
                vertices.Add(new Vector3(i, 0, j));
                uvs.Add(new Vector2(i / (float)terrainSize, j / (float)terrainSize));

                int index1 = vertices.IndexOf(new Vector3(i - 1, 0, j));
                int index2 = vertices.IndexOf(new Vector3(i - 1, 0, j + 1));
                if (index1 != -1 && index2 != -1)
                {
                    indexes.Add(index1);
                    indexes.Add(index2);
                    indexes.Add(vertices.Count - 1);
                }

                index2 = vertices.IndexOf(new Vector3(i, 0, j - 1));
                if (index1 != -1 && index2 != -1)
                {
                    indexes.Add(vertices.Count - 1);
                    indexes.Add(index2);
                    indexes.Add(index1);
                }
            }
        }

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] = new Vector3(vertices[i].x, height * Mathf.PerlinNoise(scale * vertices[i].x, scale * vertices[i].z), vertices[i].z);
        }

        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indexes, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
#else
        NativeArray<Vector3> vertices = new(terrainSize * terrainSize, Allocator.Persistent);
        NativeArray<Vector2> uvs = new(terrainSize * terrainSize, Allocator.Persistent);
        NativeArray<SquareIndexes> indexes = new(terrainSize * terrainSize, Allocator.Persistent);

        VerticesJob verticesJob = new()
        {
            vertices = vertices,
            uvs = uvs,
        };

        verticesJob.Schedule(terrainSize * terrainSize, 32).Complete();

        IndexesJob indexesJob = new()
        {
            vertices = vertices,
            uvs = uvs,
            indexes = indexes,
        };

        indexesJob.Schedule(terrainSize * terrainSize, 32).Complete();

        NoiseJob noiseJob = new()
        {
            vertices = vertices,
            height = height,
            scale = scale,
        };

        noiseJob.Schedule(terrainSize * terrainSize, 32).Complete();

        List<int> indexesInt = new();
        foreach (SquareIndexes i in indexes)
        {
            if (i.valid1)
            {
                indexesInt.Add(i.x1);
                indexesInt.Add(i.y1);
                indexesInt.Add(i.z1);
            }

            if (i.valid2)
            {
                indexesInt.Add(i.x2);
                indexesInt.Add(i.y2);
                indexesInt.Add(i.z2);
            }
        }

        Mesh mesh = new() { name = "Terrain" };

        mesh.SetVertices(vertices);
        mesh.SetTriangles(indexesInt, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;

        vertices.Dispose();
        uvs.Dispose();
        indexes.Dispose();
#endif
        Debug.Log($"Time spent generating: {Time.realtimeSinceStartupAsDouble - timer}");
    }
}
