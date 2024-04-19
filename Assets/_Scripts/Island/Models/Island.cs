using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Models
{
    public class Island
    {
        public const float HexagonSize = .808f;
        public const float ZDistance = HexagonSize * 1.5f;
        public const float XDistance = .7f * 2;
        
        public const float TileDistance = 1.4f;
        public const float HalfTileDistance = TileDistance * .5f;
        public const float TileBuffer = HalfTileDistance / 3;

        public int Level { get; private set; }
        
        public List<Tile> Tiles = new();
        public List<Tile> EdgeTiles = new();
        public List<Tile> PathTiles = new();
        public List<Segment> Segments = new();

        public Tile StartTile;
        public Tile HeartTile;
        public ReactiveCollection<Unit> Units { get; private set; } = new();
        public BoolReactiveProperty IsDestroyed = new();
        public BoolReactiveProperty IsHeartDestroyed = new();

        public ReactiveCommand DissolveIslandCommand = new();

        public Vector3 IslandShipPosition;
        
        public Island(List<Tile> islandTiles, Tile startTile, Tile heartTile, List<Segment> segments, int level)
        {
            Tiles = islandTiles;
            StartTile = startTile;
            HeartTile = heartTile;
            Segments = segments ?? new();
            Level = level;
            
            LinkTiles();
        }
        
        private void LinkTiles()
        {
            foreach (var tile in Tiles)
                tile.SetIsland(this);
        }

        public void AddUnit(Unit unit)
        {
            Units.Add(unit);
        }

        public void RemoveUnit(Unit unit)
        {
            Units.Remove(unit);
        }

        public void DestroyTile(Tile tile)
        {
            Tiles.Remove(tile);
            foreach (Tile n in tile.Neighbours)
                n.Neighbours.Remove(tile);

            if (tile.HasUnit)
            {
                if (tile.Unit.Value is Player player)
                    player.Damage(player.Health.Value, null, true);
                else
                    tile.Unit.Value.IsDestroyed.Value = true;
            }
            tile.Destroyed.Value = true;
            
            
        }

        public void DestroyIslandContent()
        {
            for (int i = Units.Count - 1; i >= 0; i--)
            {
                Units[i].IsDestroyed.Value = true;
            }

            for (int i = Segments.Count - 1; i >= 0; i--)
            {
                Segments[i].IsDestroyed.Value = true;
            }
        }

        public class B
        {
        }

        public class C
        {
            public int number;
            public void methode(){
                int newNumber = 1;
                number = newNumber;
                B newClass = new();
            }
        }
    }
}