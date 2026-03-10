using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;

    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerReady;
    //private float waitingToStartTimer = 2f;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 300f;
    private bool isGamePaused = false;

    private Dictionary<ulong, bool> playerReadyDictionary;

    private void Awake()
    {
        state.Value = State.WaitingToStart;
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
    }
    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
    }
    private void State_OnValueChanged(State _, State newState)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        // Game start
        if (state.Value == State.WaitingToStart)
        {
            isLocalPlayerReady = true;
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);

            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictionary.ContainsKey(clientId) || !playerReadyDictionary[clientId])
            {
                allClientsReady = false;
            }
        }
        if (allClientsReady)
        {
            state.Value = State.CountdownToStart;
        }
    }
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        if (!IsServer)
        {
            return;
        }
        //print(state);
        //if (state == State.WaitingToStart)
        //{
        //    waitingToStartTimer -= Time.deltaTime;
        //    if (waitingToStartTimer < 0f)
        //    {
        //        state = State.CountdownToStart;
        //        OnStateChanged?.Invoke(this, EventArgs.Empty);
        //    }
        //}
        if (state.Value == State.CountdownToStart)
        {
            countdownToStartTimer.Value -= Time.deltaTime;
            if (countdownToStartTimer.Value < 0f)
            {
                state.Value = State.GamePlaying;
                gamePlayingTimer.Value = gamePlayingTimerMax;
                //OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (state.Value == State.GamePlaying)
        {
            gamePlayingTimer.Value -= Time.deltaTime;
            if (gamePlayingTimer.Value < 0f)
            {
                state.Value = State.GameOver;
                //OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public bool IsGamePlaying()
    {
        return state.Value == State.GamePlaying;
    }
    public bool IsCountdownToStartActive()
    {
        return state.Value == State.CountdownToStart;
    }
    public bool IsGameOver()
    {
        return state.Value == State.GameOver;
    }
    public bool IsLocalPlayerReady()
    {
        return isLocalPlayerReady;
    }

    public float GetCountdownToStartTimer() { return countdownToStartTimer.Value; }
    public float GetPlayingTimerNormalized() { return gamePlayingTimer.Value / gamePlayingTimerMax; }

    public void TogglePauseGame()
    {
        isGamePaused = !isGamePaused;
        if (!isGamePaused)
        {
            Time.timeScale = 1f;
            OnGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 0f;
            OnGamePaused?.Invoke(this, EventArgs.Empty);
        }
    }
}
