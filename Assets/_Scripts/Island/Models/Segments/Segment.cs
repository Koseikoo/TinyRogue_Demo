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
        EnemyCamp,
        Village,
        Ruin,
        Start,
        End,
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
        public List<Unit> Units = new();
        private IDisposable _GameStateSubscription;

        public Segment(SegmentView definition, Tile centerTile)
        {
            Type = definition.Type;
            Size = definition.Size;
            CenterTile = centerTile;
        }

        protected virtual void CheckSegmentCompleteCondition()
        {
            var units = Units.FindAll(u => u is Enemy && !u.IsDead.Value);
            if (units.Count == 0)
            {
                IsCompleted.Value = true;
                _GameStateSubscription?.Dispose();
            }
        }

        public virtual void SegmentCompleteAction(Transform parent)
        {
            
        }

        public void AddUnit(Unit unit)
        {
            Units.Add(unit);
            
            _GameStateSubscription = unit.IsDead
                .Where(b => b)
                .Subscribe(_ => CheckSegmentCompleteCondition());
        }

        public void SetTiles(List<Tile> tiles)
        {
            Tiles = tiles;

            foreach (var tile in Tiles)
            {
                tile.IsSegmentTile = true;
            }
        }
    }
}