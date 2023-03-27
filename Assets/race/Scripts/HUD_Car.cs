using SceneRefAttributes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUD_Car : MonoBehaviourValidated
{
    [SerializeField, Anywhere] private TextMeshProUGUI speed;
    [SerializeField, Anywhere] private TextMeshProUGUI gear;
    [SerializeField, Anywhere] private Slider gearRatio;

    private CarController car;

    private float GetGearRatio()
    {
        float forwardComponent = Vector3.Dot(car.transform.forward, car.RB.velocity);
        float forwardRatio = forwardComponent / car.config.topSpeed;
        AnimationCurve gearCurve = car.config.motorTorqueResponseCurve[car.inputData.gear];

        float rpm = (forwardRatio - gearCurve.keys[0].time) / (gearCurve.keys[gearCurve.length - 1].time - gearCurve.keys[0].time);

        return Mathf.Clamp01(car.inputData.gear == 0 ? 1 - rpm : rpm);
    }

    private void Update()
    {
        if (car == null) car = FindObjectOfType<CarController>();

        speed.text = (3.6f * car.RB.velocity.magnitude).ToString("F0") + " km/h";
        gear.text = car.inputData.gear == 0 ? "R" : car.inputData.gear.ToString();
        gearRatio.value = GetGearRatio();
    }
}
