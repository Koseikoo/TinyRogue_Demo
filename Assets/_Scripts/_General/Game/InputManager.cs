using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using Zenject;

namespace Game
{
    public class InputManager
    {
        //[Inject] private PlayerFeedbackManager _playerFeedbackManager;
        [Inject] private PlayerManager _playerManager;

        private const float MinInputDistance = 1;
        
        public void ProcessInput()
        {
            InputHelper.ProcessSwipeInput();
            if (UIHelper.Camera == null || InputHelper.StartedOverUI || GameStateContainer.GameState.Value == GameState.Dead)
            {
                return;
            }

            bool canAct = GameStateContainer.TurnState.Value == TurnState.PlayerTurnStart;

            Vector3 swipeVector = UIHelper.Camera
                .GetWorldSwipeVector();
            
            List<Tile> aimedTiles = _playerManager.Player.Weapon.Value.GetAimedTiles(swipeVector);
            List<Tile> aimedUnitTiles = aimedTiles.Where(tile => tile.HasUnit).ToList();
            _playerManager.Player.AimedTiles = aimedTiles;
            

            bool hasAimedTiles = aimedTiles.Count > 0;
            bool isSwipeOverThreshold = swipeVector.magnitude > MinInputDistance;
            
            if (canAct && !TryCloseUIElements())
            {
                if (InputHelper.IsSwiping() && hasAimedTiles && isSwipeOverThreshold)
                {
                    _playerManager.Player.LookDirection.Value = swipeVector.normalized;
                    PlayerAction(swipeVector, aimedTiles, aimedUnitTiles);
                    GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
                }
                else if (InputHelper.TouchEnded())
                {
                    _playerManager.Player.SkippedTurns.Value++;
                    GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
                }
            }
        }
        
        private bool TryCloseUIElements()
        {
            bool hasOpenUI = GameStateContainer.OpenUI;
            if (hasOpenUI && !InputHelper.StartedOverUI)
            {
                GameStateContainer.CloseOpenUIElements.Execute();
            }

            return hasOpenUI;
        }

        public void PlayerAction(Vector3 swipeVector, List<Tile> aimedTiles, List<Tile> aimedUnitTiles)
        {
            bool unitInAimedTiles = aimedUnitTiles.Count > 0;
            Tile dashHitTarget = GetDashHitTarget(swipeVector);
            bool canDashHit = _playerManager.Player.Weapon.Value.CanDashHit && dashHitTarget != null;
            
            if (unitInAimedTiles || canDashHit)
            {
                if(canDashHit)
                {
                    DashHit(swipeVector, dashHitTarget);
                }
                else
                {
                    _playerManager.Player.StartAttack(aimedTiles.ToArray());
                }
            }
            else
            {
                aimedTiles[0].MoveUnitWithAction(_playerManager.Player);
            }
        }

        private Tile GetDashHitTarget(Vector3 direction)
        {
            List<Tile> tiles = direction.GetTilesInDirection(3);
            Tile attackTile = null;

            foreach (Tile tile in tiles)
            {
                if (tile.HasUnit)
                {
                    attackTile = tile;
                    break;
                }
            }
            return attackTile;
        }

        private void DashHit(Vector3 direction, Tile dashTarget)
        {
            Tile moveTile = (-direction).GetTileInDirection(dashTarget);
            moveTile.MoveUnit(_playerManager.Player);
            _playerManager.Player.StartAttack(new Tile[] {dashTarget});
        }
    }
}