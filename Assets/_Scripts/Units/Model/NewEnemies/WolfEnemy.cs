using System.Collections.Generic;
using UnityEngine;

namespace Models
{
    public class WolfEnemy : Enemy
    {
        private const int LeapTurnDelay = 2;
        private const int BaseTurnDelay = 0;
        
        private List<Tile> _leapPath = new();
        private List<Tile> _lastLeapPath = new();
        
        protected override void EnemyAction()
        {
            if (TurnDelay == LeapTurnDelay)
                TurnDelay = BaseTurnDelay;
            
            var path = AStar.FindPath(Tile.Value,
                AttackTarget.Tile.Value, 
                IsAttackPathTile);

            if (_leapPath.Count > 0)
            {
                Leap(_leapPath);
                _leapPath.Clear();
                
            }
            else if(path.Count != 0)
            {
                this.FollowTarget(path[0]);
            }
        }
        
        protected override bool IsAttackPathTile(Tile tile)
        {
            var tileUnit = tile.CurrentUnit.Value;
            if (tile.HasUnit && (tileUnit.IsInvincible.Value || 
                                 tileUnit is Enemy || 
                                 tileUnit.Type == UnitType.CampWall || 
                                 tileUnit.Type == UnitType.CampFire))
                return false;
            return true;
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

                _leapPath = path;
                Debug.Log("assign leap path");
            }
            else if(!path[0].HasUnit)
            {
                UpdateSelectedTiles(new(){path[0]}, TileSelectionType.Move);
                NextMoveTile = path[0];
            }
        }

        private void Leap(List<Tile> path)
        {
            int jumpIndex = path.Count - 1;
            bool attack = false;

            for (int i = path.Count - 1; i >= 0; i--)
            {
                if (path[i].CurrentUnit.Value == AttackTarget)
                {
                    jumpIndex = i - 1;
                    attack = true;
                }
            }
            
            if (jumpIndex >= 0 && !path[jumpIndex].HasUnit)
            {
                path[jumpIndex].MoveUnit(this);
            }
            
            if(attack)
                this.AttackUnit(AttackTarget);
            
            TurnDelay = LeapTurnDelay;
            AimAtTarget.Value = false;
        }
    }
}