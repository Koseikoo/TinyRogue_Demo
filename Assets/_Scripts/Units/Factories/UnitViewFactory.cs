using Container;
using Models;
using UnityEngine;
using Views;
using Zenject;

namespace Factories
{
    public class UnitViewFactory
    {
        [Inject] private PlayerView _playerPrefab;
        [Inject] private WeaponBoundFeedbackView _weaponBoundFeedbackPrefab;
        [Inject] private UnitViewContainer _unitViewContainer;
        [Inject] private DiContainer _container;

        public PlayerView CreatePlayerView(Player player)
        {
            PlayerView view = _container.InstantiatePrefab(_playerPrefab).GetComponent<PlayerView>();
            view.Initialize(player);
            return view;
        }

        public WeaponBoundFeedbackView CreateWeaponBoundFeedbackView(Transform weaponView, Transform playerView)
        {
            WeaponBoundFeedbackView view = _container.InstantiatePrefab(_playerPrefab).GetComponent<WeaponBoundFeedbackView>();
            view.Initialize(weaponView, playerView);
            return view;
        }
        
        public EnemyView CreateEnemyView(Enemy enemy)
        {
            var prefab = _unitViewContainer.GetEnemyPrefab(enemy.Type);
            EnemyView view = _container.InstantiatePrefab(prefab).GetComponent<EnemyView>();
            view.Initialize(enemy);
            return view;
        }

        public UnitView CreateUnitView(Unit unit)
        {
            var prefab = _unitViewContainer.GetUnitPrefab(unit.Type);
            var view = _container.InstantiatePrefab(prefab).GetComponent<UnitView>();
            view.Initialize(unit);
            return view;
        }

        public InteractableView CreateInteractableView(Interactable interactable)
        {
            var prefab = _unitViewContainer.GetInteractablePrefab(interactable.Type);
            var view = _container.InstantiatePrefab(prefab).GetComponent<InteractableView>();
            view.Initialize(interactable);
            return view;
        }
    }
}