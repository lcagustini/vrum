using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RaceManager : SingletonMonoBehaviour<RaceManager>
{
    class SortRace : IComparer<Car>
    {
        public int Compare(Car a, Car b)
        {
            int lapA = LapManager.Instance.GetLap(a);
            int lapB = LapManager.Instance.GetLap(b);

            if (lapA != lapB) return lapA > lapB ? -1 : 1;

            int checkpointA = LapManager.Instance.GetCheckpoint(a).order;
            int checkpointB = LapManager.Instance.GetCheckpoint(b).order;

            if (checkpointA == checkpointB)
            {
                CheckpointCollider checkpoint = LapManager.Instance.GetNextCheckpoint(a);
                float distanceA = (checkpoint.transform.position - a.transform.position).magnitude;
                float distanceB = (checkpoint.transform.position - b.transform.position).magnitude;

                return distanceA < distanceB ? -1 : 1;
            }
            else
            {
                return checkpointA > checkpointB ? -1 : 1;
            }
        }
    }

    public List<Car> racingCars;
    public int firstRacingCar;

    public bool RaceRunning { get; private set; }
    public bool RaceStarting { get; private set; }
    public bool RaceEnded { get; private set; }

    private readonly SortRace raceSorter = new SortRace();

    private async void Start()
    {
        RaceStarting = true;

        TrackAsset track = AssetContainer.Instance.trackAssets.FirstOrDefault(a => a.assetID == SceneLoader.Instance.playData.trackAssetID) ?? AssetContainer.Instance.trackAssets[0];
        await SpawnTrack(track);

        MinimapCamera.Instance.Setup();

        racingCars = new List<Car>();
        firstRacingCar = 0;

#if AI_TEST
        for (int i = 0; i < 20; i++)
        {
            CarAsset car = AssetContainer.Instance.carAssets[0];
            racingCars.Add(await SpawnMLCar(car));
            racingCars[racingCars.Count - 1].name = "ML " + racingCars.Count;
        }
#else
        CarAsset car = AssetContainer.Instance.carAssets.FirstOrDefault(a => a.assetID == SceneLoader.Instance.playData.carAssetID) ?? AssetContainer.Instance.carAssets[0];
        racingCars.Add(await SpawnPlayerCar(car));
        racingCars[racingCars.Count - 1].name = "Player";

        while (LapManager.Instance.HasGridPointAvailable)
        {
            car = AssetContainer.Instance.carAssets[Random.Range(0, AssetContainer.Instance.carAssets.Length)];
            racingCars.Add(await SpawnAICar(car));
            racingCars[racingCars.Count - 1].name = "AI " + racingCars.Count;
        }

        await Task.Delay(5000);
#endif

        foreach (Car racingCar in racingCars)
        {
            LapManager.Instance.checkpointTracker.Add(racingCar, new LapManager.LapTracker(Time.timeSinceLevelLoad, 1, 0));

            float gearRatio = racingCar.GetGearRatio();
            if (gearRatio > 0.6f && gearRatio < 0.7f)
            {
                racingCar.inputData.rocketStart = racingCar.config.rocketStartLength;
            }

            racingCar.inputData.gear = 1;
        }

        RaceStarting = false;
        RaceRunning = true;
    }

    private async Task<Car> SpawnMLCar(CarAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { AssetContainer.Instance.carMain, AssetContainer.Instance.carML, asset.carModel });

        Car car = AssetContainer.Instance.Instantiate<Car>(AssetContainer.Instance.carMain);
        CarModel model = AssetContainer.Instance.Instantiate<CarModel>(asset.carModel, car.transform);
        ICarController controller = AssetContainer.Instance.Instantiate<ICarController>(AssetContainer.Instance.carML, car.transform);

        car.CarSetup(controller, model, asset.carConfig);
        car.automaticTransmission = true;
        car.PlaceInStartingGrid();

        return car;
    }

    private async Task<Car> SpawnAICar(CarAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { AssetContainer.Instance.carMain, AssetContainer.Instance.carAI, asset.carModel });

        Car car = AssetContainer.Instance.Instantiate<Car>(AssetContainer.Instance.carMain);
        CarModel model = AssetContainer.Instance.Instantiate<CarModel>(asset.carModel, car.transform);
        ICarController controller = AssetContainer.Instance.Instantiate<ICarController>(AssetContainer.Instance.carAI, car.transform);

        car.CarSetup(controller, model, asset.carConfig);
        car.automaticTransmission = true;
        car.PlaceInStartingGrid();

        return car;
    }

    private async Task<Car> SpawnPlayerCar(CarAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { AssetContainer.Instance.carMain, AssetContainer.Instance.carController, asset.carModel });

        Car car = AssetContainer.Instance.Instantiate<Car>(AssetContainer.Instance.carMain);
        CarModel model = AssetContainer.Instance.Instantiate<CarModel>(asset.carModel, car.transform);
        ICarController controller = AssetContainer.Instance.Instantiate<ICarController>(AssetContainer.Instance.carController, car.transform);

        car.CarSetup(controller, model, asset.carConfig);
        car.PlaceInStartingGrid();

        return car;
    }

    public async void ConvertPlayerCarToAI(Car car)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { AssetContainer.Instance.carAI });

        Destroy(car.controller.GameObject);

        ICarController controller = AssetContainer.Instance.Instantiate<ICarController>(AssetContainer.Instance.carAI, car.transform);

        car.CarSetup(controller, car.model, car.config);
        car.automaticTransmission = true;
    }

    private async Task SpawnTrack(TrackAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { asset.trackModel, asset.trackData });

        AssetContainer.Instance.Instantiate(asset.trackModel);
        AssetContainer.Instance.Instantiate(asset.trackData);
    }

    private void Update()
    {
        if (RaceRunning)
        {
            racingCars.Sort(firstRacingCar, racingCars.Count - firstRacingCar, raceSorter);

            if (firstRacingCar == racingCars.Count)
            {
                RaceRunning = false;
                RaceEnded = true;

                SceneLoader.Instance.LoadScene("Menu", 5000);
            }
        }
    }
}
