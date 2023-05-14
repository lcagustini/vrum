//#define MAIN_THREAD

using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TerrainManager : ValidatedMonoBehaviour
{
    public struct TerrainJob : IJobParallelFor
    {
        [ReadOnly] public Vector3Int terrainOffset;

        public NativeArray<Vector3> vertices;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> indexes;

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
            int i = index / 60;
            int j = index % 60;

            vertices[index] = new Vector3(i, 0, j);
            uvs[index] = new Vector2(i / 60.0f, j / 60.0f);

            int index1 = IndexOf(vertices, new Vector3(i - 1, 0, j));
            int index2 = IndexOf(vertices, new Vector3(i - 1, 0, j + 1));
            if (index1 != -1 && index2 != -1)
            {
                indexes[3 * index] = index1;
                indexes[3 * index + 1] = index2;
                indexes[3 * index + 2] = index;
            }

            index2 = IndexOf(vertices, new Vector3(i, 0, j - 1));
            if (index1 != -1 && index2 != -1)
            {
                indexes[3 * index] = index;
                indexes[3 * index + 1] = index2;
                indexes[3 * index + 2] = index1;
            }
        }
    }

    [SerializeField, Self] private MeshFilter meshFilter;
    [SerializeField, Self] private MeshCollider meshCollider;
    [SerializeField] public float scale;
    [SerializeField] public float height;

#if MAIN_THREAD
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs = new List<Vector2>();
    private List<int> indexes = new List<int>();
#endif

    private Car car;
    private Vector3Int terrainOffset;

    private void Start()
    {
        UpdateTerrain();
    }

    private void UpdateTerrain()
    {
#if MAIN_THREAD
        vertices.Clear();
        indexes.Clear();
        uvs.Clear();

        for (int i = terrainOffset.x - 20; i < 40 + terrainOffset.x; i++)
        {
            for (int j = terrainOffset.z - 20; j < 40 + terrainOffset.z; j++)
            {
                vertices.Add(new Vector3(i, 0, j));
                uvs.Add(new Vector2(i / 60.0f, j / 60.0f));

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
        NativeArray<Vector3> vertices = new NativeArray<Vector3>(60 * 60, Allocator.Persistent);
        NativeArray<Vector2> uvs = new NativeArray<Vector2>(60 * 60, Allocator.Persistent);
        NativeArray<int> indexes = new NativeArray<int>(3 * 2 * 59 * 59, Allocator.Persistent);

        TerrainJob job = new TerrainJob()
        {
            terrainOffset = terrainOffset,
            vertices = vertices,
            uvs = uvs,
            indexes = indexes
        };

        job.Schedule(60 * 60, 30).Complete();

        vertices.Dispose();
        uvs.Dispose();
        indexes.Dispose();
#endif
    }

    private Vector3Int Vector3F2I(Vector3 v) => new Vector3Int(Mathf.RoundToInt(car.transform.position.x), Mathf.RoundToInt(car.transform.position.y), Mathf.RoundToInt(car.transform.position.z));

    private void Update()
    {
        if (car == null && RaceManager.Instance.racingCars.Count > 0) car = RaceManager.Instance.racingCars[0];
        if (car == null) return;

        Vector3Int expectedTerrainOffset = Vector3F2I(car.transform.position);
        expectedTerrainOffset.y = 0;
        if (expectedTerrainOffset != terrainOffset)
        {
            terrainOffset = expectedTerrainOffset;
            UpdateTerrain();
        }
    }
}
