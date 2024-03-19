using System;
using System.Collections.Generic;
using Factories;
using Installer;
using UnityEngine;
using Models;
using Views;
using Zenject;

public class IslandInitializer
{
    private const int DirectionsToCheck = 40;
    
    private Island _island;

    [Inject] private TileActionContainer _tileActionContainer;
    
    [Inject] private SegmentFactory _segmentFactory;
    [Inject] private SegmentContainer _segmentContainer;
    public void Initialize(Island island, Vector3[] polygon)
    {
        _island = island;

        // Add Segments
        
        SetEdgeTiles(_island);
        SetStartTile(_island);
        SetEndTileFromStartTile(_island);
        
        List<Segment> segments = GetIslandSegments(island, polygon);
        island.AddSegments(segments);
    }
        
    private void SetPath(Tile start, Tile end)
    {
        var path = AStar.FindPath(start, end, unit => true);

        for (int i = 0; i < path.Count; i++)
        {
            path[i].IsPathTile.Value = true;
        }

        start.Island.PathTiles = path;
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
                    Vector3 checkPosition = currentSegments[i].CenterTile.WorldPosition + (checkDirections[j] * distance);
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

                        Tile previousCenterTile = currentSegments[i].CenterTile;
                        Tile newCenterTile = segment.CenterTile;
                        
                        SetPathGrass(previousCenterTile, newCenterTile);
                    }
                }
            }
    
            currentSegments.Clear();
            currentSegments.AddRange(new List<Segment>(newSegments));
            newSegments.Clear();
        }

        Tile start = spacedSegments[^1].CenterTile;
        Tile end = endSegment.CenterTile;
        
        SetPathGrass(start, end);

        return spacedSegments;
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
    
    private void SetPathGrass(Tile start, Tile end)
    {
        var path = AStar.FindPath(start, end, unit => true);
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
            path[k].IsPathTile.Value = true;
        }
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
        
    public void SetEdgeTiles(Island island)
    {
        List<Tile> edgeTiles = new();

        foreach (var tile in island.Tiles)
        {
            if(tile.IsEdgeTile)
                edgeTiles.Add(tile);
        }

        island.EdgeTiles = edgeTiles;
    }

    private void SetStartTile(Island island)
    {
        island.StartTile = island.EdgeTiles.PickRandom();
    }

    private void SetEndTileFromStartTile(Island island)
    {
        island.EndTile = island.StartTile.GetTileFurthestAway();
        island.EndTile.AddMoveToLogic(_tileActionContainer.IslandEndAction);
    }
}