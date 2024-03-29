using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AssetContainer : PersistentSingletonMonobehaviour<AssetContainer>
{
    public AssetReference carMain;
    public AssetReference carController;
    public AssetReference carAI;
    public AssetReference carML;
    public CarAsset[] carAssets;

    public TrackAsset[] trackAssets;

    private Dictionary<AssetReference, AsyncOperationHandle<Object>> handles = new Dictionary<AssetReference, AsyncOperationHandle<Object>>();

    public GameObject Instantiate(AssetReference asset)
    {
        GameObject prefab = GetLoadedAsset(asset);
        return Instantiate(prefab);
    }

    public GameObject Instantiate(AssetReference asset, Transform parent)
    {
        GameObject prefab = GetLoadedAsset(asset);
        return Instantiate(prefab, parent);
    }

    public T Instantiate<T>(AssetReference asset)
    {
        GameObject prefab = GetLoadedAsset(asset);
        return Instantiate(prefab).GetComponent<T>();
    }

    public T Instantiate<T>(AssetReference asset, Transform parent)
    {
        GameObject prefab = GetLoadedAsset(asset);
        return Instantiate(prefab, parent).GetComponent<T>();
    }

    public GameObject GetLoadedAsset(AssetReference asset)
    {
        if (handles.ContainsKey(asset) && handles[asset].IsValid()) return handles[asset].Result as GameObject;
        return null;
    }

    public async Task LoadAsset(AssetReference asset)
    {
        if (asset == null) return;

        AsyncOperationHandle<Object> handle;
        if (handles.ContainsKey(asset))
        {
            handle = handles[asset];
        }
        else
        {
            handle = Addressables.LoadAssetAsync<Object>(asset);
            handles.Add(asset, handle);
        }

        while (!handle.IsDone) await Task.Yield();

        if (!handle.IsValid())
        {
            handles.Remove(asset);
            Debug.LogError($"Error loading asset {asset}");
        }
    }

    public async Task LoadAssets(IEnumerable<AssetReference> assets)
    {
        IEnumerable<Task> loadTasks = assets.Select(a => LoadAsset(a));
        await Task.WhenAll(loadTasks);
    }
}
