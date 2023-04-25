using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public interface ICarController
{
    GameObject GameObject { get; }
    Car Car { get; set; }
}
