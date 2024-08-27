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
    public static Tile GetTileInDirection(Tile tile, Vector3 direction)
    {
        int index = 0;
        float lastDot = -1;
        float dot;

        for (int i = 0; i < tile.Neighbours.Count; i++)
        {
            Vector3 tileDir = (tile.Neighbours[i].FlatPosition - tile.FlatPosition).normalized;
            dot = Vector3.Dot(tileDir, direction);
            if(dot > lastDot)
            {
                lastDot = dot;
                index = i;
            }
        }

        return tile.Neighbours[index];
    }

    public static float GetSegmentDistance(this float baseSegmentRadius, float segmentRadius)
    {
        return baseSegmentRadius + segmentRadius + (Island.TileDistance * 2);
    }

    public static Tile GetTileInDirection(this Vector3 direction, Tile startTile = null)
    {
        List<Tile> tilesInDirection = direction.GetTilesInDirection(1, startTile);
        return tilesInDirection.Count > 0 ? tilesInDirection[0] : null;
    }
    
    /// <summary>
    /// Does not Include start Tile
    /// </summary>
    public static List<Tile> GetTilesInDirection(this Vector3 direction, int range, Tile startTile = null)
    {
        startTile ??= GameStateContainer.Player.Tile.Value;
        Tile currentTile = startTile;
        direction.Normalize();

        List<Tile> tiles = new();

        for (int i = 0; i < range; i++)
        {
            Tile next = GetTileInDirection(currentTile, direction);
            
            if (next == null || tiles.Contains(next))
            {
                break;
            }

            float dot = Vector3.Dot(direction, (next.FlatPosition - currentTile.FlatPosition).normalized);
            if (dot > .8f)
            {
                tiles.Add(next);
                currentTile = next;
            }
        }

        return tiles;
    }

    public static Tile GetNearestNeighbourTo(this Tile currentTile, Tile targetTile)
    {
        Tile closest = currentTile.Neighbours.OrderBy(tile => tile.DistanceTo(targetTile)).First();
        return closest;
    }
    
    /// <summary>
    /// Distance in Tiles
    /// </summary>
    public static int DistanceTo(this Tile startTile, Tile endTile)
    {
        Tile currentTile = startTile;
        int distance = 0;

        while (currentTile != endTile)
        {
            Tile nextTile = currentTile.Neighbours
                .OrderBy(tile => Vector3.Distance(tile.FlatPosition, endTile.FlatPosition))
                .First();
            currentTile = nextTile;
            distance++;
        }

        return distance;
    }
    
    public static Vector3 GetRelativeDirection(this Tile startTile, Tile targetTile)
    {
        return (targetTile.FlatPosition - startTile.FlatPosition).normalized;
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
        {
            throw new Exception("No Closest Tile Found");
        }

        return closest;
    }

    public static Segment GetClosestSegment(this List<Segment> segments, Tile tile)
    {
        return segments.OrderBy(s => Vector3.Distance(s.CenterTile.FlatPosition, tile.FlatPosition)).First();
    }

    public static Vector3 GetExtendedPositionFromCamera(this Camera camera, Vector2 screenPosition)
    {
        Vector3 worldPosition =
            camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, camera.nearClipPlane));
            
        Vector3 startPosition = camera.transform.position;
        Vector3 direction = (worldPosition - startPosition).normalized;
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

    public static Vector3 GetWorldSwipeVector(this Camera camera)
    {
        Vector3 startWorldPosition = camera.GetExtendedPositionFromCamera(InputHelper.StartPosition);
        Vector3 touchWorldPosition = camera.GetExtendedPositionFromCamera(InputHelper.GetTouchPosition());

        return touchWorldPosition - startWorldPosition;
    }

    public static List<Tile> FilterForBounceBack(this List<Tile> orderedTiles)
    {
        List<Tile> filteredForBounceBack = new();
        for (int i = 0; i < orderedTiles.Count; i++)
        {
            filteredForBounceBack.Add(orderedTiles[i]);
            if (orderedTiles[i].AttackBouncesFromTile)
            {
                break;
            }
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
        {
            return null;
        }

        return matches[0];
    }

    public static Tile GetTileFurthestAway(this List<Tile> tiles, Tile referenceTile)
    {
        List<Tile> orderedTiles = tiles.OrderByDistanceToPosition(referenceTile.FlatPosition);
        return orderedTiles[^1];
    }
}