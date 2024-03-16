using System;
using System.Collections.Generic;
using System.Linq;
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

        [SerializeField] private SegmentView[] Segments;
        [SerializeField] private SegmentView StartSegment;
        [SerializeField] private SegmentView EndSegment;
        [SerializeField] private SegmentView[] BossSegments;

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
        
        #if UNITY_EDITOR
        [Header("DEBUG")]
        [SerializeField] private GameObject DEBUG_SPHERE;
        #endif
        
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

#if UNITY_EDITOR
            Container.Bind<GameObject>().WithId("Sphere").FromInstance(DEBUG_SPHERE).AsSingle();
#endif
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

            Container.Bind<SegmentContainer>().FromInstance(new(Segments, StartSegment, EndSegment, BossSegments)).AsSingle();
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
        public List<SegmentView> SegmentPool;
        public SegmentView StartSegment;
        public SegmentView EndSegment;
        public List<SegmentView> BossSegments;

        public SegmentContainer(
            SegmentView[] segmentDefinitions, 
            SegmentView start, 
            SegmentView end, 
            SegmentView[] bossDefinitions)
        {
            SegmentPool = new(segmentDefinitions.ToList());
            
            StartSegment = start;
            EndSegment = end;

            BossSegments = new(bossDefinitions.ToList());
        }

        public SegmentView GetPrefab(SegmentType type)
        {
            if (type == SegmentType.Start)
                return StartSegment;
            if (type == SegmentType.End)
                return EndSegment;
            
            SegmentView prefab = SegmentPool.FirstOrDefault(p => p.Type == type);
            if (prefab != null)
                return prefab;

            prefab = BossSegments.FirstOrDefault(p => p.Type == type);
            if (prefab != null)
                return prefab;

            throw new Exception($"No Segment Prefab of Type {type} in SegmentContainer");
        }
    }
}