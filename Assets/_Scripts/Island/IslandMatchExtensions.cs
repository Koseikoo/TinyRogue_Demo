using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;

public static class IslandMatchExtensions
{
    public static List<Tile> GetMatchingTiles(this List<Tile> tiles, Func<Tile, bool> conditionMet)
    {
        List<Tile> matchingTiles = new();
        foreach (Tile tile in tiles)
            if(conditionMet(tile))
            {
                matchingTiles.Add(tile);
            }

        return matchingTiles;
    }
    
    public static List<Tile> GetTilesWithinDistance(this List<Tile> tiles, Tile centerTile, int rangeInTiles)
    {
        float range = (rangeInTiles * Island.TileDistance) + Island.TileBuffer;

        return tiles.GetMatchingTiles(tile =>
        {
            float distance = (centerTile.FlatPosition - tile.FlatPosition).magnitude;
            return distance <= range;
        });
    }
    
    public static List<Tile> GetTilesOutsideOfDistance(this List<Tile> tiles, Tile centerTile, int rangeInTiles)
    {
        float range = (rangeInTiles * Island.TileDistance) + Island.TileBuffer;

        return tiles.GetMatchingTiles(tile =>
        {
            float distance = (centerTile.FlatPosition - tile.FlatPosition).magnitude;
            return distance > range;
        });
    }

    public static List<Tile> WithoutUnitOnTile(this List<Tile> tiles)
    {
        return tiles.GetMatchingTiles(tile => !tile.HasUnit);
    }
    
    public static List<Tile> WithUnitOnTile(this List<Tile> tiles)
    {
        return tiles.GetMatchingTiles(tile => tile.HasUnit);
    }
    
    public static List<Tile> WithEnemyOnTile(this List<Tile> tiles)
    {
        return tiles.GetMatchingTiles(tile => tile.HasUnit && tile.Unit.Value is Enemy);
    }

    public static List<Tile> GetEdgeTiles(this List<Tile> tiles)
    {
        return tiles.GetMatchingTiles(tile => tile.IsEdgeTile);
    }

    public static Tile GetFirstEmptyTile(this Tile tile)
    {
        Tile emptyTile = null;
        Tile currentCenterTile = tile;
        List<Tile> checkedCenterTiles = new();
        
        while (emptyTile == null)
        {
            emptyTile = currentCenterTile.Neighbours.FirstOrDefault(t => !t.HasUnit);
            checkedCenterTiles.Add(currentCenterTile);
            currentCenterTile = currentCenterTile.Neighbours.First(t => !checkedCenterTiles.Contains(t));
        }

        return emptyTile;
    }

    public static bool IsBehind(this Tile a, Tile b)
    {
        bool result = false;
        Tile playerTile = GameStateContainer.Player.Tile.Value;

        Vector3 pa = a.FlatPosition - playerTile.FlatPosition;
        Vector3 pb = b.FlatPosition - playerTile.FlatPosition;

        float normalizedDot = Vector3.Dot(pa.normalized, pb.normalized);
        if (normalizedDot > .8f && pa.magnitude > pb.magnitude)
        {
            result = true;
        }

        return result;
    }

    public static List<Tile> GetSwipedTiles(this List<Tile> tiles, Tile startTile, Tile endTile)
    {
        List<Tile> tilesToAttack = new List<Tile>();
        Vector3 swipeVector = endTile.FlatPosition - startTile.FlatPosition;
        foreach (Tile tile in tiles)
        {
            Vector3 closestPoint =
                MathHelper.GetClosestPointOnLine(startTile.FlatPosition, startTile.FlatPosition + swipeVector,
                    tile.FlatPosition);

            bool swipedOverTile = Vector3.Distance(closestPoint, tile.FlatPosition) <= Island.HalfTileDistance + .1f;
            if (swipedOverTile)
            {
                tilesToAttack.Add(tile);
            }
        }

        tilesToAttack = tilesToAttack.OrderByDistanceToPosition(startTile.FlatPosition);
        return tilesToAttack;
    }
}