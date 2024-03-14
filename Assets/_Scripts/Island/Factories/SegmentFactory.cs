using System;
using System.Collections.Generic;
using System.Linq;
using Container;
using Installer;
using Models;
using UnityEngine;
using Views;
using Zenject;
using Random = UnityEngine.Random;

namespace Factories
{
    public class SegmentFactory
    {
        private const float EnemySpawnChance = .3f;
        private const float DestructibleSpawnChance = .4f;
        
        [Inject] private UnitFactory _unitFactory;
        [Inject] private TrapFactory _trapFactory;
        [Inject] private EnemyDefinitionContainer _enemyDefinitionContainer;
        [Inject] private InteractableDefinitionContainer _interactableDefinitionContainer;
        [Inject] private UnitDefinitionContainer _unitDefinitionContainer;
        [Inject] private TrapDefinitionContainer _trapDefinitionContainer;
        [Inject] private UnitDeathActionContainer _unitDeathActionContainer;

        [Inject] private SegmentContainer _segmentContainer;
        [Inject] private DiContainer _container;

        private Dictionary<SegmentType, Action<Segment>> _segmentSpawnActions = new();

        public SegmentFactory()
        {
            _segmentSpawnActions[SegmentType.Start] = CreateStartSegment;
            _segmentSpawnActions[SegmentType.End] = CreateEndSegment;
            _segmentSpawnActions[SegmentType.Forrest] = CreateForrestSegment;
            _segmentSpawnActions[SegmentType.EnemyCamp] = CreateEnemySegment;
            _segmentSpawnActions[SegmentType.Ruin] = CreateRuinSegment;
            _segmentSpawnActions[SegmentType.Village] = CreateVillageSegment;
            _segmentSpawnActions[SegmentType.Boss] = CreateBossSegment;
        }

        public void CreateSegment(Segment segment)
        {
            CreateSegmentView(segment);
            
            var action = _segmentSpawnActions[segment.Type];
            action?.Invoke(segment);
        }

        private void CreateStartSegment(Segment segment)
        {
            
        }

        private void CreateEndSegment(Segment segment)
        {
            var miniBossDefinition = _enemyDefinitionContainer.GetEnemyDefinition(UnitType.FishermanMiniBoss);
            Enemy enemy = _unitFactory.CreateEnemy(miniBossDefinition, segment.Tiles[0].Island.EndTile);
            enemy.DeathActions.Add(_unitDeathActionContainer.UnlockEndTileAction);
        }
        
        private void CreateBossSegment(Segment segment)
        {
            var tile = segment.Tiles.PickRandom();
            var definition = _enemyDefinitionContainer.GetEnemyDefinition(UnitType.WerewolfBoss);
            Unit boss = _unitFactory.CreateEnemy(definition, tile);
            segment.AddUnit(boss, true);
            boss.DeathActions.Add(_unitDeathActionContainer.UnlockEndTileAction);
        }
        
        private void CreateRuinSegment(Segment segment)
        {
            var tiles = segment.Tiles.GetMatchingTiles(tile => !tile.HasUnit);
            
            var definition = _enemyDefinitionContainer.GetEnemyDefinition(UnitType.GolemEnemy);
            Enemy enemy = _unitFactory.CreateEnemy(definition, tiles.PickRandom());
            segment.AddUnit(enemy, true);
        }
        
        private void CreateEnemySegment(Segment segment)
        {
            var tiles = segment.Tiles.GetMatchingTiles(tile => !tile.HasUnit);

            for (int i = 0; i < segment.MaxUnits; i++)
            {
                var tile = tiles.PickRandom();
                tiles.Remove(tile);
                var definition = _enemyDefinitionContainer.GetEnemyDefinition(i % 2 == 0 ? UnitType.Orc : UnitType.WolfEnemy);
                Enemy enemy = _unitFactory.CreateEnemy(definition, tile);
                segment.AddUnit(enemy, true);
            }
        }
        
        private void CreateForrestSegment(Segment segment)
        {
            var tiles = segment.Tiles.GetMatchingTiles(tile => !tile.HasUnit);

            UnitType[] enemyPool =
            {
                UnitType.Mushroom,
                UnitType.SpiderEnemy,
            };

            for (int i = 0; i < segment.MaxUnits; i++)
            {
                var tile = tiles.PickRandom();
                tiles.Remove(tile);
                
                var definition = _enemyDefinitionContainer.GetEnemyDefinition(enemyPool.PickRandom());
                Unit spider = _unitFactory.CreateEnemy(definition, tile);
                segment.AddUnit(spider, true);
            }
            
            for (int i = 0; i < tiles.Count; i++)
            {
                if (Random.value < DestructibleSpawnChance)
                {
                    var definition = _unitDefinitionContainer.GetUnitDefinition(UnitType.TreeDestructible);
                    Unit destructible = _unitFactory.CreateUnit(definition, tiles[i]);
                    segment.AddUnit(destructible);
                }
            }
        }

        private void CreateVillageSegment(Segment segment)
        {
            var tiles = segment.Tiles.GetMatchingTiles(tile => !tile.HasUnit);

            for (int i = 0; i < segment.MaxUnits; i++)
            {
                var tile = tiles.PickRandom();
                tiles.Remove(tile);
                
                var definition = _enemyDefinitionContainer.GetEnemyDefinition(UnitType.Rat);
                Unit rat = _unitFactory.CreateEnemy(definition, tile);
                segment.AddUnit(rat, true);
            }
        }

        private SegmentView CreateSegmentView(Segment segment)
        {
            SegmentView prefab = _segmentContainer.GetSegmentPrefab(segment.Type);
            SegmentView view = _container.InstantiatePrefab(prefab).GetComponent<SegmentView>();
            view.Initialize(segment);

            for (int i = 0; i < view.SegmentUnitDefinitions.Length; i++)
            {
                _unitFactory.CreateSegmentUnit(view.SegmentUnitDefinitions[i], segment);
            }
            
            return view;
        }
    }
}