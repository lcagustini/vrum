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
    [SerializeField, Anywhere] private TextMeshProUGUI lap;

    [SerializeField, Anywhere] private RectTransform raceStarting;
    [SerializeField, Anywhere] private RectTransform raceEnded;
    [SerializeField, Anywhere] private RectTransform wrongWay;

    [SerializeField, Anywhere] private Image drift;
    [SerializeField, Anywhere] private Image rocket;
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
        if (car == null && RaceManager.Instance.racingCars.Count > 0) car = RaceManager.Instance.racingCars[0];
        if (car == null) return;

        speed.text = (3.6f * car.RB.velocity.magnitude).ToString("F0") + " km/h";
        gear.text = car.inputData.gear == -1 ? "R" : (car.inputData.gear == 0 ? "N" : car.inputData.gear.ToString());
        gearRatio.value = car.GetGearRatio();

        if (gearRatio.value > 0.9f) gearRatioImage.color = Color.red;
        else if (gearRatio.value > 0.75f) gearRatioImage.color = Color.yellow;
        else gearRatioImage.color = Color.white;

        bestTime.text = FormatTime(LapManager.Instance.GetBestTime(car));
        currentTime.text = FormatTime(LapManager.Instance.GetRunningTime(car));

        drift.color = car.inputData.drift > 0 ? Color.yellow : Color.black;
        rocket.color = car.inputData.rocketStart > 0 ? Color.yellow : Color.black;
        acceleration.value = car.inputData.accelerate;
        brake.value = car.inputData.brake;
        steer.value = (1 + car.inputData.steer) / 2;

        for (int i = 0; i < positions.Length; i++)
        {
            if (i >= RaceManager.Instance.racingCars.Count)
            {
                positions[i].text = "";
            }
            else
            {
                positions[i].text = (i + 1) + ": " + RaceManager.Instance.racingCars[i].name;
            }
        }

        if (RaceManager.Instance.RaceRunning)
        {
            raceEnded.gameObject.SetActive(false);
            raceStarting.gameObject.SetActive(false);
            lap.text = LapManager.Instance.GetLap(car) + " / " + LapManager.Instance.totalLaps;
        }
        else
        {
            if (RaceManager.Instance.RaceStarting)
            {
                raceEnded.gameObject.SetActive(false);
                raceStarting.gameObject.SetActive(true);
            }
            else
            {
                raceStarting.gameObject.SetActive(false);
                raceEnded.gameObject.SetActive(true);
            }
            lap.text = "";
        }

        wrongWay.gameObject.SetActive(false);
    }
}
