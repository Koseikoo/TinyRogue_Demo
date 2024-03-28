using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

public static class IslandMatchExtensions
{
    public static List<Tile> GetMatchingTiles(this List<Tile> tiles, Func<Tile, bool> conditionMet)
    {
        List<Tile> matchingTiles = new();
        foreach (Tile tile in tiles)
            if(conditionMet(tile))
                matchingTiles.Add(tile);
        
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
    
    public static List<Tile> GetSwipedTiles(this List<Tile> tiles, Tile startTile, Tile endTile)
    {
        List<Tile> tilesToAttack = new List<Tile>();
        Vector3 swipeVector = endTile.FlatPosition - startTile.FlatPosition;
        foreach (Tile tile in tiles)
        {
            Vector3 closestPoint =
                MathHelper.GetClosestPointOnLine(startTile.FlatPosition, startTile.FlatPosition + swipeVector, tile.FlatPosition);

            bool swipedOverTile = Vector3.Distance(closestPoint, tile.FlatPosition) <= Island.HalfTileDistance + .1f;
            if(swipedOverTile)
                tilesToAttack.Add(tile);
        }

        tilesToAttack = tilesToAttack.OrderByDistanceToPosition(startTile.FlatPosition);
        return tilesToAttack;
    }
    
    public static List<Tile> GetTilesInWeaponRange(this List<Tile> tiles, Weapon weapon)
    {
        float maxDistanceToPlayer = (Island.TileDistance * weapon.Range) + Island.TileBuffer;
        
        return tiles.GetMatchingTiles(tile =>
        {
            var distance = Vector3.Distance(weapon.Tile.Value.FlatPosition, tile.FlatPosition);
            return distance <= maxDistanceToPlayer;
        });
    }
}