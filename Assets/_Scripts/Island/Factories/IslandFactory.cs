using System.Collections.Generic;
using Installer;
using Models;
using UnityEngine;
using Views;
using Zenject;
using Random = UnityEngine.Random;
using Tile = Models.Tile;

namespace Factories
{
    public class IslandFactory
    {
        [Inject] private PolygonConfig _islandPolygonConfig;
        [Inject] private PolygonFactory _polygonFactory;
        [Inject] private HexGridFactory _hexGridFactory;
        [Inject] private IslandInitializer _islandInitializer;
        
        [Inject] private IslandViewFactory _islandViewFactory;
        [Inject] private SegmentContainer _segmentContainer;
        [Inject] private SegmentFactory _segmentFactory;

        [Inject] private DiContainer _container;
        
        #if UNITY_EDITOR
        [Inject(Id = "Sphere")] private GameObject _debugSphere;
        #endif

        
        public Island CreateIsland(int level)
        {
            bool isBossLevel = level % 5 == 0 && level > 0;
            Island island = isBossLevel ?
                CreateBossIslandModel(level) :
                CreateIslandModel(level);

            for (int i = 0; i < island.Segments.Count; i++)
            {
                _segmentFactory.CreateSegmentView(island.Segments[i]);
            }
            
            _islandViewFactory.CreateIslandView(island);
            return island;
        }

        public Island CreateTestIsland(float size)
        {
            Vector3[] polygon = _polygonFactory.Create(size);
            Bounds bounds = polygon.GetBounds();
            Vector3[,] baseGrid = _hexGridFactory.Create(bounds, Island.HexagonSize);
            List<Tile> islandTiles = CreateIslandTiles(baseGrid, polygon);
            
            Island island = _container.Instantiate<Island>(
                new object[] {islandTiles, 0});
            _islandInitializer.Initialize(island, polygon);
            
            foreach (Tile tile in island.Tiles)
            {
                SetVisualAttributes(tile);
            }
            
            _islandViewFactory.CreateIslandView(island);
            return island;
        }

        private Island new_CreateIslandModel(int level)
        {
            // create polygon
            // create segment circles
            
            // create tiles (if inside circle add to segment)
            // add neighbours
            
            // set start and end tiles
            // set edge tiles

            return null;
        }

        private Island CreateIslandModel(int level)
        {
            Vector3[] polygon = _polygonFactory.Create(_islandPolygonConfig);
            Bounds bounds = polygon.GetBounds();
            Vector3[,] baseGrid = _hexGridFactory.Create(bounds, Island.HexagonSize);
            List<Tile> islandTiles = CreateIslandTiles(baseGrid, polygon);

            Island island = _container.Instantiate<Island>(
                new object[] {islandTiles, level});
            _islandInitializer.Initialize(island, polygon);

            foreach (Tile tile in island.Tiles)
            {
                SetVisualAttributes(tile);
            }

            return island;
        }

        private Island CreateBossIslandModel(int level)
        {
            Vector3[] polygon = _polygonFactory.Create(8);
            Bounds bounds = polygon.GetBounds();
            Vector3[,] baseGrid = _hexGridFactory.Create(bounds, Island.HexagonSize);
            List<Tile> islandTiles = CreateIslandTiles(baseGrid, polygon);
            
            Island island = _container.Instantiate<Island>(
                new object[] {islandTiles, 0}
                );
            _islandInitializer.Initialize(island, polygon);
            CreateBossIslandSegments(island);

            foreach (Tile tile in island.Tiles)
                SetVisualAttributes(tile);
            
            _islandViewFactory.CreateIslandView(island);
            return island;
        }

        List<Tile> CreateIslandTiles(Vector3[,] positions, Vector3[] polygon)
        {
            Tile lastTile = null;
            List<Tile> islandTiles = new();

            List<Tile> lastRow = new();
            List<Tile> currentRow = new();
        
            Vector3 position;

            for (int y = 0; y < positions.GetLength(1); y++) {
                for (int x = 0; x < positions.GetLength(0); x++) {
                
                    position = positions[x, y];
                    if (!position.IsInsidePolygon(polygon))
                        continue;
                    
                    Tile tile = _container.Instantiate<Tile>(
                        new object[] {position});
                    islandTiles.Add(tile);
                    LinkNeighbours(tile);
                }

                lastTile = null;
                lastRow = new(currentRow);
                currentRow.Clear();
            }

            return islandTiles;
        
            void LinkNeighbours(Tile tile)
            {
                float islandDistance = Island.TileDistance + Island.TileBuffer;
                if (lastTile != null)
                {
                    LinkTiles(tile, lastTile);
                }

                foreach (Tile t in lastRow)
                {
                    if(Vector3.Distance(tile.WorldPosition, t.WorldPosition) <= islandDistance)
                        LinkTiles(tile, t);
                }
            
                currentRow.Add(tile);
                lastTile = tile;
            }
            
            void LinkTiles(Tile a, Tile b)
            {
                a.AddNeighbour(b);
                b.AddNeighbour(a);
            }
        }
        
        private void SetVisualAttributes(Tile tile)
        {
            tile.TerrainType = GetTerrainType();
            tile.BoardType = GetBoardType();
            tile.GrassType = GetGrassType();
            tile.BridgeType = BridgeType.None;

            TerrainType GetTerrainType()
            {
                if (tile.IsEdgeTile)
                    return TerrainType.Top;
                return TerrainType.Surface;
            }

            GrassType GetGrassType()
            {
                if (tile.IsPathTile.Value)
                    return tile.GrassType;
                return tile.BoardType != BoardType.None ? GrassType.Board : GrassType.Base;
            }

            BoardType GetBoardType()
            {
                if (tile.IsEndTile)
                    return BoardType.Metal;
                if (tile.IsSegmentTile || tile.IsPathTile.Value || tile.IsStartTile)
                    return BoardType.None;
                return Random.value > .6f || tile.IsPathTile.Value ? BoardType.Stone : BoardType.None;
            }
        }

        private void CreateBossIslandSegments(Island island)
        {
            List<Tile> islandTiles = new(island.Tiles);
        
            Tile centerTile = islandTiles.GetTileClosestToPosition(island.StartTile.WorldPosition);
            SegmentView segmentDefinition = _segmentContainer.StartSegment;
            Segment startSegment = _segmentFactory.CreateSegment(segmentDefinition, centerTile);
        
            Tile tile = islandTiles.GetTileClosestToPosition(default);
            SegmentView definition = _segmentContainer.BossSegments.PickRandom();
            Segment segment = _segmentFactory.CreateSegment(definition, tile);
        }

        
    }
}