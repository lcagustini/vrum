using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : SingletonMonoBehaviourValidated<MinimapCamera>
{
    [SerializeField, Self] private Camera minimapCamera;

    public void Setup()
    {
        Mesh trackMesh = LapManager.Instance.Track.roadMesh.mesh;

        float size = Mathf.Max(trackMesh.bounds.extents.x, trackMesh.bounds.extents.z);

        minimapCamera.transform.position = trackMesh.bounds.center + new Vector3(0, 10, 0);
        minimapCamera.orthographicSize = size;
    }
}
