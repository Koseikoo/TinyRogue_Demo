using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class GolemEnemy : Enemy
    {
        private List<Tile> _attackPath = new();
        protected override void EnemyAction()
        {
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);
            
            if(path == null)
                return;

            if (path.Count <= AttackRange)
            {
                this.TryAttackTarget(path, true);
                AnimationCommand.Execute(AnimationState.Attack1);
            }
            else
            {
                this.FollowTarget(path[0]);
            }
        }

        protected override bool IsAttackPathTile(Tile tile)
        {
            bool isAttackPathTile = base.IsAttackPathTile(tile);
            var tileUnit = tile.CurrentUnit.Value;
            if (isAttackPathTile && tileUnit?.Type != UnitType.Pillar)
                return true;
            return false;
        }

        protected override void RenderAttackPath()
        {
            if(!AimAtTarget.Value)
                return;
            
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value);
            
            if(path == null)
                return;
            
            if (path.Count <= AttackRange)
            {
                UpdateSelectedTiles(path, TileSelectionType.Attack);
            }
            else if(!path[0].HasUnit)
            {
                UpdateSelectedTiles(new(){path[0]}, TileSelectionType.Move);
                NextMoveTile = path[0];
            }
            
            //if(_attackPath.Count == 0)
            //    return;
            //
            //if (_attackPath.Count <= AttackRange)
            //{
            //    UpdateSelectedTiles(_attackPath, TileSelectionType.Attack);
            //}
            //else if(!_attackPath[0].HasUnit)
            //{
            //    UpdateSelectedTiles(new(){_attackPath[0]}, TileSelectionType.Move);
            //    NextMoveTile = _attackPath[0];
            //}
        }
    }
}