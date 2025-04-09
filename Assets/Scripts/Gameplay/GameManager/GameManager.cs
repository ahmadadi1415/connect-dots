using System;
using UnityEngine;

public enum GameState
{
    IDLE, DOTS_SOLVED, GRID_LOADING
}

public class GameManager : MonoBehaviour
{
    [field: SerializeField] public GameState CurrentState { get; private set; } = GameState.IDLE;

    private void OnEnable()
    {
        EventManager.Subscribe<OnGameStateChangedMessage>(OnGameStateChanged);
    }

    private void OnDisable()
    {
        EventManager.Unsubscribe<OnGameStateChangedMessage>(OnGameStateChanged);
    }

    private void OnGameStateChanged(OnGameStateChangedMessage message)
    {
        CurrentState = message.State;
    }

}