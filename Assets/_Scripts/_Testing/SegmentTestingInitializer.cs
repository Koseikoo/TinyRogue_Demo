using System;
using Factories;
using Game;
using Installer;
using Models;
using UnityEditor;
using UnityEngine;
using Views;
using Zenject;

namespace _Testing
{
    public class SegmentTestingInitializer : MonoBehaviour
    {
        [Inject] private SegmentFactory _segmentFactory;
        
        [Inject] private PlayerManager _playerManager;
        [Inject] private GameAreaManager _gameAreaManager;
        [Inject] private PlayerFeedbackManager _playerFeedbackManager;
        [Inject] private TurnManager _turnManager;
        [Inject] private IntuitiveInputManager _inputManager;
        [Inject] private SegmentContainer _segmentContainer;
        
        [Space]
        [SerializeField] private PlayerDefinition _playerDefinition;
        [SerializeField] private WeaponDefinition _weaponDefinition;

        public bool ReSpawnSegment;

        [Header("Segment Spawning")] [SerializeField]
        private GameObject PlacingVisuals;
        [SerializeField] private SegmentView segmentToTest;
        
        [Inject]
        private void PostInject()
        {
            CreatePlayer();
            _gameAreaManager.SpawnSegmentTestIsland(segmentToTest);
        
            GameStateContainer.GameState.Value = GameState.Island;
            _turnManager.StartTurn(this);
        }

        private void Start()
        {
            PlacingVisuals.SetActive(false);
        }
        
        private void Update()
        {
            if(_playerManager.Player == null)
                return;
            
            _inputManager.ProcessInput();
            _playerFeedbackManager.UpdateTileSelection();
            
            if (ReSpawnSegment)
            {
                _gameAreaManager.SpawnNewSegmentTestIsland(segmentToTest);
                ReSpawnSegment = false;
            }
        }

        private void CreatePlayer()
        {
            _playerManager.SpawnPlayerWithWeapon(_weaponDefinition, _playerDefinition);
        }

        private void ReSpawn()
        {
            
        }
    }
}