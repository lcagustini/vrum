using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersistentSingletonMonoBehaviourValidated<T> : MonoBehaviourValidated where T : PersistentSingletonMonoBehaviourValidated<T>
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
