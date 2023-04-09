using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointCollider : MonoBehaviourValidated
{
    [SerializeField, Self] public new BoxCollider collider;
    public int order;
}
