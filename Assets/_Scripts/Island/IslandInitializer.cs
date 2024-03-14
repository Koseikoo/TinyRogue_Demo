using System.Collections.Generic;
using UnityEngine;
using Models;
using Zenject;

public class IslandInitializer
{
    private Island _island;

    [Inject] private TileActionContainer _tileActionContainer;
    public void Initialize(Island island)
    {
        _island = island;

        SetEdgeTiles(_island);
        SetStartTile(_island);
        SetEndTileFromStartTile(_island);
    }
        
    private void SetPath(Tile start, Tile end)
    {
        var path = AStar.FindPath(start, end, unit => true);

        for (int i = 0; i < path.Count; i++)
        {
            path[i].PathTile.Value = true;
        }

        start.Island.PathTiles = path;
    }
        
    private void SetEdgeTiles(Island island)
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