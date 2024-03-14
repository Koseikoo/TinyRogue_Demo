using System.Collections.Generic;
using Models;
using UnityEngine;
using Views;
using Zenject;

namespace Factory
{
    public class ModalFactory
    {
        [Inject] private EnemyInfoModalView _enemyInfoModalPrefab;
        [Inject] private DeathModalView _deathModalPrefab;
        [Inject] private CharacterCreationModalView _characterCreationModalPrefab;
        [Inject] private ChoiceModalView _choiceModalPrefab;
        [Inject] private CraftModalUIView _craftModalPrefab;
        [Inject] private UnlockRecipeUIModal _unlockRecipeUIPrefab;
        
        [Inject] private DiContainer _container;

        private Transform _modalUICanvas;

        public ModalFactory()
        {
            _modalUICanvas = GameObject.Find("ModalUICanvas").transform;
        }

        public EnemyInfoModalView CreateUnitInfoModal(Unit unit)
        {
            var view = _container.InstantiatePrefab(_enemyInfoModalPrefab, _modalUICanvas)
                .GetComponent<EnemyInfoModalView>();
            view.Initialize(unit);
            return view;
        }

        public DeathModalView CreateDeathModal()
        {
            var view = _container.InstantiatePrefab(_deathModalPrefab, _modalUICanvas)
                .GetComponent<DeathModalView>();
            view.Initialize();
            return view;
        }
        
        public CharacterCreationModalView CreateCharacterCreationModal()
        {
            var view = _container.InstantiatePrefab(_characterCreationModalPrefab, _modalUICanvas)
                .GetComponent<CharacterCreationModalView>();
            view.Initialize();
            return view;
        }

        public ChoiceModalView CreateChoiceModal(List<Choice> choices, bool canClose = true)
        {
            var view = _container.InstantiatePrefab(_choiceModalPrefab, _modalUICanvas)
                .GetComponent<ChoiceModalView>();
            view.Initialize(choices, canClose);
            return view;
        }
        
        public CraftModalUIView CreateCraftModal(BlackSmith blackSmith)
        {
            var view = _container.InstantiatePrefab(_craftModalPrefab, _modalUICanvas)
                .GetComponent<CraftModalUIView>();
            view.Initialize(blackSmith);
            return view;
        }
        
        public UnlockRecipeUIModal CreateUnlockRecipeModal(ItemType unlockedRecipe)
        {
            // modal doesnt appear
            var view = _container.InstantiatePrefab(_unlockRecipeUIPrefab, _modalUICanvas)
                .GetComponent<UnlockRecipeUIModal>();
            view.Initialize(unlockedRecipe);
            return view;
        }
    }
}