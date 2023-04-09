using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTransform : MonoBehaviour
{
    public float speed;

    void Update()
    {
        transform.rotation *= Quaternion.Euler(0, speed * Time.deltaTime, 0);
    }
}
