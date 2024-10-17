using System;
using System.Collections.Generic;
using Factories;
using UniRx;
using UnityEngine;
using Views;
using Zenject;

namespace Models
{
    public enum SegmentType
    {
        Forrest,
        WolfCamp,
        Village,
        Ruin,
        Start,
        MiniBoss,
        Boss
    }
    public class Segment
    {
        public BoolReactiveProperty IsDestroyed = new();
        public BoolReactiveProperty IsCompleted = new();
        public SegmentType Type;
        public Tile CenterTile;
        public int Size;

        public float Radius => Size * Island.TileDistance;

        public List<Tile> Tiles;
        public List<Tile> EntryTiles = new();
        public List<Tile> ExitTiles = new();
        public List<GameUnit> Units = new();


        public Segment(SegmentView definition, Tile centerTile)
        {
            Type = definition.Type;
            Size = definition.Size;
            CenterTile = centerTile;
        }

        public virtual void SegmentCompleteAction(Transform parent)
        {
            
        }

        public void AddUnit(GameUnit gameUnit)
        {
            Units.Add(gameUnit);
        }

        public void SetTiles(List<Tile> tiles)
        {
            Tiles = tiles;

            foreach (Tile tile in Tiles)
            {
                tile.IsSegmentTile = true;
            }
        }
    }
}