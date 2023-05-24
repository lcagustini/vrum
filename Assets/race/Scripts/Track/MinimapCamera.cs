using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : SingletonMonoBehaviourValidated<MinimapCamera>
{
    [SerializeField, Self] private Camera minimapCamera;

    public void Setup()
    {
    }
}
