using System;
using System.Collections.Generic;
using Factories;
using UniRx;
using UnityEngine;

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
        public const float SegmentDistance = 2f;
        
        public BoolReactiveProperty IsDestroyed = new();
        public BoolReactiveProperty IsCompleted = new();
        public SegmentType Type;
        public Vector3 Position;
        public float Radius;
        public int MaxUnits;

        public List<Tile> Tiles;
        public List<SegmentUnit> Units = new();

        private IDisposable _GameStateSubscription;

        public Segment(SegmentDefinition definition, Vector3 position = default)
        {
            Type = definition.Type;
            Radius = definition.Radius;
            MaxUnits = definition.MaxUnits;
            Position = position;
        }

        protected virtual void CheckSegmentCompleteCondition()
        {
            var units = Units.FindAll(u => u.TrackUnit && u?.Unit != null && !u.Unit.IsDead.Value);
            if (units.Count == 0)
            {
                IsCompleted.Value = true;
                _GameStateSubscription?.Dispose();
            }
        }

        public virtual void SegmentCompleteAction(Transform parent)
        {
            
        }

        public void AddUnit(Unit unit, bool trackUnit = false, List<Tile> unitTiles = null)
        {
            Units.Add(new(unit, unitTiles, trackUnit));
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
                if (Vector3.Distance(segments[i].Position, Position) < segments[i].Radius + Radius + SegmentDistance)
                    return true;
            }
            return false;
        }
    }
}