using KBCore.Refs;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUD_Car : ValidatedMonoBehaviour
{
    [SerializeField, Anywhere] private TextMeshProUGUI speed;
    [SerializeField, Anywhere] private TextMeshProUGUI gear;
    [SerializeField, Anywhere] private Slider gearRatio;
    [SerializeField, Anywhere] private Image gearRatioImage;

    [SerializeField, Anywhere] private TextMeshProUGUI bestTime;
    [SerializeField, Anywhere] private TextMeshProUGUI currentTime;

    [SerializeField, Anywhere] private Slider acceleration;
    [SerializeField, Anywhere] private Slider brake;
    [SerializeField, Anywhere] private Slider steer;

    [SerializeField, Anywhere] private TextMeshProUGUI[] positions;

    private Car car;

    private string FormatTime(float time)
    {
        int mili = (int)((time % 1) * 100);
        int sec = (int)(time % 60);
        int min = (int)(time / 60);

        return $"{min.ToString("00")}:{sec.ToString("00")}:{mili.ToString("00")}";
    }

    private void Update()
    {
        if (car == null) car = FindObjectOfType<Car>();
        if (car == null) return;

        speed.text = (3.6f * car.RB.velocity.magnitude).ToString("F0") + " km/h";
        gear.text = car.inputData.gear == 0 ? "R" : car.inputData.gear.ToString();
        gearRatio.value = car.GetGearRatio();

        if (gearRatio.value > 0.9f) gearRatioImage.color = Color.red;
        else if (gearRatio.value > 0.75f) gearRatioImage.color = Color.yellow;
        else gearRatioImage.color = Color.white;

        bestTime.text = FormatTime(LapManager.Instance.GetBestTime(car));
        currentTime.text = FormatTime(LapManager.Instance.GetRunningTime(car));

        acceleration.value = car.inputData.accelerate;
        brake.value = car.inputData.brake;
        steer.value = (1 + car.inputData.steer) / 2;

        for (int i = 0; i < positions.Length; i++)
        {
            if (i >= RaceManager.Instance.cars.Count)
            {
                positions[i].text = "";
            }
            else
            {
                positions[i].text = (i + 1) + ": " + RaceManager.Instance.cars[i].name;
            }
        }
    }
}