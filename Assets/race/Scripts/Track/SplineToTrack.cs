using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

[System.Serializable]
public class TransformSnapshot
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;

    public TransformSnapshot() { }

    public TransformSnapshot(Transform transform)
    {
        position = transform.position;
        rotation = transform.rotation;
        scale = transform.localScale;
    }

    public TransformSnapshot(Rigidbody RB)
    {
        position = RB.position;
        rotation = RB.rotation;
        scale = RB.transform.localScale;
    }

    public void ApplySnapshotTo(Transform transform)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;
    }

    public void ApplySnapshotTo(Rigidbody RB)
    {
        RB.position = position;
        RB.rotation = rotation;
        RB.transform.localScale = scale;
    }
}

public class SplineToTrack : ValidatedMonoBehaviour
{
    [SerializeField, Self] public SplineContainer racingLine;

    [SerializeField, Anywhere] public MeshFilter roadMesh;
    [SerializeField, Anywhere] private MeshCollider roadCollider;

    [SerializeField, Anywhere] private MeshFilter dirtMesh;
    [SerializeField, Anywhere] private MeshCollider dirtCollider;

    [SerializeField, Anywhere] private MeshFilter railingsMesh;
    [SerializeField, Anywhere] private MeshCollider railingsCollider;

    [SerializeField] private float radius;
    [SerializeField] private float density;
    [SerializeField] private int checkpointCount;
    [SerializeField] private Vector2Int gridSize;

    [SerializeField] private CheckpointCollider checkpointPrefab;

    public List<TransformSnapshot> gridPoints = new List<TransformSnapshot>();
    public List<CheckpointCollider> checkpoints = new List<CheckpointCollider>();

    private Mesh GenerateRailingMesh(float radius, float height)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new();
        List<Vector2> uvs = new();
        List<int> indexes = new();

        float splineLength = racingLine.CalculateLength();

        for (float i = 0; i < 1; i += density)
        {
            if (racingLine.Evaluate(i, out float3 center, out float3 forward, out float3 up))
            {
                float3 right = Vector3.Cross(up, forward).normalized;

                vertices.Add(center + radius * right);
                vertices.Add(center + radius * right + height * up);

                vertices.Add(center - radius * right);
                vertices.Add(center - radius * right + height * up);

                float scale = splineLength / (vertices[vertices.Count - 1] - vertices[vertices.Count - 2]).magnitude;

                uvs.Add(new Vector2(1, scale * i));
                uvs.Add(new Vector2(0, scale * i));

                uvs.Add(new Vector2(1, scale * i));
                uvs.Add(new Vector2(0, scale * i));

                if (vertices.Count >= 8)
                {
                    indexes.Add(vertices.Count - 3);
                    indexes.Add(vertices.Count - 7);
                    indexes.Add(vertices.Count - 8);

                    indexes.Add(vertices.Count - 8);
                    indexes.Add(vertices.Count - 4);
                    indexes.Add(vertices.Count - 3);

                    indexes.Add(vertices.Count - 6);
                    indexes.Add(vertices.Count - 5);
                    indexes.Add(vertices.Count - 1);

                    indexes.Add(vertices.Count - 1);
                    indexes.Add(vertices.Count - 2);
                    indexes.Add(vertices.Count - 6);
                }
            }
        }

        //indexes.Add(vertices.Count - 1);
        //indexes.Add(1);
        //indexes.Add(0);

        //indexes.Add(0);
        //indexes.Add(vertices.Count - 2);
        //indexes.Add(vertices.Count - 1);

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indexes, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    private Mesh GenerateTrackMesh(float radius)
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new();
        List<Vector2> uvs = new();
        List<int> indexes = new();

        float splineLength = racingLine.CalculateLength();

        for (float i = 0; i < 1; i += density)
        {
            if (racingLine.Evaluate(i, out float3 center, out float3 forward, out float3 up))
            {
                float3 right = Vector3.Cross(up, forward).normalized;

                vertices.Add(center + radius * right);
                vertices.Add(center - radius * right);

                float scale = splineLength / (vertices[vertices.Count - 1] - vertices[vertices.Count - 2]).magnitude;

                uvs.Add(new Vector2(1, scale * i));
                uvs.Add(new Vector2(0, scale * i));

                if (vertices.Count >= 4)
                {
                    indexes.Add(vertices.Count - 4);
                    indexes.Add(vertices.Count - 3);
                    indexes.Add(vertices.Count - 1);

                    indexes.Add(vertices.Count - 1);
                    indexes.Add(vertices.Count - 2);
                    indexes.Add(vertices.Count - 4);
                }
            }
        }

        indexes.Add(vertices.Count - 1);
        indexes.Add(1);
        indexes.Add(0);

        indexes.Add(0);
        indexes.Add(vertices.Count - 2);
        indexes.Add(vertices.Count - 1);

        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetIndices(indexes, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        return mesh;
    }

    private void CreateStartingGrid()
    {
        for (int i = 1; i <= gridSize.y; i++)
        {
            if (racingLine.Evaluate(1 - (i / ((gridSize.y + 1) * (float)checkpointCount)), out float3 center, out float3 forward, out float3 up))
            {
                float3 right = Vector3.Cross(up, forward).normalized;

                Vector3 rightPoint = center + radius * right;
                Vector3 leftPoint = center - radius * right;

                float thickness = 2 * radius;
                float spacing = thickness / gridSize.x;

                for (int j = 0; j < gridSize.x; j++)
                {
                    TransformSnapshot snapshot = new TransformSnapshot()
                    {
                        position = leftPoint + (j + 0.5f) * spacing * (Vector3)right,
                        rotation = Quaternion.FromToRotation(Vector3.forward, forward),
                        scale = Vector3.one
                    };
                    gridPoints.Add(snapshot);
                }
            }
        }
    }

    private void CreateCheckpoints()
    {
        for (int i = 0; i < checkpointCount; i++)
        {
            if (racingLine.Evaluate(i / (float)checkpointCount, out float3 center, out float3 forward, out float3 up))
            {
                CheckpointCollider checkpoint = Instantiate(checkpointPrefab, transform);
                checkpoint.transform.position = center + up * checkpoint.collider.size.y / 2;
                checkpoint.transform.rotation = Quaternion.FromToRotation(Vector3.forward, forward);
                checkpoint.order = i;
                checkpoints.Add(checkpoint);
            }
        }
    }

    private void Awake()
    {
        Mesh mesh;

        mesh = GenerateTrackMesh(radius);
        roadCollider.sharedMesh = mesh;
        roadMesh.mesh = mesh;

        mesh = GenerateTrackMesh(3 * radius);
        dirtCollider.sharedMesh = mesh;
        dirtMesh.mesh = mesh;
        dirtCollider.transform.position = new Vector3(0, -0.001f, 0);

        mesh = GenerateRailingMesh(3 * radius, 5f);
        railingsCollider.sharedMesh = mesh;
        railingsMesh.mesh = mesh;

        CreateStartingGrid();
        CreateCheckpoints();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        foreach (TransformSnapshot p in gridPoints)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(p.position, 0.1f);
        }
    }
#endif
}
