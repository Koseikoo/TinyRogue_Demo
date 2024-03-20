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
        [Inject] private UnitContainer _unitContainer;
        [Inject] private UnitDeathActionContainer _unitDeathActionContainer;

        [Inject] private SegmentContainer _segmentContainer;
        [Inject] private DiContainer _container;

        private Dictionary<SegmentType, Action<Segment>> _segmentSpawnActions = new();

        public SegmentFactory()
        {
            _segmentSpawnActions[SegmentType.EnemyCamp] = CreateEnemyCampSegment;
            _segmentSpawnActions[SegmentType.End] = CreateEndSegment;
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

                if (definition.Type.ToString().ToLower().Contains("enemy") ||
                    definition.Type.ToString().ToLower().Contains("boss"))
                {
                    unit =
                        _unitFactory.CreateEnemy(_unitContainer.GetEnemyDefinition(definition.Type), tile);
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
                SegmentType.EnemyCamp => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                SegmentType.Village => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                SegmentType.Start => _container.Instantiate<Segment>(new object[]{prefab, center}),
                SegmentType.End => _container.Instantiate<Segment>(new object[]{prefab, center}),
                SegmentType.Ruin => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                SegmentType.Boss => _container.Instantiate<DefeatSegment>(new object[]{prefab, center}),
                _ => throw new ArgumentOutOfRangeException()
            };

            return segment;
        }

        private void CreateEnemyCampSegment(Segment segment)
        {
            var orcTiles = segment.Tiles.FindAll(tile => tile.Unit.Value is OrcEnemy);

            for (int i = 0; i < orcTiles.Count; i++)
            {
                OrcEnemy orc = orcTiles[i].Unit.Value as OrcEnemy;
                var neighbourWolfTile = orcTiles[i].Neighbours.FirstOrDefault(tile => tile.Unit.Value is WolfEnemy);
                if (neighbourWolfTile == null)
                    throw new Exception("No neighboring Wolf!");
                
                neighbourWolfTile.Unit.Value.DeathActions.Add(tile => orc.IsEnraged.Value = true);
            }
        }

        private void CreateEndSegment(Segment segment)
        {
            try
            {
                Unit unit = segment.Units.FirstOrDefault(unit => unit.Type == UnitType.FishermanMiniBoss);
                unit.DeathActions.Add(_unitDeathActionContainer.UnlockEndTileAction);
            }
            catch (Exception e)
            {
                throw new Exception("No Mini Boss in End Segment");
            }
            
            
        }
        
        
    }
}