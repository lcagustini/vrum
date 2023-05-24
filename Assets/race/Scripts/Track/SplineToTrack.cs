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
        List<Vector2> uvs = new();
        List<int> indexes = new();

        float splineLength = racingLine.CalculateLength();

        for (float i = 0; i < 1; i += density)
        {
            if (racingLine.Evaluate(i, out float3 center, out float3 forward, out float3 up))
            {
                float3 right = Vector3.Cross(up, forward).normalized;

                vertices.Add(center + 10 * right);
                vertices.Add(center - 10 * right);

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

        meshCollider.sharedMesh = mesh;
        meshFilter.mesh = mesh;

        transform.position = worldPos;
    }
}
