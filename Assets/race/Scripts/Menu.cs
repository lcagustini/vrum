using KBCore.Refs;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Menu : MonoBehaviourValidated
{
    [SerializeField, Self] private UIDocument ui;
    [SerializeField, Anywhere] private RotateTransform modelParent;

    private CarModel[] models;
    private int modelOnScreen;

    private void UISetup()
    {
        ui.rootVisualElement.Q<Button>("LeftButton").clicked += () =>
        {
            models[modelOnScreen].gameObject.SetActive(false);

            if (modelOnScreen == 0) modelOnScreen = models.Length - 1;
            else modelOnScreen--;

            models[modelOnScreen].gameObject.SetActive(true);
        };

        ui.rootVisualElement.Q<Button>("RightButton").clicked += () =>
        {
            models[modelOnScreen].gameObject.SetActive(false);

            if (modelOnScreen == models.Length - 1) modelOnScreen = 0;
            else modelOnScreen++;

            models[modelOnScreen].gameObject.SetActive(true);
        };

        ui.rootVisualElement.Q<Button>("GoButton").clicked += () =>
        {
            SceneLoader.Instance.playData.carAssetID = AssetContainer.Instance.carAssets[modelOnScreen].assetID;
            SceneLoader.Instance.LoadScene("Race");
        };
    }

    private async void Start()
    {
        await AssetContainer.Instance.LoadAssets(AssetContainer.Instance.carAssets.Select(a => a.carModel));

        models = new CarModel[AssetContainer.Instance.carAssets.Length];

        for (int i = 0; i < AssetContainer.Instance.carAssets.Length; i++)
        {
            CarAsset asset = AssetContainer.Instance.carAssets[i];
            models[i] = AssetContainer.Instance.Instantiate<CarModel>(asset.carModel, modelParent.transform);
            models[i].gameObject.SetActive(false);
        }

        models[0].gameObject.SetActive(true);
        modelOnScreen = 0;

        UISetup();
    }
}
