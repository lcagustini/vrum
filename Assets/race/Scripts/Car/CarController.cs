using KBCore.Refs;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public abstract class CarController : ValidatedMonoBehaviour
{
    [ReadOnly] public Car car;
}
