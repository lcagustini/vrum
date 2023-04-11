using KBCore.Refs;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviourValidated
{
    [SerializeField, Anywhere] private RotateTransform carModelParent;
    [SerializeField, Anywhere] private RotateTransform trackModelParent;

    private CarModel[] models;
    private int modelOnScreen;

    private Transform[] tracks;
    private int trackOnScreen;

    private async void Start()
    {
        List<AssetReference> assetReferences = new List<AssetReference>();
        assetReferences.AddRange(AssetContainer.Instance.carAssets.Select(a => a.carModel));
        assetReferences.AddRange(AssetContainer.Instance.trackAssets.Select(a => a.trackModel));
        await AssetContainer.Instance.LoadAssets(assetReferences);

        models = new CarModel[AssetContainer.Instance.carAssets.Length];

        for (int i = 0; i < AssetContainer.Instance.carAssets.Length; i++)
        {
            CarAsset asset = AssetContainer.Instance.carAssets[i];
            models[i] = AssetContainer.Instance.Instantiate<CarModel>(asset.carModel, carModelParent.transform);
            models[i].gameObject.SetActive(false);
        }

        models[0].gameObject.SetActive(true);
        modelOnScreen = 0;

        tracks = new Transform[AssetContainer.Instance.trackAssets.Length];

        for (int i = 0; i < AssetContainer.Instance.trackAssets.Length; i++)
        {
            TrackAsset asset = AssetContainer.Instance.trackAssets[i];
            tracks[i] = AssetContainer.Instance.Instantiate<Transform>(asset.trackModel, trackModelParent.transform);
            tracks[i].gameObject.SetActive(false);
        }

        tracks[0].gameObject.SetActive(true);
        trackOnScreen = 0;
    }

    public void LeftTrackButtonClick()
    {
        tracks[trackOnScreen].gameObject.SetActive(false);

        if (trackOnScreen == 0) trackOnScreen = tracks.Length - 1;
        else trackOnScreen--;

        tracks[trackOnScreen].gameObject.SetActive(true);
    }

    public void RightTrackButtonClick()
    {
        tracks[trackOnScreen].gameObject.SetActive(false);

        if (trackOnScreen == tracks.Length - 1) trackOnScreen = 0;
        else trackOnScreen++;

        tracks[trackOnScreen].gameObject.SetActive(true);
    }

    public void LeftCarButtonClick()
    {
        models[modelOnScreen].gameObject.SetActive(false);

        if (modelOnScreen == 0) modelOnScreen = models.Length - 1;
        else modelOnScreen--;

        models[modelOnScreen].gameObject.SetActive(true);
    }

    public void RightCarButtonClick()
    {
        models[modelOnScreen].gameObject.SetActive(false);

        if (modelOnScreen == models.Length - 1) modelOnScreen = 0;
        else modelOnScreen++;

        models[modelOnScreen].gameObject.SetActive(true);
    }

    public void GoButtonClick()
    {
        SceneLoader.Instance.playData.carAssetID = AssetContainer.Instance.carAssets[modelOnScreen].assetID;
        SceneLoader.Instance.playData.trackAssetID = AssetContainer.Instance.trackAssets[trackOnScreen].assetID;
        SceneLoader.Instance.LoadScene("Race");
    }
}
