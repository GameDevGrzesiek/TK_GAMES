using Assets.Scripts;
using Assets.Scripts.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MyLevelManager : Singleton<MyLevelManager>
{
    public bool DoneLoading { get; private set; } = true;

    private string m_levelName = "";

    public override void Awake()
    {
        base.Awake();
        DoneLoading = true;
        GameEvents.GameStateEvents.FinishedLoading += StartLevel;
    }

    private void OnDestroy()
    {
        GameEvents.GameStateEvents.FinishedLoading -= StartLevel;
    }

    public void SetupLevel(string levelName = "")
    {
        m_levelName = levelName;
    }

    public void LoadLevelData()
    {
        StartCoroutine(DelayedLevelLoad());
    }

    // LOAD EVERYTHING NEEDED HERE
    IEnumerator DelayedLevelLoad()
    {
        DoneLoading = false;

        // IF WE HAVE ADDITIONAL SCENE ASSET HEREs
        if (!string.IsNullOrEmpty(m_levelName))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(m_levelName, LoadSceneMode.Additive);
            asyncLoad.allowSceneActivation = true;

            while (!asyncLoad.isDone)
                yield return null;
        }

        // SLOW STUFF HERE
        yield return new WaitForSeconds(5);

        DoneLoading = true;
    }

    private void StartLevel()
    {
        // LAST INIT FOR GAMEPLAY STUFF
    }
}
