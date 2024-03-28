using System;
using System.Collections.Generic;
using System.Linq;
using Container;
using DG.Tweening;
using Installer;
using Models;
using Views;
using Zenject;
using UniRx;
using UnityEngine;
using Unit = Models.Unit;

namespace Factories
{
    public class SegmentFactory
    {
        private const float EnemySpawnChance = .3f;
        private const float DestructibleSpawnChance = .4f;
        
        [Inject] private UnitFactory _unitFactory;
        [Inject] private UnitContainer _unitContainer;
        [Inject] private UnitDeathActionContainer _unitDeathActionContainer;
        [Inject] private ItemContainer _itemContainer;

        [Inject] private WorldShipView _shipView;

        [Inject] private SegmentContainer _segmentContainer;
        [Inject] private DiContainer _container;

        private Dictionary<SegmentType, Action<Segment>> _segmentSpawnActions = new();

        public SegmentFactory()
        {
            _segmentSpawnActions[SegmentType.Start] = CreateStartSegment;
            _segmentSpawnActions[SegmentType.Ruin] = CreateSimpleEnemySegment;
            _segmentSpawnActions[SegmentType.Forrest] = CreateSimpleEnemySegment;
            _segmentSpawnActions[SegmentType.WolfCamp] = CreateWolfCamp;
            _segmentSpawnActions[SegmentType.MiniBoss] = CreateMiniBossSegment;
        }

        public SegmentView CreateSegmentView(Segment segment, SegmentView prefab = null)
        {
            if(prefab == null)
                prefab = _segmentContainer.GetPrefab(segment.Type);
            SegmentView view = _container.InstantiatePrefab(prefab).GetComponent<SegmentView>();
            view.Initialize(segment);
            
            for (int i = 0; i < view.SegmentUnitDefinitions.Length; i++)
            {
                SegmentUnitDefinition definition = view.SegmentUnitDefinitions[i];
                Tile tile = segment.Tiles.GetClosestTileFromPosition(definition.Point.position);

                Unit unit = null;

                if (definition.Type.ToString().ToLower().Contains("enemy") ||
                    definition.Type.ToString().ToLower().Contains("boss"))
                {
                    unit =
                        _unitFactory.CreateEnemy(_unitContainer.GetEnemyDefinition(definition.Type), tile);
                }
                else if (definition.Type.ToString().ToLower().Contains("interact"))
                {
                    unit = _unitFactory.CreateUnit(
                        _unitContainer.GetInteractableDefinition(definition.Type), tile);
                }
                else
                {
                    unit = _unitFactory.CreateUnit(
                        _unitContainer.GetUnitDefinition(definition.Type), tile);
                }
                
                segment.AddUnit(unit);
            }
                                                                          
            if(_segmentSpawnActions.TryGetValue(prefab.Type, out var action))         
                action(segment);                                                      
            
            return view;
        }
        
        public Segment CreateSegment(SegmentView prefab, Tile center)
        {
            Segment segment = prefab.Type switch
            {
                SegmentType.Forrest => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                SegmentType.WolfCamp => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                SegmentType.Village => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                SegmentType.Start => _container.Instantiate<Segment>(new object[]{prefab, center}),
                SegmentType.Ruin => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                SegmentType.MiniBoss => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                SegmentType.Boss => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                _ => throw new ArgumentOutOfRangeException()
            };

            return segment;
        }

        private void CreateWolfCamp(Segment segment)
        {
            var cave = segment.Units.First(unit => unit.Type == UnitType.WolfCave);
            
            Action<Tile> caveDestroyAction = tile =>
            {
                var wolfDefinition = _unitContainer.GetEnemyDefinition(UnitType.BigWolfEnemy);
                var wolf = _unitFactory.CreateEnemy(wolfDefinition, tile);

                var deathSub = wolf.IsDead.Where(b => b).Subscribe(_ =>
                {
                    segment.IsCompleted.Value = true;
                });
                wolf.UnitSubscriptions.Insert(0, deathSub);
            };

            Action<Segment> allWolfsDefeatedCheck = s =>
            {
                var aliveWolfs = s.Units.Where(unit => unit.Type == UnitType.WolfEnemy && !unit.IsDead.Value);
                if (!aliveWolfs.Any())
                {
                    var caveTile = cave.Tile.Value;
                    cave.Damage(cave.Health.Value, null, true);
                    caveDestroyAction(caveTile);
                }
            };
            
            foreach (Unit wolf in segment.Units.Where(unit => unit.Type == UnitType.WolfEnemy))
            {
                IDisposable wolfSub = wolf.IsDead.Where(b => b).Subscribe(_ => allWolfsDefeatedCheck(segment));
                wolf.UnitSubscriptions.Insert(0, wolfSub);
            }
        }

        private void CreateMiniBossSegment(Segment segment)
        {
            Unit miniBoss = segment.Units.FirstOrDefault(unit => unit.Type == UnitType.FishermanMiniBoss);
            Unit heart = segment.Units.FirstOrDefault(unit => unit.Type == UnitType.IslandHeart);
            if(miniBoss == null)
                throw new Exception("No Mini Boss in End Segment");
            miniBoss.DeathActions.Add(tile =>
            {
                heart.IsInvincible.Value = false;
            });
            
            heart.DeathActions.Add(tile =>
            {
                Loot loot = _itemContainer.GetRandomUnitLoot(heart, 15);
                loot.RewardTo(GameStateContainer.Player, tile.WorldPosition);
                WorldLootContainer.AddToLootDrops(loot);
                WorldLootContainer.DropLoot.Execute();

                Sequence sequence = DOTween.Sequence();
                sequence.InsertCallback(.7f, () =>
                {
                    var explosionTargetTile = tile.Island.StartTile.Neighbours.FirstOrDefault(t => !t.HasUnit);
                    explosionTargetTile?.MoveUnit(GameStateContainer.Player);
                });
            });
        }

        private void CreateStartSegment(Segment segment)
        {
            var center = segment.CenterTile;
            var averageDirection = center.GetAverageDirection();

            var worldShip = _container.InstantiatePrefab(_shipView).GetComponent<WorldShipView>();
            worldShip.Initialize(center.Island);
            
            worldShip.transform.position = center.WorldPosition;
            worldShip.transform.right = averageDirection;
            
            
        }

        private void CreateSimpleEnemySegment(Segment segment)
        {
            foreach (Unit enemy in segment.Units.Where(unit => unit is Enemy))
            {
                var sub = enemy.IsDead
                    .Where(b => b)
                    .Subscribe(_ =>
                    {
                        var livingEnemies = segment.Units.Where(unit => unit is Enemy && !unit.IsDead.Value);
                        if (!livingEnemies.Any())
                            segment.IsCompleted.Value = true;

                    });
            }
        }
        
        
    }
}