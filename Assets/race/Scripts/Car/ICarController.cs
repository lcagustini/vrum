using Cinemachine;
using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public interface ICarController
{
    CinemachineVirtualCamera VirtualCamera { get; }
    GameObject GameObject { get; }
    Car Car { get; set; }
}
