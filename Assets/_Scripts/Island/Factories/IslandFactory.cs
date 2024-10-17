using System;
using System.Collections.Generic;
using System.Linq;
using Container;
using Game;
using Installer;
using Models;
using TinyRogue;
using UnityEngine;
using Views;
using Zenject;
using Random = UnityEngine.Random;
using Tile = Models.Tile;

namespace Factories
{
    public class IslandFactory
    {
        [Inject] private ArchipelConfig _archipelConfig;
        [Inject] private PolygonConfig _islandPolygonConfig;
        [Inject] private PolygonFactory _polygonFactory;
        [Inject] private HexGridFactory _hexGridFactory;
        
        [Inject] private IslandViewFactory _islandViewFactory;
        [Inject] private SegmentContainer _segmentContainer;
        [Inject] private TileActionContainer _tileActionContainer;
        [Inject] private SegmentFactory _segmentFactory;
        
        [Inject] private UnitFactory _unitFactory;
        [Inject] private UnitContainer _unitContainer;
        //[Inject] private GameAreaManager _gameAreaManager;

        [Inject] private DiContainer _container;

        private DungeonGenerator _dungeonGenerator;
        private System.Random _rand = new();

        
        #if UNITY_EDITOR
        [Inject(Id = "Sphere")] private GameObject _debugSphere;
        #endif


        public Island GetIsland()
        {
            if (_dungeonGenerator == null)
            {
                _dungeonGenerator = new(_archipelConfig);
            }
            
            Island island = _dungeonGenerator.GenerateEndlessIsland();
            return island;
        }
        
        public Archipel CreateArchipel()
        {
            if (_dungeonGenerator == null)
            {
                _dungeonGenerator = new(_archipelConfig);
            }

            Archipel archipel = _dungeonGenerator.GenerateDungeon();
            //_gameAreaManager.Archipel = archipel;
            archipel.EndTile.AddMoveToLogic(unit =>
            {
                if (unit is Player && archipel.EndIsland.EnemiesOnIsland == 0)
                {
                    //_gameAreaManager.SpawnNewArchipel();
                }
            });

            foreach (Island island in archipel.Islands)
            {
                AddBaseContent(island);
            }
            
            _islandViewFactory.CreateArchipelVisuals(archipel);
            return archipel;
        }
        
        private void AddBaseContent(Island island)
        {
            List<Tile> tilePool = new(island.Tiles);
            List<Tile> edgeTiles = tilePool.Where(tile => tile.Neighbours.Count < 6).ToList();
            
            int resourceAreas = 6;
            int enemies = 5;

            UnitType[] resourceTypes = new[]
            {
                UnitType.Tree,
                UnitType.Rock
            };

            for (int i = 0; i < resourceAreas; i++)
            {
                List<Tile> sectionTiles = new();
                Tile tile = edgeTiles.Random();
                sectionTiles.Add(tile);
                sectionTiles.AddRange(tile.Neighbours);
                
                UnitDefinition definition = _unitContainer.GetUnitDefinition(resourceTypes.Random());
                
                foreach (Tile sectionTile in sectionTiles)
                {
                    //bool spawnUnit = sectionTile == tile || _rand.Next(0, 100) > 50;
                    //bool isNoConnectionTile = island.Connections
                    //    .FirstOrDefault(pair => pair.start == sectionTile || pair.end == sectionTile) == default;
                    //bool isStartTile = _gameAreaManager.Archipel.StartTile == sectionTile;
                    //
                    //if (!sectionTile.HasUnit && isNoConnectionTile && !isStartTile && spawnUnit)
                    //{
                    //    _unitFactory.CreateUnit(definition, sectionTile);
//
                    //    tilePool.Remove(sectionTile);
                    //    if (edgeTiles.Contains(sectionTile))
                    //    {
                    //        edgeTiles.Remove(sectionTile);
                    //    }
                    //}
                }
            }
            
            for (int i = 0; i < enemies; i++)
            {
                Tile tile = tilePool.Random();
                //bool isNoConnectionTile = island.Connections
                //    .FirstOrDefault(pair => pair.start == tile || pair.end == tile) == default;
                //bool isStartTile = _gameAreaManager.Archipel.StartTile == tile;
//
                //if (isNoConnectionTile && !isStartTile)
                //{
                //    EnemyDefinition definition = _unitContainer.GetEnemyDefinition(UnitType.SpiderEnemy);
                //    _unitFactory.CreateEnemy(definition, tile);
                //    tilePool.Remove(tile);
                //}
            }

            Func<Island, bool> completeCondition = i => i.Units.Count(unit => unit is Enemy) == 0;
            Action<Island> onComplete = i =>
            {
                List<Tile> orderedTiles = i.Tiles
                    .OrderBy(tile => Vector3.Distance(tile.FlatPosition, tile.Island.Bounds.center))
                    .ToList();

                Tile chestTile = orderedTiles.FirstOrDefault(tile => !tile.HasUnit);

                InteractableDefinition definition = _unitContainer.GetInteractableDefinition(UnitType.ChestInteractable);
                Interactable chest = _unitFactory.CreateInteractable(definition, chestTile);

            };
            
            island.AddCompletionLogic(completeCondition, onComplete);
        }

        public Island CreateIsland(int level)
        {
            bool isBossLevel = level % 5 == 0 && level > 0;
            Island island = CreateIslandModel(level);
            
            int timeout = 0;
            while (timeout < 10 && island.Segments.Count < 3)
            {
                island = CreateIslandModel(level);
                timeout++;
            }

            for (int i = 0; i < island.Segments.Count; i++)
            {
                _segmentFactory.CreateSegmentView(island.Segments[i]);
            }
            
            _islandViewFactory.ResetPooledTiles();
            _islandViewFactory.CreateIslandView(island);
            return island;
        }

        public Island CreateEnemyTestIsland(int size = 8)
        {
            Vector3[] polygon = _polygonFactory.Create(size);
            Bounds bounds = polygon.GetBounds();
            Vector3[,] baseGrid = _hexGridFactory.Create(bounds, Island.HexagonSize);
            
            (Dictionary<Vector2Int, Tile> tileDict, List<Tile> tiles, Vector2Int maxValue) = GetIslandTiles(baseGrid, polygon);
            List<Tile> islandTiles = LinkTiles(tileDict, maxValue);
            
            List<Tile> edgeTiles = SetEdgeTiles(tiles);
            Tile startTile = edgeTiles.Random();
            startTile.AddMoveToLogic(_tileActionContainer.IslandEndAction);
            Tile endTile = tiles.GetTileFurthestAway(startTile);
            
            Island island = _container.Instantiate<Island>(
                new object[] {islandTiles, startTile, endTile, new List<Segment>(), 0});
            
            foreach (Tile tile in island.Tiles)
            {
                SetVisualAttributes(tile);
            }
            
            _islandViewFactory.ResetPooledTiles();
            _islandViewFactory.CreateIslandView(island);

            return island;
        }

        public Island CreateSegmentTestIsland(SegmentView segmentToTest)
        {

            Island i = CreateTestIslandModel(segmentToTest);
            _segmentFactory.CreateSegmentView(i.Segments[1], segmentToTest);
            _islandViewFactory.ResetPooledTiles();
            _islandViewFactory.CreateIslandView(i);
            return i;
        }

        private Island CreateTestIslandModel(SegmentView segmentToTestPrefab)
        {
            Vector3[] polygon = _polygonFactory.Create(_islandPolygonConfig);
            Bounds bounds = polygon.GetBounds();
            Vector3[,] baseGrid = _hexGridFactory.Create(bounds, Island.HexagonSize);
            
            (Dictionary<Vector2Int, Tile> tileDict, List<Tile> tiles, Vector2Int maxValue) = GetIslandTiles(baseGrid, polygon);
            
            List<Tile> islandTiles = LinkTiles(tileDict, maxValue);
            
            List<Tile> edgeTiles = SetEdgeTiles(tiles);
            Tile startTile = edgeTiles.Random();
            startTile.AddMoveToLogic(_tileActionContainer.IslandEndAction);
            Tile endTile = tiles.GetTileFurthestAway(startTile);

            List<Segment> segments = CreateTestSegments(islandTiles);
            foreach (Segment segment in segments)
            {
                List<Tile> segmentTiles = islandTiles.GetSegmentTiles(segment);                       
                segment.SetTiles(segmentTiles);

                List<Tile> outerTiles = segment.Tiles.GetTilesOutsideOfDistance(segment.CenterTile, segment.Size - 1);
                
                outerTiles.ForEach(t =>
                {
                    IEnumerable<Tile> pathNeighbours = t.Neighbours.Where(n => n.IsPathTile.Value);
                    if (!pathNeighbours.Any())
                    {
                        t.HeightLevel = -1;
                        t.IsWeak = true;
                    }
                });
            }

            tileDict = RemoveUnusedTiles(tileDict);
            islandTiles = LinkTiles(tileDict, maxValue);

            Segment heartSegment = segments.FirstOrDefault(s => s.Type == SegmentType.MiniBoss);
            
            Island island = _container.Instantiate<Island>(
                new object[] {islandTiles, startTile, endTile, segments, 0});
            
            foreach (Tile tile in island.Tiles)
            {
                SetVisualAttributes(tile);
            }

            return island;

            List<Segment> CreateTestSegments(List<Tile> tiles)
            {
                List<Segment> testSegments = new();
                
                Segment startSegment = _segmentFactory.CreateSegment(_segmentContainer.StartSegment, startTile);
                testSegments.Add(startSegment);

                float distance = startSegment.Radius.GetSegmentDistance(segmentToTestPrefab.Radius);
                Vector3 checkDirection = endTile.FlatPosition - startSegment.CenterTile.FlatPosition;
                checkDirection.Normalize();

                Tile centerTile = tiles.GetTileClosestToPosition(startSegment.CenterTile.FlatPosition + (checkDirection * distance));
                Segment segment = _segmentFactory.CreateSegment(segmentToTestPrefab, centerTile);
                testSegments.Add(segment);
                
                AssignPath(startSegment, segment);
                return testSegments;

            }
        }

        private Island CreateIslandModel(int level)
        {
            Vector3[] polygon = _polygonFactory.Create(_islandPolygonConfig);
            Bounds bounds = polygon.GetBounds();
            Vector3[,] baseGrid = _hexGridFactory.Create(bounds, Island.HexagonSize);
            
            (Dictionary<Vector2Int, Tile> tileDict, List<Tile> tiles, Vector2Int maxValue) = GetIslandTiles(baseGrid, polygon);
            
            List<Tile> islandTiles = LinkTiles(tileDict, maxValue);
            
            List<Tile> edgeTiles = SetEdgeTiles(tiles);
            Tile startTile = edgeTiles.Random();
            startTile.AddMoveToLogic(_tileActionContainer.IslandEndAction);
            Tile endTile = tiles.GetTileFurthestAway(startTile);

            List<Segment> segments = CreateSegments(startTile, endTile, tiles, polygon);
            
            foreach (Segment segment in segments)
            {
                List<Tile> segmentTiles = islandTiles.GetSegmentTiles(segment);                       
                segment.SetTiles(segmentTiles);
                
                List<Tile> outerTiles = segment.Tiles.GetTilesOutsideOfDistance(segment.CenterTile, segment.Size - 1);
                outerTiles.ForEach(t =>
                {
                    IEnumerable<Tile> pathNeighbours = t.Neighbours.Where(n => n.IsPathTile.Value);
                    if (!pathNeighbours.Any())
                    {
                        t.HeightLevel = -1;
                        t.IsWeak = true;
                    }
                });
            }

            islandTiles = LinkTiles(RemoveUnusedTiles(tileDict), maxValue);

            Segment heartSegment = segments.First(s => s.Type == SegmentType.MiniBoss);
            Island island = _container.Instantiate<Island>(
                new object[] {islandTiles, startTile, heartSegment.CenterTile, segments, 0});
            
            foreach (Tile tile in island.Tiles)
            {
                SetVisualAttributes(tile);
            }

            return island;
        }

        private Dictionary<Vector2Int, Tile> RemoveUnusedTiles(Dictionary<Vector2Int, Tile> tileDict)
        {
            Dictionary<Vector2Int, Tile> newTileDict = new();

            foreach (KeyValuePair<Vector2Int, Tile> kvp in tileDict)
            {
                bool isPathTile = kvp.Value.IsPathTile.Value;
                bool isSegmentTile = kvp.Value.IsSegmentTile;
                if (isPathTile || isSegmentTile)
                {
                    newTileDict[kvp.Key] = kvp.Value;
                }
            }
            return newTileDict;
        }
        
        public List<Tile> SetEdgeTiles(List<Tile> tiles)
        {
            List<Tile> edgeTiles = new();

            foreach (Tile tile in tiles)
            {
                if(tile.IsEdgeTile)
                {
                    edgeTiles.Add(tile);
                }
            }

            return edgeTiles;
        }

        public (Dictionary<Vector2Int, Tile> tileDict, List<Tile> tiles, Vector2Int maxValue) GetIslandTiles(Vector3[,] grid, Vector3[] polygon)
        {
            Vector2Int maxValue = default;
            Dictionary<Vector2Int, Tile> tileDict = new();
            List<Tile> tiles = new();
            for (int y = 0; y < grid.GetLength(1); y++) {
                for (int x = 0; x < grid.GetLength(0); x++) {
                
                    Vector3 position = grid[x, y];
                    if (!position.IsInsidePolygon(polygon))
                    {
                        continue;
                    }

                    Tile tile = _container.Instantiate<Tile>(
                        new object[] {position});
                    tileDict[new(x, y)] = tile;
                    tiles.Add(tile);

                    if (x > maxValue.x)
                    {
                        maxValue.x = x;
                    }

                    if (y > maxValue.y)
                    {
                        maxValue.y = y;
                    }
                }
            }

            return (tileDict, tiles, maxValue);
        }

        private List<Segment> CreateSegments(Tile startTile, Tile endTile, List<Tile> tiles, Vector3[] polygon)
        {
            List<Segment> spacedSegments = new();
            List<Segment> currentSegments = new();

            // create start Segment
            Segment startSegment = _segmentFactory.CreateSegment(_segmentContainer.StartSegment, startTile);
            spacedSegments.Add(startSegment);
            currentSegments.Add(startSegment);

            Segment previousSegment = startSegment;

            Vector3[] checkDirections = GetCheckDirections();

            while (currentSegments.Count > 0)
            {
                List<Segment> newSegments = new();

                for (int i = 0; i < currentSegments.Count; i++)
                {
                    previousSegment = currentSegments[i];
                    for (int j = 0; j < checkDirections.Length; j++)
                    {
                        SegmentView prefab = _segmentContainer.SegmentPool.Random();

                        float distance = currentSegments[i].Radius.GetSegmentDistance(prefab.Radius);
                        Vector3 checkPosition = currentSegments[i].CenterTile.FlatPosition +
                                                (checkDirections[j] * distance);
                        Tile centerTile = tiles.GetTileClosestToPosition(checkPosition);

                        if (centerTile == null)
                        {
                            continue;
                        }

                        checkPosition = centerTile.FlatPosition;
                        //Segment segment = _segmentFactory.CreateSegment(prefab, centerTile);
                        Segment segment = _segmentFactory.CreateSegment(prefab, centerTile);

                        bool isWithinPolygon = checkPosition.IsWithinPolygon(polygon, segment.Radius);
                        bool isInsideSegment = segment.IsInsideSegment(spacedSegments);
                        bool isInsidePolygon = checkPosition.IsInsidePolygon(polygon);

                        if (isWithinPolygon && !isInsideSegment && isInsidePolygon)
                        {
                            newSegments.Add(segment);
                            spacedSegments.Add(segment);

                            AssignPath(previousSegment, segment);
                        }
                    }
                }
                
                
                
                currentSegments.Clear();
                currentSegments.AddRange(new List<Segment>(newSegments));
                newSegments.Clear();
            }

            Segment lastSegment = spacedSegments.GetClosestSegment(endTile);
            int index = spacedSegments.IndexOf(lastSegment);

            SegmentView miniBossPrefab = _segmentContainer.BossSegments.Random();
            Segment miniBossSegment = _segmentFactory.CreateSegment(miniBossPrefab, lastSegment.CenterTile);
            
            spacedSegments.RemoveAt(index);
            spacedSegments.Insert(index, miniBossSegment);
            
            return spacedSegments;

            Vector3[] GetCheckDirections()
            {
                Vector3[] directions = new Vector3[40];
                Vector3 startVector = Vector3.right;

                float angle = 360f / directions.Length;

                for (int i = 0; i < directions.Length; i++)
                {
                    directions[i] = startVector.RotateVector(Vector3.up, angle * i);
                }

                return directions;
            }
        }
        
        void AssignPath(Segment startSegment, Segment endSegment)
        {
            List<Tile> path = AStar.FindPath(startSegment.CenterTile, endSegment.CenterTile, unit => true);
            path = path.Where(tile => Vector3.Distance(startSegment.CenterTile.FlatPosition, tile.FlatPosition) >= startSegment.Radius &&
                                      Vector3.Distance(endSegment.CenterTile.FlatPosition, tile.FlatPosition) >= endSegment.Radius).ToList();

            startSegment.ExitTiles.Add(path[0]);
            endSegment.EntryTiles.Add(path[^1]);

            if (path.Count == 1)
            {
                path[0].GrassType = GrassType.Board;
                path[0].IsPathTile.Value = true;
                return;
            }

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

                path[k].IsPathTile.Value = true;
            }
            
            GrassType GetPathType(List<Tile> path, int index)
            {
                if (index == 0 || index == path.Count - 1)
                {
                    return GrassType.End;
                }

                Vector3 nextDirection = (path[index + 1].FlatPosition - path[index].FlatPosition).normalized;
                Vector3 previousDirection = (path[index - 1].FlatPosition - path[index].FlatPosition).normalized;
    
                if (Vector3.Dot(nextDirection, previousDirection) < -.9)
                {
                    return GrassType.Straight;
                }

                return GrassType.Bend60;
            }
    
            float GetPathEndRotation(List<Tile> path, int index)
            {
                Vector3 startDirection = Vector3.right;
    
                for (int i = 0; i < 6; i++)
                {
                    Vector3 direction = startDirection.RotateVector(Vector3.up, i * 60);
                    int indexToCheck = index == 0 ? 1 : path.Count - 2;
    
                    Vector3 targetDirection = (path[indexToCheck].FlatPosition - path[index].FlatPosition).normalized;
    
                    if (Vector3.Dot(direction, targetDirection) > .9f)
                    {
                        return i * 60;
                    }
                }
    
                throw new Exception("Cant Get Rotation (End Piece)");
            }
    
            float GetPathStraightRotation(List<Tile> path, int index)
            {
                float rotationOffset = 180;
                Vector3 startDirection = Vector3.right;
    
                Vector3 targetDirectionIn = (path[index - 1].FlatPosition - path[index].FlatPosition).normalized;
                Vector3 targetDirectionOut = (path[index + 1].FlatPosition - path[index].FlatPosition).normalized;
    
                for (int i = 0; i < 6; i++)
                {
                    Vector3 directionIn = startDirection.RotateVector(Vector3.up, i * 60);
                    Vector3 directionOut = directionIn.RotateVector(Vector3.up, rotationOffset);
    
                    float inDot = Vector3.Dot(directionIn, targetDirectionIn);
                    float outDot = Vector3.Dot(directionOut, targetDirectionOut);
    
                    float altInDot = Vector3.Dot(directionIn, targetDirectionOut);
                    float altOutDot = Vector3.Dot(directionOut, targetDirectionIn);
    
                    bool validRotation = inDot > .9f && outDot > .9f;
                    bool altValidRotation = altInDot > .9f && altOutDot > .9f;
    
                    if (validRotation || altValidRotation)
                    {
                        return i * 60;
                    }
                }
    
                throw new Exception("Cant Get Rotation (Straight Piece)");
    
            }
    
            float GetPathBendRotation(List<Tile> path, int index)
            {
                float rotationOffset = 240;
                Vector3 startDirection = Vector3.right;
    
                Vector3 targetDirectionIn = (path[index - 1].FlatPosition - path[index].FlatPosition).normalized;
                Vector3 targetDirectionOut = (path[index + 1].FlatPosition - path[index].FlatPosition).normalized;
    
                for (int i = 0; i < 6; i++)
                {
                    Vector3 directionIn = startDirection.RotateVector(Vector3.up, i * 60);
                    Vector3 directionOut = directionIn.RotateVector(Vector3.up, rotationOffset);
    
                    float inDot = Vector3.Dot(directionIn, targetDirectionIn);
                    float outDot = Vector3.Dot(directionOut, targetDirectionOut);
    
                    float altInDot = Vector3.Dot(directionIn, targetDirectionOut);
                    float altOutDot = Vector3.Dot(directionOut, targetDirectionIn);
    
                    bool validRotation = inDot > .9f && outDot > .9f;
                    bool altValidRotation = altInDot > .9f && altOutDot > .9f;
    
                    if (validRotation || altValidRotation)
                    {
                        return i * 60;
                    }
                }
    
                throw new Exception("Cant Get Rotation (Bend Piece)");
            }
        }

        private List<Tile> LinkTiles(Dictionary<Vector2Int, Tile> tiles, Vector2Int maxPosition)
        {
            List<Tile> linkedtiles = new();
            foreach (KeyValuePair<Vector2Int, Tile> kvp in tiles)
            {
                GoThroughPossibleNeighbours(kvp.Key);
            }

            void GoThroughPossibleNeighbours(Vector2Int key)
            {
                if(!tiles.ContainsKey(key))
                {
                    return;
                }

                Tile tile = tiles[key];
                linkedtiles.Add(tile);

                bool uneven = key.y % 2 > 0;
                int offsetLeft = uneven ? key.x - 1 : key.x;
                int offsetRight = uneven ? key.x : key.x + 1;

                List<Tile> neighbours = new();
                if(tiles.TryGetValue(new(offsetLeft, key.y - 1), out Tile bottomLeft))
                {
                    neighbours.Add(bottomLeft);
                }

                if(tiles.TryGetValue(new(offsetRight, key.y - 1), out Tile bottomRight))
                {
                    neighbours.Add(bottomRight);
                }

                if(tiles.TryGetValue(new(key.x - 1, key.y), out Tile left))
                {
                    neighbours.Add(left);
                }

                if(tiles.TryGetValue(new(key.x + 1, key.y), out Tile right))
                {
                    neighbours.Add(right);
                }

                if(tiles.TryGetValue(new(offsetLeft, key.y + 1), out Tile topLeft))
                {
                    neighbours.Add(topLeft);
                }

                if(tiles.TryGetValue(new(offsetRight, key.y + 1), out Tile topRight))
                {
                    neighbours.Add(topRight);
                }

                tile.Neighbours = new(neighbours);
            }

            return linkedtiles;
        }

        private void SetVisualAttributes(Tile tile)
        {
            tile.TerrainType = GetTerrainType();
            tile.BoardType = GetBoardType();
            tile.GrassType = GetGrassType();
            tile.BridgeType = BridgeType.None;

            TerrainType GetTerrainType()
            {
                if (tile.IsWeak)
                {
                    return TerrainType.Weak;
                }

                if (tile.Neighbours.Any(n => n.IsWeak) || tile.IsPathTile.Value)
                {
                    return TerrainType.Top;
                }

                return TerrainType.Surface;
            }

            GrassType GetGrassType()
            {
                if (tile.IsPathTile.Value)
                {
                    return tile.GrassType;
                }

                return tile.BoardType != BoardType.None ? GrassType.Board : GrassType.Base;
            }

            BoardType GetBoardType()
            {
                if (tile.IsHeartTile)
                {
                    return BoardType.Metal;
                }

                if (tile.IsSegmentTile || tile.IsPathTile.Value || tile.IsStartTile)
                {
                    return BoardType.None;
                }

                return Random.value > .6f || tile.IsPathTile.Value ? BoardType.Stone : BoardType.None;
            }
        }

        private void CreateBossIslandSegments(Island island)
        {
            List<Tile> islandTiles = new(island.Tiles);
        
            Tile centerTile = islandTiles.GetTileClosestToPosition(island.StartTile.FlatPosition);
            SegmentView segmentDefinition = _segmentContainer.StartSegment;
            Segment startSegment = _segmentFactory.CreateSegment(segmentDefinition, centerTile);
        
            Tile tile = islandTiles.GetTileClosestToPosition(default);
            SegmentView definition = _segmentContainer.BossSegments.Random();
            Segment segment = _segmentFactory.CreateSegment(definition, tile);
        }
    }
}