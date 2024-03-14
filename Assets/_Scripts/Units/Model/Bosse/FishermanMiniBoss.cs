using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Models
{
    public class FishermanMiniBoss : Enemy
    {
        // Drops Poison Mod
        private const int StabRange = 2;
        private const int FireRange = 5;
        private const int FireReloadTurns = 2;

        private int _currentReloadState;
        private bool _stabAim;
        private bool _fireAim;
        protected override void EnemyAction()
        {
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);
            
            if(path == null)
                return;
            
            _currentReloadState = Mathf.Max(0, _currentReloadState - 1);
            if (path.Count <= FireRange && _currentReloadState == 0 && _fireAim)
            {
                FireProjectile();
                _fireAim = false;
                _currentReloadState = FireReloadTurns;
            }
            else if (path.Count <= StabRange && _stabAim)
            {
                StabAttack();
                _stabAim = false;
            }
            else
            {
                this.FollowTarget(path[0]);
            }
        }

        protected override void RenderAttackPath()
        {
            if(!AimAtTarget.Value)
                return;

            List<Tile> path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value);

            if(path == null)
                return;
            
            if (path.Count <= FireRange && _currentReloadState <= 1)
            {
                path = GetAimedRangedTiles();
                UpdateSelectedTiles(path, TileSelectionType.Aim);
                _fireAim = true;
            }
            else if (path.Count <= StabRange)
            {
                UpdateSelectedTiles(path, TileSelectionType.Attack);
                _stabAim = true;
            }
            else if(!path[0].HasUnit)
            {
                UpdateSelectedTiles(new(){path[0]}, TileSelectionType.Move);
                NextMoveTile = path[0];
            }
        }

        private void StabAttack()
        {
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);
            
            if(path == null)
                return;

            if (path.Count <= StabRange)
            {
                this.TryAttackTarget(path);
                AnimationCommand.Execute(AnimationState.Attack1);
            }
        }

        private void FireProjectile()
        {
            var path = GetAimedRangedTiles();
            
            if(path == null || path.Count == 0 || !path.HasUnit(AttackTarget))
                return;

            this.TryAttackTarget(path);
            AnimationCommand.Execute(AnimationState.Attack2);
            CurrentTurnDelay.Value = FireReloadTurns;
        }

        private List<Tile> GetAimedRangedTiles()
        {
            var targetDirection = GameStateContainer.Player.Tile.Value.WorldPosition - Tile.Value.WorldPosition;
            var direction = Tile.Value.Neighbours[0].WorldPosition - Tile.Value.WorldPosition;
            
            for (int i = 1; i < Tile.Value.Neighbours.Count; i++)
            {
                var newDirection = Tile.Value.Neighbours[i].WorldPosition - Tile.Value.WorldPosition;
                var lastDot = Vector3.Dot(direction.normalized, targetDirection);
                var nextDot = Vector3.Dot(newDirection, targetDirection);

                if (nextDot > lastDot)
                    direction = newDirection;
            }

            List<Tile> aimedTiles = new();
            var currentTile = Tile.Value;

            for (int i = 1; i < FireRange; i++)
            {
                var position = currentTile.WorldPosition + (direction * i);
                var nextPosition = currentTile.WorldPosition + (direction.RotateVector(Vector3.up, 60) * i);
                var previousPosition = currentTile.WorldPosition + (direction.RotateVector(Vector3.up, -60) * i);
                List<Tile> tiles = new()
                {
                    Tile.Value.Island.Tiles.GetTileClosestToPosition(position),
                    Tile.Value.Island.Tiles.GetTileClosestToPosition(nextPosition),
                    Tile.Value.Island.Tiles.GetTileClosestToPosition(previousPosition)
                };

                var tilesNonNull = tiles.Where(t => t != null);
                aimedTiles.AddRange(tilesNonNull);
            }

            return aimedTiles;
        }
    }
}