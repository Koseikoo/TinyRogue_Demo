using System;
using System.Collections.Generic;
using Container;
using Installer;
using Models;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.Tilemaps;
using Views;
using Zenject;
using Random = UnityEngine.Random;
using Tile = Models.Tile;

namespace Factories
{
    public class IslandFactory
    {
        private const int DirectionsToCheck = 40;
        
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
            _islandInitializer.Initialize(island);
            
            foreach (Tile tile in island.Tiles)
            {
                SetVisualAttributes(tile);
            }
            
            _islandViewFactory.CreateIslandView(island);
            return island;
        }

        private Island CreateIslandModel(int level)
        {
            Vector3[] polygon = _polygonFactory.Create(_islandPolygonConfig);
            Bounds bounds = polygon.GetBounds();
            Vector3[,] baseGrid = _hexGridFactory.Create(bounds, Island.HexagonSize);
            List<Tile> islandTiles = CreateIslandTiles(baseGrid, polygon);

            Island island = _container.Instantiate<Island>(
                new object[] {islandTiles, level});
            _islandInitializer.Initialize(island);
            for (int i = 0; i < 10; i++)
            {
                List<Segment> segments = GetIslandSegments(island, polygon);
                if (segments.Count > 2)
                {
                    island.AddSegments(segments);
                    break;
                }
            }
            
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
            _islandInitializer.Initialize(island);
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
                if (tile.PathTile.Value)
                    return tile.GrassType;
                return tile.BoardType != BoardType.None ? GrassType.Board : GrassType.Base;
            }

            BoardType GetBoardType()
            {
                if (tile.IsEndTile)
                    return BoardType.Metal;
                if (tile.IsSegmentTile || tile.PathTile.Value || tile.IsStartTile)
                    return BoardType.None;
                return Random.value > .6f || tile.PathTile.Value ? BoardType.Stone : BoardType.None;
            }
        }
        private List<Segment> GetIslandSegments(Island island, Vector3[] polygon)
        {
            List<Tile> islandTiles = new(island.Tiles);
            Vector3[] checkDirections = GetCheckDirections();
        
            Segment startSegment = _segmentFactory.CreateSegment(_segmentContainer.StartSegment, island.StartTile);
            Segment endSegment = _segmentFactory.CreateSegment(_segmentContainer.EndSegment, island.EndTile);

            List<Segment> spacedSegments = new()
            {
                startSegment,
                endSegment
            };

            List<Segment> currentSegments = new()
            {
                startSegment
            };

            while (currentSegments.Count > 0)
            {
                List<Segment> newSegments = new();
        
                for (int i = 0; i < currentSegments.Count; i++)
                {
                    for (int j = 0; j < checkDirections.Length; j++)
                    {
                        SegmentView prefab = _segmentContainer.SegmentPool.PickRandom();
                        
                        float distance = currentSegments[i].Radius.GetSegmentDistance(prefab.Radius);
                        Vector3 checkPosition = currentSegments[i].Tile.WorldPosition + (checkDirections[j] * distance);
                        Tile centerTile = islandTiles.GetTileClosestToPosition(checkPosition);

                        if(centerTile == null)
                            continue;
                        
                        checkPosition = centerTile.WorldPosition;
                        Segment segment = _segmentFactory.CreateSegment(prefab, centerTile);
                        
                        bool isWithinPolygon = segment.IsWithinPolygon(polygon, checkPosition);
                        bool isInisideSegment = segment.IsInsideSegment(spacedSegments);
                        bool isInisidePolygon = checkPosition.IsInsidePolygon(polygon);
                
                        if (isWithinPolygon && !isInisideSegment && isInisidePolygon)
                        {
                            newSegments.Add(segment);
                            spacedSegments.Add(segment);

                            Tile previousCenterTile = island.Tiles
                                .GetTileClosestToPosition(currentSegments[i].Tile.WorldPosition);
                            Tile newCenterTile = island.Tiles.GetTileClosestToPosition(segment.Tile.WorldPosition);
                            
                            var path = AStar.FindPath(previousCenterTile, newCenterTile, unit => true);
                            path = path.GetRange(1, path.Count - 2);

                            for (int k = 0; k < path.Count; k++)
                            {
                                path[k].GrassType = GetPathType(path, k);

                                switch (path[k].GrassType)
                                {
                                    case GrassType.Bend60:
                                        path[k].GrassRotation = GetPathBendRotation(path, k);
                                        break;
                                    case GrassType.Straight:
                                        path[k].GrassRotation = GetPathStraightRotation(path, k);
                                        break;
                                    case GrassType.End:
                                        path[k].GrassRotation = GetPathEndRotation(path, k);
                                        break;
                                    default:
                                        throw new Exception($"path tile has wrong type ({path[k].GrassType})");
                                }
                                path[k].PathTile.Value = true;
                            }
                        }
                    }
                }
        
                currentSegments.Clear();
                currentSegments.AddRange(new List<Segment>(newSegments));
                newSegments.Clear();
            }

            return spacedSegments;
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

        private GrassType GetPathType(List<Tile> path, int index)
        {
            if (index == 0 || index == path.Count - 1)
                return GrassType.End;

            var nextDirection = (path[index + 1].WorldPosition - path[index].WorldPosition).normalized;
            var previousDirection = (path[index - 1].WorldPosition - path[index].WorldPosition).normalized;

            if (Vector3.Dot(nextDirection, previousDirection) < -.9)
                return GrassType.Straight;
            return GrassType.Bend60;
        }

        private float GetPathEndRotation(List<Tile> path, int index)
        {
            Vector3 startDirection = Vector3.right;

            for (int i = 0; i < 6; i++)
            {
                Vector3 direction = startDirection.RotateVector(Vector3.up, i * 60);
                int indexToCheck = index == 0 ? 1 : path.Count - 2;

                var targetDirection = (path[indexToCheck].WorldPosition - path[index].WorldPosition).normalized;

                if (Vector3.Dot(direction, targetDirection) > .9f)
                    return i * 60;
            }

            throw new Exception("Cant Get Rotation (End Piece)");
        }

        private float GetPathStraightRotation(List<Tile> path, int index)
        {
            float rotationOffset = 180;
            Vector3 startDirection = Vector3.right;
            
            var targetDirectionIn = (path[index - 1].WorldPosition - path[index].WorldPosition).normalized;
            var targetDirectionOut = (path[index + 1].WorldPosition - path[index].WorldPosition).normalized;

            for (int i = 0; i < 6; i++)
            {
                Vector3 directionIn = startDirection.RotateVector(Vector3.up, i * 60);
                Vector3 directionOut = directionIn.RotateVector(Vector3.up, rotationOffset);
                
                var inDot = Vector3.Dot(directionIn, targetDirectionIn);
                var outDot = Vector3.Dot(directionOut, targetDirectionOut);
                
                var altInDot = Vector3.Dot(directionIn, targetDirectionOut);
                var altOutDot = Vector3.Dot(directionOut, targetDirectionIn);

                bool validRotation = inDot > .9f && outDot > .9f;
                bool altValidRotation = altInDot > .9f && altOutDot > .9f;

                if (validRotation || altValidRotation)
                    return i * 60;
            }

            throw new Exception("Cant Get Rotation (Straight Piece)");
            
        }

        private float GetPathBendRotation(List<Tile> path, int index)
        {
            float rotationOffset = 240;
            Vector3 startDirection = Vector3.right;
            
            var targetDirectionIn = (path[index - 1].WorldPosition - path[index].WorldPosition).normalized;
            var targetDirectionOut = (path[index + 1].WorldPosition - path[index].WorldPosition).normalized;

            for (int i = 0; i < 6; i++)
            {
                Vector3 directionIn = startDirection.RotateVector(Vector3.up, i * 60);
                Vector3 directionOut = directionIn.RotateVector(Vector3.up, rotationOffset);
                
                var inDot = Vector3.Dot(directionIn, targetDirectionIn);
                var outDot = Vector3.Dot(directionOut, targetDirectionOut);
                
                var altInDot = Vector3.Dot(directionIn, targetDirectionOut);
                var altOutDot = Vector3.Dot(directionOut, targetDirectionIn);

                bool validRotation = inDot > .9f && outDot > .9f;
                bool altValidRotation = altInDot > .9f && altOutDot > .9f;

                if (validRotation || altValidRotation)
                    return i * 60;
            }

            throw new Exception("Cant Get Rotation (Bend Piece)");
        }

        private Vector3[] GetCheckDirections()
        {
            Vector3[] directions = new Vector3[DirectionsToCheck];
            Vector3 startVector = Vector3.right;

            float angle = 360f / DirectionsToCheck;

            for (int i = 0; i < directions.Length; i++)
            {
                directions[i] = startVector.RotateVector(Vector3.up, angle * i);
            }

            return directions;
        }
    }
}