using System.Collections.Generic;
using Container;
using Factories;
using Factory;
using Models;
using UniRx;
using UnityEngine;
using Views;
using Zenject;

namespace Game
{
    public class GameSetup
    {
        [Inject] private ModalFactory _modalFactory;

        [Inject] private TileActionContainer _tileActionContainer;
        [Inject] private UnitActionContainer _unitActionContainer;
        [Inject] private ChoiceContainer _choiceContainer;
        [Inject] private UnitRecipeDropContainer _unitRecipeDropContainer;

        [Inject] private GameAreaManager _gameAreaManager;

        public void Setup()
        {
            DefineTileEnterLogic();
            SetPersistantPlayerState();
        }

        private void DefineTileEnterLogic()
        {
            _tileActionContainer.NextIslandAction = unit =>
            {
                if (unit is Player)
                {
                    GameStateContainer.GameState.Value = GameState.Island;
                }
            };
    
            _tileActionContainer.IslandEndAction = unit =>
            {
                if (unit is Player && _gameAreaManager.Island.EndTileUnlocked.Value)
                {
                    List<Choice> choices = new()
                    {
                        _choiceContainer.GetChoice(Choices.NextIsland, _gameAreaManager.SpawnNewIsland),
                        _choiceContainer.GetChoice(Choices.ToShip, () =>
                        {
                            GameStateContainer.GameState.Value = GameState.Ship;
                        })
                    };
                    _modalFactory.CreateChoiceModal(choices);
                }
            };
    
            _unitActionContainer.SelectIslandAction = interactable =>
            {
                GameStateContainer.GameState.Value = GameState.Island;
            };
    
            _unitActionContainer.OpenModSmithUI = interactable =>
            {
                Debug.Log("Show Open Mod Smith UI");
            };
    
            _unitActionContainer.OpenChest = interactable =>
            {
                interactable.Damage(interactable.Health.Value, GameStateContainer.Player);
            };
        }
        private void SetPersistantPlayerState()
        {
            // Load From Player Prefs
            PersistentPlayerState.Set(10,
                new(),
                new(),
                1);
        }
        
    }
}