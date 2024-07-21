using System.Collections.Generic;
using System.Linq;
using Models;
using Unity.VisualScripting;
using UnityEngine;
using Zenject;
using Unit = Models.Unit;

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
                    PlayerAction(aimedTiles, aimedUnitTiles);
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

        public void PlayerAction(List<Tile> aimedTiles, List<Tile> aimedUnitTiles)
        {
            bool unitInAimedTiles = aimedUnitTiles.Count > 0;
            if (unitInAimedTiles)
            {
                // Attack
                //_playerManager.Player.AttackTiles(aimedTiles.ToArray());
                _playerManager.Player.StartAttack(aimedTiles.ToArray());
                //_playerFeedbackManager.RenderAttackLine(aimedTiles);
            }
            else
            {
                // Move
                aimedTiles[0].MoveUnitWithAction(_playerManager.Player);
            }
        }
    }
}