using System;
using System.Collections.Generic;
using Container;
using Factories;
using Models;
using UnityEngine;
using UnityEngine.Serialization;
using Views;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(fileName = "IslandInstaller", menuName = "Installer/IslandInstaller")]
    public class IslandInstaller : ScriptableObjectInstaller<IslandInstaller>
    {
        [SerializeField] private TileView _tilePrefab;
        [SerializeField] private IslandView _islandPrefab;
        
        [SerializeField] private PolygonConfig _polygonConfig;

        [Header("Segment Definitions")]
        [SerializeField] private SegmentDefinition _startDefinition;
        [SerializeField] private SegmentDefinition _endDefinition;
        [SerializeField] private SegmentDefinition _forrestDefinition;
        [SerializeField] private SegmentDefinition _enemyCampDefinition;
        [SerializeField] private SegmentDefinition _villageDefinition;
        [SerializeField] private SegmentDefinition _ruinDefinition;
        [SerializeField] private SegmentDefinition _bossDefinition;

        [Header("Tile Visuals")]
        [Header("Grass")]
        [SerializeField] private GameObject _basePrefab;
        [SerializeField] private GameObject _boardPrefab;
        [SerializeField] private GameObject _surfacePrefab;
        [SerializeField] private GameObject _pathBend60Prefab;
        [SerializeField] private GameObject _pathStraightPrefab;
        [SerializeField] private GameObject _pathEndPrefab;

        [Header("Board")]
        [SerializeField] private GameObject _metalBorderPrefab;
        [SerializeField] private GameObject _stoneChiseledPrefab;
        [SerializeField] private GameObject[] _stoneVariantPrefabs;
        
        [Header("Bridge")]
        [SerializeField] private GameObject _bridgePrefab;
        [SerializeField] private GameObject _bridgeBrokenPrefab;
        
        [Header("Terrain")]
        [SerializeField] private GameObject _topPrefab;
        [SerializeField] private GameObject _topSurfacePrefab;
        
        public override void InstallBindings()
        {
            BindPrefabs();
            BindContainer();
            BindFactories();

            Container.Bind<IslandInitializer>().AsSingle();
        }

        private void BindPrefabs()
        {
            Container.Bind<TileView>().FromInstance(_tilePrefab).AsSingle();
            Container.Bind<IslandView>().FromInstance(_islandPrefab).AsSingle();
            Container.Bind<PolygonConfig>().FromInstance(_polygonConfig).AsSingle();
        }

        private void BindContainer()
        {
            Container.Bind<GrassContainer>().FromInstance(new(
                _basePrefab,
                _boardPrefab,
                _surfacePrefab,
                _pathBend60Prefab,
                _pathStraightPrefab,
                _pathEndPrefab)).AsSingle();

            Container.Bind<BoardContainer>().FromInstance(new(
                _metalBorderPrefab,
                _stoneChiseledPrefab,
                _stoneVariantPrefabs)).AsSingle();
            
            Container.Bind<BridgeContainer>().FromInstance(new(
                _bridgePrefab,
                _bridgeBrokenPrefab)).AsSingle();
            
            Container.Bind<TerrainContainer>().FromInstance(new(
                _topPrefab,
                _topSurfacePrefab)).AsSingle();

            Container.Bind<TileActionContainer>().AsSingle();

            Container.Bind<SegmentContainer>().FromInstance(new(
                _startDefinition, 
                _endDefinition, 
                _forrestDefinition, 
                _enemyCampDefinition, 
                _villageDefinition,
                _ruinDefinition,
                _bossDefinition)).AsSingle();
        }

        private void BindFactories()
        {
            Container.Bind<PolygonFactory>().AsSingle();
            Container.Bind<HexGridFactory>().AsSingle();
            Container.Bind<IslandFactory>().AsSingle();
            Container.Bind<SegmentFactory>().AsSingle();
            Container.Bind<IslandViewFactory>().AsSingle();
        }
    }

    public class SegmentContainer
    {
        public List<SegmentDefinition> SegmentPool;
        public SegmentDefinition StartSegment;
        public SegmentDefinition EndSegment;
        public List<SegmentDefinition> BossSegments;
        private Dictionary<SegmentType, SegmentView> _segmentPrefabs = new();

        public SegmentContainer(
            SegmentDefinition start, 
            SegmentDefinition end, 
            SegmentDefinition forrest, 
            SegmentDefinition enemyCamp, 
            SegmentDefinition village,
            SegmentDefinition ruin,
            SegmentDefinition boss)
        {
            _segmentPrefabs = new(new Dictionary<SegmentType, SegmentView>()
            {
                { SegmentType.Forrest, forrest.Prefab },
                { SegmentType.EnemyCamp, enemyCamp.Prefab },
                { SegmentType.Village , village.Prefab},
                { SegmentType.Ruin , ruin.Prefab},
                { SegmentType.Boss , boss.Prefab},
            });

            SegmentPool = new()
            {
                new(forrest),
                new(enemyCamp),
                new(ruin),
                new(village),
            };

            StartSegment = new(start);
            EndSegment = new(end);

            BossSegments = new()
            {
                new(boss)
            };
        }

        public SegmentView GetSegmentPrefab(SegmentType type)
        {
            if (type == SegmentType.Start)
                return StartSegment.Prefab;
            if (type == SegmentType.End)
                return EndSegment.Prefab;
            
            if (!_segmentPrefabs.ContainsKey(type))
                throw new Exception($"no prefab for segment {type}");
            return _segmentPrefabs[type];
        }
    }
}