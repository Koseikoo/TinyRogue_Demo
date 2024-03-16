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
            
            //var action = _segmentSpawnActions[segment.Type];
            //action?.Invoke(segment);
        }
        
        public SegmentView CreateSegmentView(Segment segment)
        {
            SegmentView prefab = _segmentContainer.GetPrefab(segment.Type);
            SegmentView view = _container.InstantiatePrefab(prefab).GetComponent<SegmentView>();
            view.Initialize(segment);

            for (int i = 0; i < view.SegmentUnitDefinitions.Length; i++)
            {
                SegmentUnitDefinition definition = view.SegmentUnitDefinitions[i];
                Tile tile = segment.Tiles.GetClosestTileFromPosition(definition.Point.position);

                Unit unit = null;

                if (definition.Type.ToString().ToLower().Contains("enemy"))
                {
                    unit =
                        _unitFactory.CreateEnemy(_enemyDefinitionContainer.GetEnemyDefinition(definition.Type), tile);
                }
                else
                {
                    unit = _unitFactory.CreateUnit(
                        _unitDefinitionContainer.GetUnitDefinition(definition.Type), tile);
                }
                
                segment.AddUnit(unit);
            }
            
            return view;
        }
        
        public Segment CreateSegment(SegmentView prefab, Tile center)
        {
            Segment segment = prefab.Type switch
            {
                SegmentType.Forrest => _container.Instantiate<DefeatSegment>(new object[]{prefab, center.WorldPosition}),
                SegmentType.EnemyCamp => _container.Instantiate<DefeatSegment>(new object[]{prefab, center.WorldPosition}),
                SegmentType.Village => _container.Instantiate<DefeatSegment>(new object[]{prefab, center.WorldPosition}),
                SegmentType.Start => _container.Instantiate<Segment>(new object[]{prefab, center.WorldPosition}),
                SegmentType.End => _container.Instantiate<Segment>(new object[]{prefab, center.WorldPosition}),
                SegmentType.Ruin => _container.Instantiate<DefeatSegment>(new object[]{prefab, center.WorldPosition}),
                SegmentType.Boss => _container.Instantiate<DefeatSegment>(new object[]{prefab, center.WorldPosition}),
                _ => throw new ArgumentOutOfRangeException()
            };
            return segment;
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
            segment.AddUnit(boss);
            boss.DeathActions.Add(_unitDeathActionContainer.UnlockEndTileAction);
        }
        
        private void CreateRuinSegment(Segment segment)
        {
            var tiles = segment.Tiles.GetMatchingTiles(tile => !tile.HasUnit);
            
            var definition = _enemyDefinitionContainer.GetEnemyDefinition(UnitType.GolemEnemy);
            Enemy enemy = _unitFactory.CreateEnemy(definition, tiles.PickRandom());
            segment.AddUnit(enemy);
        }
        
        private void CreateEnemySegment(Segment segment)
        {
            var tiles = segment.Tiles.GetMatchingTiles(tile => !tile.HasUnit);

            for (int i = 0; i < 2; i++)
            {
                var tile = tiles.PickRandom();
                tiles.Remove(tile);
                var definition = _enemyDefinitionContainer.GetEnemyDefinition(i % 2 == 0 ? UnitType.Orc : UnitType.WolfEnemy);
                Enemy enemy = _unitFactory.CreateEnemy(definition, tile);
                segment.AddUnit(enemy);
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

            for (int i = 0; i < 2; i++)
            {
                var tile = tiles.PickRandom();
                tiles.Remove(tile);
                
                var definition = _enemyDefinitionContainer.GetEnemyDefinition(enemyPool.PickRandom());
                Unit spider = _unitFactory.CreateEnemy(definition, tile);
                segment.AddUnit(spider);
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

            for (int i = 0; i < 2; i++)
            {
                var tile = tiles.PickRandom();
                tiles.Remove(tile);
                
                var definition = _enemyDefinitionContainer.GetEnemyDefinition(UnitType.Rat);
                Unit rat = _unitFactory.CreateEnemy(definition, tile);
                segment.AddUnit(rat);
            }
        }
    }
}