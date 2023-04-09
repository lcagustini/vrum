using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FullSerializer;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

public class SceneLoader : PersistentSingletonMonobehaviour<SceneLoader>
{
    public struct PlayData
    {
        public string carAssetID;
    }

    public PlayData playData;

    public async void LoadScene(string name)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(name);

        while (!op.isDone) await Task.Delay(100);
    }
}
