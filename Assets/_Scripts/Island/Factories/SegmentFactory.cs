using System;
using System.Collections.Generic;
using System.Linq;
using Container;
using DG.Tweening;
using Game;
using Installer;
using Models;
using Views;
using Zenject;
using UniRx;
using UnityEngine;

namespace Factories
{
    public class SegmentFactory
    {
        private const float EnemySpawnChance = .3f;
        private const float DestructibleSpawnChance = .4f;

        [Inject(Id = IslandInstaller.IslandParent)] private Transform _islandParent;
        
        [Inject] private UnitFactory _unitFactory;
        [Inject] private UnitContainer _unitContainer;
        [Inject] private UnitDeathActionContainer _unitDeathActionContainer;
        [Inject] private ItemContainer _itemContainer;

        [Inject] private WorldShipView _shipView;

        [Inject] private SegmentContainer _segmentContainer;
        //[Inject] private GameAreaManager _gameAreaManager;
        [Inject] private DiContainer _container;

        private Dictionary<SegmentType, Action<Segment>> _segmentSpawnActions = new();

        public SegmentFactory()
        {
            _segmentSpawnActions[SegmentType.Start] = CreateStartSegment;
            _segmentSpawnActions[SegmentType.Ruin] = CreateRuinSegment;
            _segmentSpawnActions[SegmentType.Forrest] = CreateSimpleEnemySegment;
            _segmentSpawnActions[SegmentType.WolfCamp] = CreateWolfCamp;
            _segmentSpawnActions[SegmentType.MiniBoss] = CreateMiniBossSegment;
        }

        public SegmentView CreateSegmentView(Segment segment, SegmentView prefab = null)
        {
            if(prefab == null)
            {
                prefab = _segmentContainer.GetPrefab(segment.Type);
            }
            SegmentView view = _container.InstantiatePrefab(prefab).GetComponent<SegmentView>();
            view.transform.SetParent(_islandParent);
            view.Initialize(segment);
            
            for (int i = 0; i < view.SegmentUnitDefinitions.Length; i++)
            {
                SegmentUnitDefinition definition = view.SegmentUnitDefinitions[i];
                Tile tile = segment.Tiles.GetClosestTileFromPosition(definition.Point.position);

                GameUnit gameUnit = null;

                if (definition.Type.ToString().ToLower().Contains("enemy") ||
                    definition.Type.ToString().ToLower().Contains("boss"))
                {
                    gameUnit =
                        _unitFactory.CreateEnemy(_unitContainer.GetEnemyDefinition(definition.Type), tile);
                }
                else if (definition.Type.ToString().ToLower().Contains("interact"))
                {
                    gameUnit = _unitFactory.CreateUnit(
                        _unitContainer.GetInteractableDefinition(definition.Type), tile);
                }
                else
                {
                    gameUnit = _unitFactory.CreateUnit(
                        _unitContainer.GetUnitDefinition(definition.Type), tile);
                }
                
                segment.AddUnit(gameUnit);
            }
                                                                          
            if(_segmentSpawnActions.TryGetValue(prefab.Type, out Action<Segment> action))
            {
                action(segment);
            }

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
            GameUnit cave = segment.Units.First(unit => unit.Type == UnitType.Cave);
            
            Action<Tile> caveDestroyAction = tile =>
            {
                EnemyDefinition wolfDefinition = _unitContainer.GetEnemyDefinition(UnitType.BigWolfEnemy);
                Enemy wolf = _unitFactory.CreateEnemy(wolfDefinition, tile);

                IDisposable deathSub = wolf.IsDead.Where(b => b).Subscribe(_ =>
                {
                    segment.IsCompleted.Value = true;
                });
                wolf.UnitSubscriptions.Insert(0, deathSub);
            };

            Action<Segment> allWolfsDefeatedCheck = s =>
            {
                IEnumerable<GameUnit> aliveWolfs = s.Units.Where(unit => unit.Type == UnitType.WolfEnemy && !unit.IsDead.Value);
                if (!aliveWolfs.Any())
                {
                    Tile caveTile = cave.Tile.Value;
                    cave.Damage(cave.Health.Value, null, true);
                    caveDestroyAction(caveTile);
                }
            };
            
            foreach (GameUnit wolf in segment.Units.Where(unit => unit.Type == UnitType.WolfEnemy))
            {
                IDisposable wolfSub = wolf.IsDead.Where(b => b).Subscribe(_ => allWolfsDefeatedCheck(segment));
                wolf.UnitSubscriptions.Insert(0, wolfSub);
            }
        }

        private void CreateRuinSegment(Segment segment)
        {
            GameUnit cave = segment.Units.FirstOrDefault(unit => unit.Type == UnitType.Cave);
            
            Action<Tile> caveDestroyAction = tile =>
            {
                EnemyDefinition wolfDefinition = _unitContainer.GetEnemyDefinition(UnitType.BigWolfEnemy);
                Enemy wolf = _unitFactory.CreateEnemy(wolfDefinition, tile);

                IDisposable deathSub = wolf.IsDead.Where(b => b).Subscribe(_ =>
                {
                    segment.IsCompleted.Value = true;
                });
                wolf.UnitSubscriptions.Insert(0, deathSub);
            };

            foreach (GameUnit wolf in segment.Units.Where(unit => unit.Type == UnitType.WolfEnemy))
            {
                wolf.DeathActions.Add(t =>
                {
                    IEnumerable<GameUnit> wolfs = segment.Units.Where(unit => unit.Type == UnitType.WolfEnemy && !unit.IsDead.Value);
                    if (!wolfs.Any())
                    {
                        cave.Damage(cave.Health.Value, null, true);
                        caveDestroyAction(cave.Tile.Value);
                    }
                });
            }
        }

        private void CreateMiniBossSegment(Segment segment)
        {
            GameUnit miniBoss = segment.Units.FirstOrDefault(unit => unit.Type == UnitType.FishermanMiniBoss);
            GameUnit heart = segment.Units.FirstOrDefault(unit => unit.Type == UnitType.IslandHeart);
            if(miniBoss == null)
            {
                throw new Exception("No Mini Boss in End Segment");
            }
            miniBoss.DeathActions.Add(tile =>
            {
                heart.IsInvincible.Value = false;
            });
            
            heart.DeathActions.Add(tile =>
            {
                Loot loot = _unitContainer.GetRandomUnitLoot(heart, 15);
                loot.RewardTo(GameStateContainer.Player, tile.WorldPosition);
                WorldLootContainer.AddToLootDrops(loot);
                WorldLootContainer.DropLoot.Execute();

                Sequence sequence = DOTween.Sequence();
                sequence.InsertCallback(.7f, () =>
                {
                    //_gameAreaManager.Island.DissolveIslandCommand.Execute();
                });
            });
        }

        private void CreateStartSegment(Segment segment)
        {
            WorldShipView worldShip = _container.InstantiatePrefab(_shipView).GetComponent<WorldShipView>();
            worldShip.Initialize(segment);
        }

        private void CreateSimpleEnemySegment(Segment segment)
        {
            foreach (GameUnit enemy in segment.Units.Where(unit => unit is Enemy))
            {
                IDisposable sub = enemy.IsDead
                    .Where(b => b)
                    .Subscribe(_ =>
                    {
                        IEnumerable<GameUnit> livingEnemies = segment.Units.Where(unit => unit is Enemy && !unit.IsDead.Value);
                        if (!livingEnemies.Any())
                        {
                            segment.IsCompleted.Value = true;
                        }
                    });
            }
        }
        
        
    }
}