using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class SimpleEnemy : Enemy
    {
        protected override void EnemyAction()
        {
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);
            
            if(path == null)
                return;

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