using Container;
using Installer;
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

        [Inject(Id = UnitInstaller.UnitParent)] private Transform _unitParent;

        public PlayerView CreatePlayerView(Player player)
        {
            PlayerView view = _container.InstantiatePrefab(_playerPrefab).GetComponent<PlayerView>();
            view.transform.SetParent(_unitParent);
            view.Initialize(player);
            return view;
        }

        public EnemyView CreateEnemyView(Enemy enemy, UnitDefinition definition)
        {
            EnemyView view = _container.InstantiatePrefab(definition.Prefab).GetComponent<EnemyView>();
            view.transform.SetParent(_unitParent);
            view.Initialize(enemy);
            return view;
        }

        public UnitView CreateUnitView(GameUnit gameUnit, UnitDefinition definition)
        {
            UnitView view = _container.InstantiatePrefab(definition.Prefab).GetComponent<UnitView>();
            view.transform.SetParent(_unitParent);
            view.Initialize(gameUnit);
            return view;
        }

        public InteractableView CreateInteractableView(Interactable interactable, UnitDefinition definition)
        {
            InteractableView view = _container.InstantiatePrefab(definition.Prefab).GetComponent<InteractableView>();
            view.transform.SetParent(_unitParent);
            view.Initialize(interactable);
            return view;
        }
    }
}