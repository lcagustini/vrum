using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointCollider : ValidatedMonoBehaviour
{
    [SerializeField, Self] public new BoxCollider collider;
    public int order;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Matrix4x4 matrix = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.color = new Color(0.5f, 0.5f, 1.0f, 0.4f);
        Gizmos.DrawCube(collider.center, collider.size);
        Gizmos.matrix = matrix;
    }
#endif
}
