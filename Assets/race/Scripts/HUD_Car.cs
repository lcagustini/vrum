using SceneRefAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD_Car : MonoBehaviourValidated
{
    [SerializeField, Anywhere] private TextMeshProUGUI speed;

    private CarController car;

    private void Update()
    {
        if (car == null) car = FindObjectOfType<CarController>();

        speed.text = (3.6f * car.RB.velocity.magnitude).ToString("F0") + " km/h";
    }
}
