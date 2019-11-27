using Assets.Scripts;
using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MySceneManager : Singleton<MySceneManager>
{
    public Canvas LoadingScreenCanvas;
    public TextMeshProUGUI TextLoading;
    public Camera LoadingScreenCamera;

    [HideInInspector]
    public Camera DestinedCamera;

	void Start ()
    {
        LoadingScreenCanvas.gameObject.SetActive(false);
    }

    public override void Awake()
    {
        base.Awake();
        GameEvents.GameStateEvents.StartedLoading += StartLoadingState;
        GameEvents.GameStateEvents.FinishedLoading += FinishLoadingState;
    }

    private void OnDestroy()
    {
        GameEvents.GameStateEvents.StartedLoading -= StartLoadingState;
        GameEvents.GameStateEvents.FinishedLoading -= FinishLoadingState;
    }

    public void LoadScene(string sceneName, GlobalGameState gameState, SceneDropMode dropMode = SceneDropMode.None)
    {
        if (MyGameManager.Instance.GameState == GlobalGameState.Loading)
            return;

        StartCoroutine(AsyncLoadScene(sceneName, gameState, dropMode));
    }

    private IEnumerator AsyncLoadScene(string sceneName, GlobalGameState gameState, SceneDropMode dropMode)
    {
        GameEvents.GameStateEvents.StartedLoading.Invoke();
        MyGameManager.instance.SetFutureGameState(gameState);

        List<Scene> scenesToDrop = new List<Scene>();

        switch(dropMode)
        {
            case SceneDropMode.OnlyLast:
                {
                    scenesToDrop.Add(UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1));
                } break;
            case SceneDropMode.TillInit:
                {
                    for (int i = 1; i < UnityEngine.SceneManagement.SceneManager.sceneCount; ++i)
                    {
                        scenesToDrop.Add(UnityEngine.SceneManagement.SceneManager.GetSceneAt(i));
                    }
                } break;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
            yield return null;

        Scene nextScene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(UnityEngine.SceneManagement.SceneManager.sceneCount - 1);
        var rootGameObjects = nextScene.GetRootGameObjects();
        for (int i = 0; i < rootGameObjects.Length; ++i)
        {
            var levelCamera = rootGameObjects[i].GetComponent<Camera>();
            if (levelCamera)
            {
                DestinedCamera = levelCamera;
                RefreshToLoadingScreen();
                break;
            }
        }

        if (dropMode != SceneDropMode.None && scenesToDrop != null)
        {
            for (int i = 0; i < scenesToDrop.Count; ++i)
            {
                AsyncOperation asyncUnload = UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(scenesToDrop[i]);
                while (!asyncUnload.isDone)
                    yield return null;
            }
        }

        MyLevelManager.Instance.LoadLevelData();
        yield return new WaitUntil(() => (MyLevelManager.Instance.DoneLoading));

        GameEvents.GameStateEvents.FinishedLoading.Invoke();
    }

    private void RefreshToLoadingScreen()
    {
        if (DestinedCamera != null)
            DestinedCamera.enabled = false;

        LoadingScreenCanvas.gameObject.SetActive(true);
        LoadingScreenCamera.enabled = true;
    }

    private void RefreshToDestinedCamera()
    {
        LoadingScreenCanvas.gameObject.SetActive(false);
        LoadingScreenCamera.enabled = false;

        if (DestinedCamera != null)
        {
            DestinedCamera.enabled = true;
            Camera.SetupCurrent(DestinedCamera);
        }
    }

    private void StartLoadingState()
    {
        RefreshToLoadingScreen();
        StartCoroutine(LoadingStateAnim());
    }

    private void FinishLoadingState()
    {
        RefreshToDestinedCamera();
        StopCoroutine(LoadingStateAnim());
    }

    IEnumerator LoadingStateAnim()
    {
        while (true)
        {
            TextLoading.alpha = Mathf.PingPong(Time.time, 1);
            yield return null;
        }
    }
}
