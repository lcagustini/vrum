using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RaceManager : SingletonMonoBehaviour<RaceManager>
{
    public List<Car> cars;

    private async void Start()
    {
        TrackAsset track = AssetContainer.Instance.trackAssets.FirstOrDefault(a => a.assetID == SceneLoader.Instance.playData.trackAssetID) ?? AssetContainer.Instance.trackAssets[0];
        await SpawnTrack(track);

        cars = new List<Car>();

        CarAsset car = AssetContainer.Instance.carAssets.FirstOrDefault(a => a.assetID == SceneLoader.Instance.playData.carAssetID) ?? AssetContainer.Instance.carAssets[0];
        cars.Add(await SpawnPlayerCar(car));
        cars[cars.Count - 1].name = "Player";

        while (LapManager.Instance.HasGridPointAvailable)
        {
            car = AssetContainer.Instance.carAssets[Random.Range(0, AssetContainer.Instance.carAssets.Length)];
            cars.Add(await SpawnAICar(car));
            cars[cars.Count - 1].name = "AI " + cars.Count;
        }
    }

    private async Task<Car> SpawnAICar(CarAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { AssetContainer.Instance.carMain, AssetContainer.Instance.carAI, asset.carModel, AssetContainer.Instance.carTemplate });

        Car car = AssetContainer.Instance.Instantiate<Car>(AssetContainer.Instance.carMain);
        CarModel model = AssetContainer.Instance.Instantiate<CarModel>(asset.carModel, car.transform);
        CarController controller = AssetContainer.Instance.Instantiate<CarController>(AssetContainer.Instance.carAI, car.transform);

        car.Setup(controller, null, model, asset.carConfig);

        return car;
    }

    private async Task<Car> SpawnPlayerCar(CarAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { AssetContainer.Instance.carMain, AssetContainer.Instance.carController, asset.carModel, AssetContainer.Instance.carTemplate });

        Car car = AssetContainer.Instance.Instantiate<Car>(AssetContainer.Instance.carMain);
        CarModel model = AssetContainer.Instance.Instantiate<CarModel>(asset.carModel, car.transform);
        CarController controller = AssetContainer.Instance.Instantiate<CarController>(AssetContainer.Instance.carController, car.transform);
        CarTemplate template = AssetContainer.Instance.Instantiate<CarTemplate>(AssetContainer.Instance.carTemplate, car.transform);

        car.Setup(controller, template, model, asset.carConfig);

        return car;
    }

    private async Task SpawnTrack(TrackAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { asset.trackModel, asset.trackData });

        AssetContainer.Instance.Instantiate(asset.trackModel);
        AssetContainer.Instance.Instantiate(asset.trackData);
    }

    private int SortRace(Car a, Car b)
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

    private void Update()
    {
        cars.Sort(SortRace);
    }
}
