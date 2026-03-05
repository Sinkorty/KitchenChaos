using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnGamePaused;
    public event EventHandler OnGameUnpaused;

    public enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }
    private State state;
    private float waitingToStartTimer = 2f;
    private float countdownToStartTimer = 3f;
    private float gamePlayingTimer;
    private float gamePlayingTimerMax = 20f;

    private bool isGamePaused = false;

    private void Awake()
    {
        state = State.WaitingToStart;
        Instance = this;
    }
    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
    }

    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }

    private void Update()
    {
        //print(state);
        if (state == State.WaitingToStart)
        {
            waitingToStartTimer -= Time.deltaTime;
            if (waitingToStartTimer < 0f)
            {
                state = State.CountdownToStart;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (state == State.CountdownToStart)
        {
            countdownToStartTimer -= Time.deltaTime;
            if (countdownToStartTimer < 0f)
            {
                state = State.GamePlaying;
                gamePlayingTimer = gamePlayingTimerMax;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (state == State.GamePlaying)
        {
            gamePlayingTimer -= Time.deltaTime;
            if (gamePlayingTimer < 0f)
            {
                state = State.GameOver;
                OnStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public bool IsGamePlaying()
    {
        return state == State.GamePlaying;
    }
    public bool IsCountdownToStartActive()
    {
        return state == State.CountdownToStart;
    }
    public bool IsGameOver()
    {
        return state == State.GameOver;
    }
    public float GetCountdownToStartTimer() { return countdownToStartTimer; }
    public float GetPlayingTimerNormalized() { return gamePlayingTimer / gamePlayingTimerMax; }

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
