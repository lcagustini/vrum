using SceneRefAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HUD_Car : MonoBehaviourValidated
{
    [SerializeField, Anywhere] private CarController car;
    [SerializeField, Anywhere] private TextMeshProUGUI speed;

    private void Update()
    {
        speed.text = (3.6f * car.RB.velocity.magnitude).ToString("F0") + " km/h";
    }
}
