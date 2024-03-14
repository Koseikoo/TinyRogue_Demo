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
        
        // PolygonConfig's Hexagon Size * 1.5
        //public const float TileDistance = 1.212f;
        public const float TileDistance = 1.4f;
        public const float HalfTileDistance = TileDistance * .5f;
        public const float TileBuffer = HalfTileDistance / 3;

        public int Level { get; private set; }
        
        public List<Tile> Tiles = new();
        public List<Tile> EdgeTiles = new();
        public List<Tile> PathTiles = new();
        public List<Segment> Segments = new();

        public Tile StartTile;
        public Tile EndTile;
        public ReactiveCollection<Unit> Units { get; private set; } = new();
        public BoolReactiveProperty IsDestroyed = new();
        public BoolReactiveProperty EndTileUnlocked = new();

        public Island(List<Tile> islandTiles, int level)
        {
            Tiles = islandTiles;
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

        public void AddSegments(List<Segment> segments)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                var tiles = this.GetSegmentTiles(segments[i]);
                segments[i].SetTiles(tiles);
            }
            
            Segments = segments;
        }

        public void RemoveUnit(Unit unit)
        {
            Units.Remove(unit);
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

        public Tile GetTappedTile(Camera camera, Vector2 screenPosition)
        {
            Vector3 intersectionPoint = camera.GetExtendedPositionFromCamera(screenPosition);
            return Tiles.GetClosestTileFromPosition(intersectionPoint);
        }
    }
}