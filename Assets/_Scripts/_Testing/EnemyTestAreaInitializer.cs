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
using Unit = Models.Unit;

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
        [Inject] private IntuitiveInputManager _inputManager;
        [Inject] private SegmentContainer _segmentContainer;

        [Inject] private DiContainer _container;
        
        [Space]
        [SerializeField] private PlayerDefinition _playerDefinition;
        [SerializeField] private WeaponDefinition _weaponDefinition;

        public bool SpawnNewIsland;
        
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
            CreatePlayer();
            _gameAreaManager.SpawnTestIsland(8);
        
            GameStateContainer.GameState.Value = GameState.Island;
            _turnManager.StartTurn(this);


            _enemySpawnTile = _gameAreaManager.Island.Segments[2].CenterTile;
            _segmentSpawnTile = _gameAreaManager.Island.Segments[2].CenterTile;
        }
        
        private void Update()
        {
            if (SpawnNewIsland)
            {
                SpawnNewIsland = false;
                _gameAreaManager.SpawnNewIsland();
            }
            SpawnUnit();
            SpawnSegment();

            if (destroySegment)
            {
                destroySegment = false;
                DestroySegment();
            }
            
            if(_playerManager.Player == null)
                return;
        
            _inputManager.ProcessInput();
        
            _playerFeedbackManager.UpdateTileSelection();
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
                foreach (Unit unit in _currentSegment.Units)
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
            
            var tiles = _gameAreaManager.Island.Tiles.GetSegmentTiles(segment);
            segment.SetTiles(tiles);
            _segmentFactory.CreateSegmentView(segment);
            return segment;
        }
    }
}