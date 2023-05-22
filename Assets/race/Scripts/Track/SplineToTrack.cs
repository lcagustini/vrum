using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.UIElements;

public class SplineToTrack : ValidatedMonoBehaviour
{
    [SerializeField, Self] private SplineContainer racingLine;
    [SerializeField, Self] private MeshFilter meshFilter;
    [SerializeField, Self] private MeshCollider meshCollider;

    [SerializeField] private float density;

    private void Start()
    {
        Vector3 worldPos = transform.position;
        transform.position = Vector3.zero;

        Mesh mesh = new Mesh();
        List<Vector3> vertices = new();
        List<int> indexes = new();

        for (float i = 0; i < 1; i += density)
        {
            if (racingLine.Evaluate(i, out float3 center, out float3 forward, out float3 up))
            {
                forward = math.normalize(forward);
                float3 right = Vector3.Cross(up, forward);

                Debug.DrawLine(center, center + 20 * forward, Color.blue);
                Debug.DrawLine(center, center + 20 * up, Color.green);
                Debug.DrawLine(center, center + 20 * right, Color.red);

                vertices.Add(center + 20 * right);
                vertices.Add(center - 20 * right);

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
        mesh.SetIndices(indexes, MeshTopology.Triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        meshCollider.sharedMesh = mesh;
        meshFilter.mesh = mesh;

        transform.position = worldPos;
    }
}
