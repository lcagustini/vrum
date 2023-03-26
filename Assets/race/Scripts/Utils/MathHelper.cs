using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathHelper
{
    public const float epsilon = 0.0001f;

    public static Vector2 RandomUnitVector()
    {
        Vector2 vector = new Vector2(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f));
        return vector.normalized;
    }

    public static Vector2 RandomScreenPoint()
    {
        return new Vector2(Random.Range(0, Screen.width), Random.Range(0, Screen.height));
    }
}
