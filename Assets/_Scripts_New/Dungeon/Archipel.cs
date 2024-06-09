using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;

namespace TinyRogue
{
    public class Archipel
    {
        public List<Island> Islands;
        public List<IslandConnection> Connections;
        public Bounds Bounds;

        public Island StartIsland;
        public Tile StartTile;
        public Island EndIsland;
        public Tile EndTile;
        
        public Archipel(Bounds bounds, List<Island> islands, List<IslandConnection> connections)
        {
            Bounds = bounds;
            Islands = islands;
            Connections = connections;
            
            StartIsland = Islands
                .OrderBy(island => Vector3.Distance(island.Bounds.center, Bounds.center))
                .Last();
            EndIsland = Islands
                .OrderBy(island => Vector3.Distance(island.Bounds.center, StartIsland.Bounds.center))
                .Last();
        }

        public void SetStartAndEndTile()
        {
            StartTile = StartIsland.Tiles
                .OrderBy(tile => Vector3.Distance(tile.FlatPosition, Bounds.center))
                .Last();
            EndTile = EndIsland.Tiles
                .OrderBy(tile => Vector3.Distance(tile.FlatPosition, Bounds.center))
                .Last();
        }
    }
}