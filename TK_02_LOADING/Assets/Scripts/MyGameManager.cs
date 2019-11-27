using Assets.Scripts;
using Assets.Scripts.Utilities;
using UnityEngine;

public class MyGameManager : Singleton<MyGameManager>
{
    private GlobalGameState m_previousGameState;

    public GlobalGameState GameState { get; private set; }
    public GlobalGameState FutureGameState { get; private set; }

    void Start ()
    {
        m_previousGameState = GameState = GlobalGameState.Starting;
        FutureGameState = GlobalGameState.None;

        GameEvents.GameStateEvents.StartedLoading += StartLoadingState;
        GameEvents.GameStateEvents.FinishedLoading += FinishLoadingState;
        
        MySceneManager.Instance.LoadScene(MySettingsManager.MainMenu, GlobalGameState.MainMenu);
    }

    private void OnDestroy()
    {
        GameEvents.GameStateEvents.StartedLoading -= StartLoadingState;
        GameEvents.GameStateEvents.FinishedLoading -= FinishLoadingState;
    }

    void Update () {}

    public void RunInitialLoading()
    {
        if (GameState != GlobalGameState.Starting)
            return;

        GameState = GlobalGameState.Loading;
    }

    private void StartLoadingState()
    {
        GameState = GlobalGameState.Loading;
    }

    public void SetFutureGameState(GlobalGameState nextGameState)
    {
        FutureGameState = nextGameState;
    }

    private void FinishLoadingState()
    {
        if (FutureGameState == GlobalGameState.None)
        {
            Debug.LogError("Next game state cannot be None!");
            return;
        }

        GameState = FutureGameState;
        FutureGameState = GlobalGameState.None;
    }
}
