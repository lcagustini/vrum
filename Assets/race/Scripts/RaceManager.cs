using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    private void Start()
    {
        TrackAsset track = AssetContainer.Instance.trackAssets.FirstOrDefault(a => a.assetID == SceneLoader.Instance.playData.trackAssetID) ?? AssetContainer.Instance.trackAssets[0];
        CarAsset car = AssetContainer.Instance.carAssets.FirstOrDefault(a => a.assetID == SceneLoader.Instance.playData.carAssetID) ?? AssetContainer.Instance.carAssets[0];

        SpawnTrack(track);
        SpawnCar(car);
    }

    private async Task SpawnCar(CarAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { asset.car, asset.carController, asset.carModel, asset.carTemplate });

        Car car = AssetContainer.Instance.Instantiate<Car>(asset.car);
        CarModel model = AssetContainer.Instance.Instantiate<CarModel>(asset.carModel, car.transform);
        CarController controller = AssetContainer.Instance.Instantiate<CarController>(asset.carController, car.transform);
        CarTemplate template = AssetContainer.Instance.Instantiate<CarTemplate>(asset.carTemplate, car.transform);

        car.Setup(controller, template, model, asset.carConfig);
    }

    private async Task SpawnTrack(TrackAsset asset)
    {
        await AssetContainer.Instance.LoadAssets(new AssetReference[] { asset.trackModel, asset.trackData });

        AssetContainer.Instance.Instantiate(asset.trackModel);
        AssetContainer.Instance.Instantiate(asset.trackData);
    }
}
