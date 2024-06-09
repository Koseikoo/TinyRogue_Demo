using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class SimpleEnemy : Enemy
    {
        protected override void EnemyAction()
        {
            List<Tile> path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);

            if (path.Count == 0)
            {
                Tile closestTileToTarget = Tile.Value.GetNearestNeighbourTo(AttackTarget.Tile.Value);
                path.Add(closestTileToTarget);
            }

            if (path.Count <= AttackRange)
            {
                this.TryAttackTarget(path);
                AnimationCommand.Execute(AnimationState.Attack1);
            }
            else
            {
                this.FollowTarget(path[0]);
            }
        }
    }
}