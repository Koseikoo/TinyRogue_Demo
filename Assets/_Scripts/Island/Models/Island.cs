using System;
using System.Collections.Generic;
using System.Linq;
using TinyRogue;
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

        public Bounds Bounds;
        public List<Vector3> Outline;
        public float Size;

        public List<(Tile start, Tile end)> Connections = new();
        
        public List<Tile> Tiles = new();
        public List<Tile> EdgeTiles = new();
        public List<Tile> PathTiles = new();
        public List<Segment> Segments = new();

        public Tile StartTile;
        public Tile EndTile;
        public Tile HeartTile;
        public ReactiveCollection<GameUnit> Units { get; private set; } = new();
        public BoolReactiveProperty IsDestroyed = new();
        public BoolReactiveProperty IsHeartDestroyed = new();

        public ReactiveCommand DissolveIslandCommand = new();

        public Vector3 IslandShipPosition;

        public int EnemiesOnIsland => Units.Count(unit => unit is Enemy);

        private bool isCompleted;

        public Island(float size, Bounds bounds, List<Vector3> outline)
        {
            Size = size;
            Bounds = bounds;
            Outline = outline;
        }
        
        public void AddConnection(Tile start, Tile end)
        {
            Connections.Add((start, end));
            start.AddMoveToLogic(unit =>
            {
                if (EnemiesOnIsland == 0)
                {
                    end.MoveUnit(unit);
                }
            });
        }

        public void AddCompletionLogic(Func<Island, bool> completionCondition, Action<Island> onComplete)
        {
            Units.ObserveRemove()
                .Where(_ => completionCondition(this) && !isCompleted)
                .Subscribe(_ =>
                {
                    onComplete(this);
                    isCompleted = true;
                });
        }

        public void AddUnit(GameUnit gameUnit)
        {
            Units.Add(gameUnit);
        }

        public void RemoveUnit(GameUnit gameUnit)
        {
            Units.Remove(gameUnit);
        }

        public void DestroyTile(Tile tile)
        {
            Tiles.Remove(tile);
            foreach (Tile n in tile.Neighbours)
                n.Neighbours.Remove(tile);

            if (tile.HasUnit)
            {
                if (tile.Unit.Value is Player player)
                {
                    player.Damage(player.Health.Value, null, true);
                }
                else
                {
                    tile.Unit.Value.IsDestroyed.Value = true;
                }
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
    }
}