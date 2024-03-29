using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMonoBehaviourValidated<T> : ValidatedMonoBehaviour where T : SingletonMonoBehaviourValidated<T>
{
    public static T Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this as T;
        else Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
