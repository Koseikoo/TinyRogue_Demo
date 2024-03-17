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
        public Tile Tile;
        public int Size;

        public float Radius => Size * Island.TileDistance;

        public List<Tile> Tiles;
        public List<Unit> Units = new();
        private IDisposable _GameStateSubscription;

        public Segment(SegmentView definition, Tile tile)
        {
            Type = definition.Type;
            Size = definition.Size;
            Tile = tile;
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
        }

        public void SetTiles(List<Tile> tiles)
        {
            Tiles = tiles;

            foreach (var tile in Tiles)
            {
                tile.IsSegmentTile = true;
            }

            _GameStateSubscription = tiles[0].Island.Units
                .ObserveRemove()
                .Subscribe(_ => CheckSegmentCompleteCondition());
        }

        public bool IsWithinPolygon(Vector3[] polygon, Vector3 position)
        {
            int numPointsOnCircle = 16;

            for (int i = 0; i < numPointsOnCircle; i++)
            {
                float angle = 2 * Mathf.PI * i / numPointsOnCircle;
                float x = position.x + Radius * Mathf.Cos(angle);
                float z = position.z + Radius * Mathf.Sin(angle);

                Vector3 pointOnCircle = new Vector3(x, position.y, z);
                if (!pointOnCircle.IsInsidePolygon(polygon))
                    return false;
            }
            return true;
        }

        public bool IsInsideSegment(List<Segment> segments)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                if (Vector3.Distance(segments[i].Tile.WorldPosition, Tile.WorldPosition) < segments[i].Radius.GetSegmentDistance(Radius))
                    return true;
            }
            return false;
        }
    }
}