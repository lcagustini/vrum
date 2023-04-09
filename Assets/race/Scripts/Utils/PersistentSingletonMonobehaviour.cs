using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentSingletonMonobehaviour<T> : MonoBehaviour where T : PersistentSingletonMonobehaviour<T>
{
    public static T Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            Instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
