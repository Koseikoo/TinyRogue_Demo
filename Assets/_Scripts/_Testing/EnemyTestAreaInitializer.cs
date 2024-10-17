using System.Collections.Generic;
using DG.Tweening;
using Factories;
using Game;
using Installer;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using Views;
using Zenject;

namespace _Testing
{
    public class EnemyTestAreaInitializer : MonoBehaviour
    {
        [Inject] private UnitFactory _unitFactory;
        [Inject] private SegmentFactory _segmentFactory;
        
        [Inject] private PlayerManager _playerManager;
        [Inject] private GameAreaManager _gameAreaManager;
        [Inject] private PlayerFeedbackManager _playerFeedbackManager;
        [Inject] private TurnManager _turnManager;
        [Inject] private InputManager _inputManager;
        [Inject] private SegmentContainer _segmentContainer;
        [Inject] private GameSetup _gameSetup;

        [Inject] private DiContainer _container;
        
        [Space]
        [SerializeField] private PlayerDefinition _playerDefinition;
        [SerializeField] private WeaponDefinition _weaponDefinition;

        public bool SpawnNewIsland;
        public bool DissolveIsland;
        
        [Header("Enemy Spawning")]
        
        [SerializeField] private EnemyDefinition enemyToSpawn;
        [SerializeField] private bool spawnEnemy;
        private Tile _enemySpawnTile;
        
        [Header("Segment Spawning")]
        
        [SerializeField] private SegmentType segmentToSpawn;
        [SerializeField] private bool spawnSegment;
        [SerializeField] private bool destroySegment;
        private Tile _segmentSpawnTile;
        private Segment _currentSegment;
        
        [Inject]
        private void PostInject()
        {
            _gameSetup.Setup();
            CreatePlayer();
            _gameAreaManager.SpawnEnemyTestIsland();
        
            GameStateContainer.GameState.Value = GameState.Island;
            _turnManager.StartTurn(this);


            _enemySpawnTile = _gameAreaManager.Island.Tiles.GetClosestTileFromPosition(default);
            _segmentSpawnTile = _gameAreaManager.Island.Tiles.GetClosestTileFromPosition(default);
        }
        
        private void Update()
        {
            if (SpawnNewIsland)
            {
                SpawnNewIsland = false;
                _gameAreaManager.SpawnNewIsland();
            }

            if (DissolveIsland && _gameAreaManager.Island != null)
            {
                DissolveIsland = false;
                _gameAreaManager.Island.DissolveIslandCommand.Execute();
            }
            
            SpawnUnit();
            SpawnSegment();

            if (destroySegment)
            {
                destroySegment = false;
                DestroySegment();
            }
            
            if(_playerManager.Player == null)
            {
                return;
            }

            _inputManager.ProcessInput();
        
        }

        private void CreatePlayer()
        {
            _playerManager.SpawnPlayerWithWeapon(_weaponDefinition, _playerDefinition);
        }

        private void SpawnUnit()
        {
            if (spawnEnemy)
            {
                spawnEnemy = false;

                if (_enemySpawnTile.HasUnit)
                {
                    Debug.Log("<color=#eb4034>Tile is Occupied!</color>");
                    return;
                }
                _unitFactory.CreateEnemy(enemyToSpawn, _enemySpawnTile);
            }
        }
        
        private void SpawnSegment()
        {
            if (spawnSegment)
            {
                spawnSegment = false;
                DestroySegment();

                if (_segmentSpawnTile.HasUnit)
                {
                    Debug.Log("<color=#eb4034>Tile is Occupied!</color>");
                    return;
                }
                
                _currentSegment = CreateSegment(_segmentSpawnTile, _segmentContainer.GetPrefab(segmentToSpawn));
                _gameAreaManager.Island.Segments.Add(_currentSegment);
                
            }
        }

        private void DestroySegment()
        {
            if (_currentSegment != null)
            {
                foreach (GameUnit unit in _currentSegment.Units)
                {
                    unit.Death();
                }
                _currentSegment.IsDestroyed.Value = true;
            }
                
        }
        
        private Segment CreateSegment(Tile centerTile, SegmentView prefab)
        {
            Segment segment = _segmentFactory.CreateSegment(prefab, centerTile);
            segment.CenterTile = centerTile;
            
            List<Tile> tiles = _gameAreaManager.Island.Tiles.GetSegmentTiles(segment);
            segment.SetTiles(tiles);
            _segmentFactory.CreateSegmentView(segment);
            return segment;
        }
    }
}