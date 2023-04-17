using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartingGridPoint : MonoBehaviour
{
    public int order;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 1, 1, 0.5f);
        Gizmos.DrawCube(transform.position, new Vector3(1, 0.5f, 1));
    }
#endif
}
