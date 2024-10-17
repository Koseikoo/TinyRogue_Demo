using System;
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
        private static System.Random _random = new();
        
        [Inject] private IslandFactory _islandFactory;
        [Inject] private IslandViewFactory _islandViewFactory;
        [Inject] private UnitFactory _unitFactory;
        [Inject] private CameraFactory _cameraFactory;
        
        [Inject] private UnitContainer _unitContainer;
        
        [Inject] private PlayerManager _playerManager;
        //[Inject] private PlayerFeedbackManager _playerFeedbackManager;

        private int currentWave;
        private int maxWaves;
        private Island _island;
        private List<Enemy> currentEnemies = new();

        private UnitType[] spawnableUnits = new[]
        {
            UnitType.SpecterEnemy,
            UnitType.MushroomEnemy,
            UnitType.RatEnemy,
            UnitType.OrcEnemy,
            UnitType.WolfEnemy,
            UnitType.SpecterEnemy
        };

        public void SpawnIsland(int waves)
        {
            _island = _islandFactory.GetIsland();
            _playerManager.Player.Tile.Value = _island.Tiles.Random();
            _island.StartTile.AddMoveToLogic(unit =>
            {
                if (!currentEnemies.Any())
                {
                    DestroyIsland();
                    SpawnIsland(maxWaves);
                }
            });

            currentWave = 0;
            maxWaves = waves;
            
            SpawnForrest();
            SpawnEnemyWave();
            IslandView view = _islandViewFactory.CreateIslandView(_island);
        }

        public void DestroyIsland()
        {
            foreach (GameUnit unit in _island.Units)
            {
                unit.IsDestroyed.Value = true;
            }
            
            _island.IsDestroyed.Value = true;
            _islandViewFactory.ResetPooledTiles();
        }
        
        private void SpawnForrest()
        {
            Tile centerTile = _island.Tiles.Where(t => !t.HasUnit).ToList().Random();
            UnitDefinition treeDefinition = _unitContainer.GetUnitDefinition(UnitType.CenterTree);
            Models.GameUnit centerTree = _unitFactory.CreateUnit(treeDefinition, centerTile);

            List<Tile> tileNeighbours = centerTile.Neighbours;
            tileNeighbours = tileNeighbours.PickRandomUniqueCollection(_random.Next(2, tileNeighbours.Count))
                .Where(t => !t.HasUnit).ToList();

            foreach (Tile neighbourTile in tileNeighbours)
            {
                treeDefinition = _unitContainer.GetUnitDefinition(UnitType.Tree);
                Models.GameUnit tree = _unitFactory.CreateUnit(treeDefinition, neighbourTile);
            }

            tileNeighbours = tileNeighbours.Where(t => !t.HasUnit).ToList();

            if (tileNeighbours.Count > 0)
            {
                Tile extraTile = tileNeighbours.Random();
                treeDefinition = _unitContainer.GetUnitDefinition(UnitType.Tree);
                Models.GameUnit extraTree = _unitFactory.CreateUnit(treeDefinition, extraTile);
            }
        }
        
        private void SpawnEnemyWave()
        {
            currentWave++;
            Debug.Log($"Current Wave: {currentWave} / {maxWaves}");
            
            for (int i = 0; i < 3; i++)
            {
                Tile tile = _island.Tiles.Where(tile => !tile.HasUnit).ToList().Random();
                EnemyDefinition enemyDefinition = _unitContainer.GetEnemyDefinition(spawnableUnits.Random());
                Enemy enemy = _unitFactory.CreateEnemy(enemyDefinition, tile);
                currentEnemies.Add(enemy);

                enemy.IsDead
                    .Where(val => val)
                    .Subscribe(val =>
                    {
                        currentEnemies.Remove(enemy);

                        if (currentEnemies.Count == 0 && currentWave < maxWaves)
                        {
                            SpawnEnemyWave();
                        }
                    });
            }
        }
    }
}