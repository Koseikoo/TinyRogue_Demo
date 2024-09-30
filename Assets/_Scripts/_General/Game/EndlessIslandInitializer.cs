using System;
using TinyRogue;
using UnityEngine;
using Zenject;

namespace Game
{
    public class EndlessIslandInitializer : MonoBehaviour
    {
        [Inject] private GameSetup _gameSetup;
        [Inject] private TurnManager _turnManager;
        //[Inject] private IntuitiveInputManager _inputManager;
        [Inject] private InputManager _inputManager;
        [Inject] private PlayerManager _playerManager;
        [Inject] private PlayerFeedbackManager _playerFeedbackManager;
        [Inject] private EndlessIslandManager _endlessIslandManager;
        
        [SerializeField] private PlayerDefinition playerDefinition;
        [SerializeField] private PlayerSkill noWeaponInitSkill;
        [SerializeField] private PlayerSkill swordInitSkill;
        
        [Inject]
        private void PostInject()
        {
            GameStateContainer.GameState.Value = GameState.Island;
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnStart;
            
            GameStateContainer.InitialSkillDict = new()
            {
                { WeaponType.None, noWeaponInitSkill },
                { WeaponType.SingleSword, swordInitSkill }
            };
            _turnManager.StartTurn(this);
            
            
            _endlessIslandManager.SpawnIsland(playerDefinition);
        }

        private void Update()
        {
            _inputManager.ProcessInput();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                _endlessIslandManager.SpawnEnemyWave();
            }
        }

        
    }
}