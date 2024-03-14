using System.Collections.Generic;
using Models;
using UniRx;
using UnityEngine;

public enum GameState
{
    None,
    CharacterCreation,
    Ship,
    Island,
    Dead
}

public enum TurnState
{
    Disabled,
    PlayerTurnStart,
    PlayerTurnEnd,
    EnemyTurn,
    IslandTurn,
}

public static class GameStateContainer
{
    public static ReactiveProperty<GameState> GameState = new();
    public static ReactiveProperty<TurnState> TurnState = new();

    public static Player Player;
    public static Slot SelectedSlot;
    public static ReactiveCommand CloseOpenUIElements = new();
    public static List<GameObject> OpenUIElements = new();
    public static bool OpenUI => OpenUIElements.Count > 0;
    public static bool LockCameraRotation;
}