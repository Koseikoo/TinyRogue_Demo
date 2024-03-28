using System;
using Factory;
using Game;
using UniRx;
using UnityEngine;
using Zenject;

public class GameInitializer : MonoBehaviour
{
    [Inject] private ModalFactory _modalFactory;

    [Inject] private GameSetup _gameSetup;
    [Inject] private TurnManager _turnManager;
    [Inject] private IntuitiveInputManager _inputManager;
    [Inject] private PlayerManager _playerManager;
    [Inject] private PlayerFeedbackManager _playerFeedbackManager;
    [Inject] private GameAreaManager _gameAreaManager;

    [Space]
    [SerializeField] private PlayerDefinition _playerDefinition;
    [SerializeField] private WeaponDefinition _weaponDefinition;

    private IDisposable _cameraFollowSubscription;
    private IDisposable _cameraShakeSubscription;
    private bool _injected;
    
    [Inject]
    private void PostInject()
    {
        _gameSetup.Setup();
        GameStateSubscriptions();
        
        GameStateContainer.GameState.Value = GameState.CharacterCreation;
        _turnManager.StartTurn(this);
        _injected = true;
    }
    
    private void Update()
    {
        if(!_injected || _playerManager.Player == null)
            return;

        _inputManager.ProcessInput();
        _playerFeedbackManager.UpdateTileSelection();
    }
    
    private void GameStateSubscriptions()
    {
        
        
        GameStateContainer.GameState.Where(state => state == GameState.Dead).Subscribe(_ =>
        {
            _modalFactory.CreateDeathModal();
    
        }).AddTo(this);
            
        GameStateContainer.GameState
            .Pairwise()
            .Where(pair => pair.Previous == GameState.CharacterCreation && pair.Current == GameState.Ship)
            .Subscribe(_ =>
            {
                _playerManager.SpawnPlayerWithWeapon(_weaponDefinition, _playerDefinition);
                _gameAreaManager.SpawnNewShip();
                
            }).AddTo(this);
            
        GameStateContainer.GameState
            .Pairwise()
            .Where(pair => pair.Previous == GameState.Island && pair.Current == GameState.Ship)
            .Subscribe(_ =>
            {
                _gameAreaManager.SpawnNewShip();
            }).AddTo(this);
            
        GameStateContainer.GameState.Where(state => state == GameState.Island).Subscribe(_ =>
        {
            _gameAreaManager.SpawnNewIsland();
        }).AddTo(this);
            
        GameStateContainer.GameState.Where(state => state == GameState.CharacterCreation).Subscribe(_ =>
        {
            _cameraFollowSubscription?.Dispose();
            _cameraShakeSubscription?.Dispose();
            
            _gameAreaManager.DestroyIsland();
            _gameAreaManager.DestroyShip();
            _playerManager.DestroyPlayer();
            _modalFactory.CreateCharacterCreationModal();
        }).AddTo(this);
    }

    private void OnDrawGizmos()
    {
        if(_playerFeedbackManager == null)
            return;
        Gizmos.DrawLine(_playerFeedbackManager.WorldPosition, _playerFeedbackManager.WorldPosition + _playerFeedbackManager.WorldSwipeVector);
        
        if(_playerManager == null || _playerManager.Player == null)
            return;

        for (int i = 0; i < _playerManager.Player.SelectedTiles.Count; i++)
        {
            Gizmos.DrawSphere(_playerManager.Player.SelectedTiles[i].FlatPosition, .1f + ((float)i / _playerManager.Player.SelectedTiles.Count));
        }
    }
}