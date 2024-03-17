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
            float distance = (centerTile.WorldPosition - tile.WorldPosition).magnitude;
            return distance <= range;
        });
    }
    
    public static List<Tile> GetTilesOutsideOfDistance(this List<Tile> tiles, Tile centerTile, int rangeInTiles)
    {
        float range = (rangeInTiles * Island.TileDistance) + Island.TileBuffer;

        return centerTile.Island.Tiles.GetMatchingTiles(tile =>
        {
            float distance = (centerTile.WorldPosition - tile.WorldPosition).magnitude;
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
        return tiles.GetMatchingTiles(tile => tile.HasUnit && tile.CurrentUnit.Value is Enemy);
    }

    public static List<Tile> GetEdgeTiles(this List<Tile> tiles)
    {
        return tiles.GetMatchingTiles(tile => tile.IsEdgeTile);
    }
    
    public static List<Tile> GetSwipedTiles(this List<Tile> tiles, Tile startTile, Tile endTile)
    {
        List<Tile> tilesToAttack = new List<Tile>();
        Vector3 swipeVector = endTile.WorldPosition - startTile.WorldPosition;
        foreach (Tile tile in tiles)
        {
            Vector3 closestPoint =
                MathHelper.GetClosestPointOnLine(startTile.WorldPosition, startTile.WorldPosition + swipeVector, tile.WorldPosition);

            bool swipedOverTile = Vector3.Distance(closestPoint, tile.WorldPosition) <= Island.HalfTileDistance + .1f;
            if(swipedOverTile)
                tilesToAttack.Add(tile);
        }

        tilesToAttack = tilesToAttack.OrderByDistanceToPosition(startTile.WorldPosition);
        return tilesToAttack;
    }
    
    public static List<Tile> GetTilesInWeaponRange(this List<Tile> tiles, Weapon weapon)
    {
        float maxDistanceToPlayer = (Island.TileDistance * weapon.Range) + Island.TileBuffer;
        
        return tiles.GetMatchingTiles(tile =>
        {
            var distance = Vector3.Distance(weapon.Tile.Value.WorldPosition, tile.WorldPosition);
            return distance <= maxDistanceToPlayer;
        });
    }
}