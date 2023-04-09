using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarModel : MonoBehaviourValidated
{
    [SerializeField, Child] private MeshRenderer[] meshes;
    [SerializeField, Child] private Collider[] colliders;
    [SerializeField, Anywhere] public Transform[] wheelVisuals;
    [SerializeField, Anywhere] public Transform[] wheelPositions;
}
