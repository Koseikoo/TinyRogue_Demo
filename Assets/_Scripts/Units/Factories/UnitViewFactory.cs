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
        [Inject] private DiContainer _container;

        public PlayerView CreatePlayerView(Player player)
        {
            PlayerView view = _container.InstantiatePrefab(_playerPrefab).GetComponent<PlayerView>();
            view.Initialize(player);
            return view;
        }

        public EnemyView CreateEnemyView(Enemy enemy, UnitDefinition definition)
        {
            EnemyView view = _container.InstantiatePrefab(definition.Prefab).GetComponent<EnemyView>();
            view.Initialize(enemy);
            return view;
        }

        public UnitView CreateUnitView(Unit unit, UnitDefinition definition)
        {
            var view = _container.InstantiatePrefab(definition.Prefab).GetComponent<UnitView>();
            view.Initialize(unit);
            return view;
        }

        public InteractableView CreateInteractableView(Interactable interactable, UnitDefinition definition)
        {
            var view = _container.InstantiatePrefab(definition.Prefab).GetComponent<InteractableView>();
            view.Initialize(interactable);
            return view;
        }
    }
}