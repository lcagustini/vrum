using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CarTemplate : MonoBehaviourValidated
{
    [SerializeField, Child] public CinemachineVirtualCamera virtualCamera;
}
