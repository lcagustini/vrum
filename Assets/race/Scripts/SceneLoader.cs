using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneLoader : PersistentSingletonMonobehaviour<SceneLoader>
{
    public struct PlayData
    {
        public string carAssetID;
        public string trackAssetID;
    }

    public PlayData playData;

    public async void LoadScene(string name, int delay = -1)
    {
        if (delay > 0) await Task.Delay(delay);
        destroyCancellationToken.ThrowIfCancellationRequested();

        AsyncOperation op = SceneManager.LoadSceneAsync(name);

        while (!op.isDone) await Task.Delay(100);
        destroyCancellationToken.ThrowIfCancellationRequested();
    }
}
