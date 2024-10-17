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
        [Inject] private SkillCraftingManager _skillCraftingManager;

        [SerializeField] private int wavesPerIsland;
        [SerializeField] private PlayerDefinition playerDefinition;
        
        [Inject]
        private void PostInject()
        {
            GameStateContainer.GameState.Value = GameState.Island;
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnStart;
            
            _turnManager.StartTurn(this);
            _playerManager.SpawnPlayerWithWeapon(null, playerDefinition);
            _endlessIslandManager.SpawnIsland(wavesPerIsland);
        }

        private void Update()
        {
            _inputManager.ProcessInput();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                //_endlessIslandManager.SpawnEnemyWave();
                _skillCraftingManager.OpenCraftingModal();
                
            }
        }
    }
}