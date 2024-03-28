using System;
using System.Collections.Generic;
using System.Linq;
using Installer;
using Models;
using UnityEditor.Rendering;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public static class IslandHelper
{
    public static Tile GetNeighbourFromDirection(Tile center, Vector3 direction)
    {
        int index = 0;
        float lastDot = -1;
        float dot;

        for (int i = 0; i < center.Neighbours.Count; i++)
        {
            Vector3 tileDir = (center.Neighbours[i].FlatPosition - center.FlatPosition).normalized;
            dot = Vector3.Dot(tileDir, direction);
            if(dot > lastDot)
            {
                lastDot = dot;
                index = i;
            }
        }

        return center.Neighbours[index];
    }

    public static Tile GetMaxRangeTile(this Island island, Tile startTile, Vector3 touchPosition)
    {
        return default;
    }

    public static float GetSegmentDistance(this float baseSegmentRadius, float segmentRadius)
    {
        return baseSegmentRadius + segmentRadius + (Island.TileDistance * 2);
    }

    public static Tile GetClosestTileFromPosition(this List<Tile> tiles, Vector3 worldPosition)
    {
        Tile closest = null;
        float closestDistance = 999;

        foreach (Tile tile in tiles)
        {
            float distance = (worldPosition - tile.FlatPosition).magnitude;
            if (closest == null || distance < closestDistance)
            {
                closest = tile;
                closestDistance = distance;
            }
        }

        if (closest == null)
            throw new Exception("No Closest Tile Found");

        return closest;
    }

    public static Segment GetClosestSegment(this List<Segment> segments, Tile tile)
    {
        return segments.OrderBy(s => Vector3.Distance(s.CenterTile.FlatPosition, tile.FlatPosition)).First();
    }

    public static Vector3 GetExtendedPositionFromCamera(this Camera camera, Vector2 screenPosition)
    {
        var worldPosition =
            camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane));
            
        var startPosition = camera.transform.position;
        var direction = (worldPosition - startPosition).normalized;
        float t = -startPosition.y / direction.y;
        return startPosition + t * direction;
    }

    public static Vector3 GetAverageDirection(this Tile tile)
    {
        Vector3 mean = default;

        foreach (Tile n in tile.Neighbours)
        {
            mean += (n.FlatPosition - tile.FlatPosition).normalized;
        }

        Vector3 average = mean / tile.Neighbours.Count;
        return average;
    }

    public static Vector3 GetWorldSwipeVector(this Camera camera, Vector2 startPosition, Vector2 touchPosition)
    {
        var startWorldPosition = camera.GetExtendedPositionFromCamera(startPosition);
        var touchWorldPosition = camera.GetExtendedPositionFromCamera(touchPosition);

        return touchWorldPosition - startWorldPosition;
    }

    public static List<Tile> FilterForBounceBack(this List<Tile> orderedTiles)
    {
        List<Tile> filteredForBounceBack = new();
        for (int i = 0; i < orderedTiles.Count; i++)
        {
            filteredForBounceBack.Add(orderedTiles[i]);
            if (orderedTiles[i].AttackBouncesFromTile)
                break;
        }

        return filteredForBounceBack;
    }

    public static Vector3 ShortenToTileRange(this Vector3 swipeVector, int range)
    {
        return Vector3.ClampMagnitude(swipeVector, range * Island.TileDistance);
    }

    public static List<Tile> OrderByDistanceToPosition(this List<Tile> tiles, Vector3 referencePosition)
    {
        List<Tile> orderedTiles = tiles.OrderBy(tile => Vector3.Distance(tile.FlatPosition, referencePosition)).ToList();
        return orderedTiles;
    }

    public static List<Tile> GetSegmentTiles(this List<Tile> tiles, Segment segment)
    {
        return tiles.GetMatchingTiles(tile =>
            Vector3.Distance(tile.FlatPosition, segment.CenterTile.FlatPosition) < segment.Radius + Island.TileBuffer);
    }
    
    public static Tile GetTileClosestToPosition(this List<Tile> tiles, Vector3 position)
    {
        List<Tile> matches = tiles.GetMatchingTiles(tile => Vector3.Distance(tile.FlatPosition, position) < Island.HexagonSize);
        if (matches == null || matches.Count == 0)
            return null;
        return matches[0];
    }

    public static Tile GetTileFurthestAway(this List<Tile> tiles, Tile referenceTile)
    {
        var orderedTiles = tiles.OrderByDistanceToPosition(referenceTile.FlatPosition);
        return orderedTiles[^1];
    }
}