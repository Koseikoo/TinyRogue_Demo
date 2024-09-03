using System;
using System.Collections.Generic;
using System.Linq;
using Container;
using Factories;
using Models;
using UniRx;
using Zenject;

namespace Game
{
    public class EndlessIslandManager
    {
        private static System.Random _random = new();
        
        [Inject] private IslandFactory _islandFactory;
        [Inject] private UnitFactory _unitFactory;
        [Inject] private CameraFactory _cameraFactory;
        
        [Inject] private UnitContainer _unitContainer;
        
        [Inject] private PlayerManager _playerManager;
        //[Inject] private PlayerFeedbackManager _playerFeedbackManager;
        
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

        public void SpawnIsland(PlayerDefinition playerDefinition)
        {
            _playerManager.SpawnPlayerWithWeapon(null, playerDefinition);
            _island = _islandFactory.CreateEndlessIsland();
            _playerManager.Player.Tile.Value = _island.Tiles.Random();
            
            SpawnForrest();
            SpawnEnemyWave();
        }

        private void SpawnForrest()
        {
            Tile centerTile = _island.Tiles.Where(t => !t.HasUnit).ToList().Random();
            UnitDefinition treeDefinition = _unitContainer.GetUnitDefinition(UnitType.CenterTree);
            Models.Unit centerTree = _unitFactory.CreateUnit(treeDefinition, centerTile);

            List<Tile> tileNeighbours = centerTile.Neighbours;
            tileNeighbours = tileNeighbours.PickRandomUniqueCollection(_random.Next(2, tileNeighbours.Count))
                .Where(t => !t.HasUnit).ToList();

            foreach (Tile neighbourTile in tileNeighbours)
            {
                treeDefinition = _unitContainer.GetUnitDefinition(UnitType.Tree);
                Models.Unit tree = _unitFactory.CreateUnit(treeDefinition, neighbourTile);
            }

            tileNeighbours = tileNeighbours.Where(t => !t.HasUnit).ToList();

            if (tileNeighbours.Count > 0)
            {
                Tile extraTile = tileNeighbours.Random();
                treeDefinition = _unitContainer.GetUnitDefinition(UnitType.Tree);
                Models.Unit extraTree = _unitFactory.CreateUnit(treeDefinition, extraTile);
            }
        }
        
        public void SpawnEnemyWave()
        {
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

                        if (currentEnemies.Count == 0)
                        {
                            SpawnEnemyWave();
                        }
                    });
            }
            
        }
    }
}