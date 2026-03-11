using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnMultiplayerPaused;
    public event EventHandler OnMultiplayerUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;

    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private Transform playerPrefab;

    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToStart);
    private bool isLocalPlayerReady;
    //private float waitingToStartTimer = 2f;
    private NetworkVariable<float> countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> gamePlayingTimer = new NetworkVariable<float>(0f);
    private float gamePlayingTimerMax = 300f;
    private bool isLocalGamePaused = false;
    private NetworkVariable<bool> isGamePause = new NetworkVariable<bool>(false);

    private Dictionary<ulong, bool> playerReadyDictionary; // 只在Server/Host那是全的，所有Client发现这里是空的
    private Dictionary<ulong, bool> playerPauseDictionary; // 只在Server/Host那是全的，所有Client发现这里是空的
    private bool autoTestGamePauseState;

    private void Awake()
    {
        state.Value = State.WaitingToStart;
        Instance = this;
        playerReadyDictionary = new Dictionary<ulong, bool>();
        playerPauseDictionary = new Dictionary<ulong, bool>();
    }
    public override void OnNetworkSpawn()
    {
        state.OnValueChanged += State_OnValueChanged;
        isGamePause.OnValueChanged += IsGamePause_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTranfsorm = Instantiate(playerPrefab);
            playerTranfsorm.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, destroyWithScene: true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        // 不直接调用TestGamePauseState()是因为在这个回调当中，是因为这个回调执行在玩家退出失去连接之前
        autoTestGamePauseState = true;
    }

    private void State_OnValueChanged(State _, State newState)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
    private void IsGamePause_OnValueChanged(bool _, bool newValue)
    {
        if (isGamePause.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerPaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerUnpaused?.Invoke(this, EventArgs.Empty);
        }
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
    private void LateUpdate()
    {
        if (autoTestGamePauseState)
        {
            autoTestGamePauseState = false;
            TestGamePauseState();
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
    public bool IsWaitingToStart()
    {
        return state.Value == State.WaitingToStart;
    }

    public float GetCountdownToStartTimer() { return countdownToStartTimer.Value; }
    public float GetPlayingTimerNormalized() { return gamePlayingTimer.Value / gamePlayingTimerMax; }

    public void TogglePauseGame()
    {
        isLocalGamePaused = !isLocalGamePaused;
        if (!isLocalGamePaused)
        {
            UnpauseGameServerRpc();
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            PauseGameServerRpc();
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = true;
        TestGamePauseState();
    }
    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerPauseDictionary[serverRpcParams.Receive.SenderClientId] = false;
        TestGamePauseState();
    }
    private void TestGamePauseState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPauseDictionary.ContainsKey(clientId) && playerPauseDictionary[clientId]) // someone pause the game
            {
                isGamePause.Value = true;
                return;
            }
        }
        // all players are unpaused
        isGamePause.Value = false;
    }
}
