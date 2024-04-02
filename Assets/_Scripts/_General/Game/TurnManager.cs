using System.Collections;
using UnityEngine;

namespace Game
{
    public class TurnManager
    {
        public const float PlayerTurnDuration = .15f;
        public const float EnemyTurnDuration = .2f;
        public const float IslandTurnDuration = .2f;

        public void StartTurn(MonoBehaviour behaviour)
        {
            behaviour.StartCoroutine(Turn());
        }
        
        private IEnumerator Turn()
        {
            while (true)
            {
                yield return new WaitUntil(() => GameStateContainer.TurnState.Value == TurnState.PlayerTurnEnd && !GameStateContainer.Player.Weapon.InAttack);
                
                yield return new WaitForSeconds(PlayerTurnDuration);
                GameStateContainer.TurnState.Value = TurnState.EnemyTurn;
                yield return new WaitForSeconds(EnemyTurnDuration);
                GameStateContainer.TurnState.Value = TurnState.IslandTurn;
                yield return new WaitForSeconds(IslandTurnDuration);
                GameStateContainer.TurnState.Value = TurnState.PlayerTurnStart;
            }
        }
    }
}