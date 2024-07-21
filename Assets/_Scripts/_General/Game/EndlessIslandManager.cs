using System.Collections.Generic;
using System.Linq;
using Container;
using Factories;
using Models;
using UniRx;
using UnityEngine;
using Views;
using Zenject;

namespace Game
{
    public class EndlessIslandManager
    {
        [Inject] private IslandFactory _islandFactory;
        [Inject] private UnitFactory _unitFactory;
        [Inject] private CameraFactory _cameraFactory;
        
        [Inject] private UnitContainer _unitContainer;
        
        [Inject] private PlayerManager _playerManager;
        //[Inject] private PlayerFeedbackManager _playerFeedbackManager;
        
        private Island _island;

        private List<Enemy> currentEnemies = new();

        public void SpawnIsland(PlayerDefinition playerDefinition)
        {
            _playerManager.SpawnPlayerWithWeapon(null, playerDefinition);
            _island = _islandFactory.CreateEndlessIsland();
            _playerManager.Player.Tile.Value = _island.Tiles.Random();
            
            SpawnEnemyWave();
        }
        
        public void SpawnEnemyWave()
        {
            for (int i = 0; i < 4; i++)
            {
                Tile tile = _island.Tiles.Where(tile => !tile.HasUnit).ToList().Random();
                EnemyDefinition enemyDefinition = _unitContainer.GetEnemyDefinition(UnitType.SpiderEnemy);
                Enemy enemy = _unitFactory.CreateEnemy(enemyDefinition, tile);
                currentEnemies.Add(enemy);

                enemy.IsDead
                    .Where(val => val)
                    .Subscribe(val =>
                    {
                        currentEnemies.Remove(enemy);

                        if (currentEnemies.Count == 0)
                        {
                            SpawnEnemyWave();
                        }
                    });
            }
            
        }
    }
}