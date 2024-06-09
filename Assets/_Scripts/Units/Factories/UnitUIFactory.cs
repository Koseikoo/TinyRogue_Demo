using Models;
using Installer;
using UnityEngine;
using Views;
using Zenject;

namespace Factories
{
    public class UnitUIFactory
    {
        [Inject] private UnitUIView _unitUIPrefab;
        [Inject] private UnitHealthUIView _unitHealthUIPrefab;
        [Inject] private EnemyTurnDelayUIView _enemyTurnDelayUIPrefab;
        [Inject] private EnemyTurnClockUIView _enemyTurnClockUIPrefab;
        [Inject] private EnemyStateUIView _enemyStateUIPrefab;
        [Inject] private XpUIView _xpUIPrefab;
        [Inject] private PlayerUIView _playerUIPrefab;
        [Inject] private InteractableUIView _interactableUIPrefab;

        [Inject] private DiContainer _container;

        private Transform _unitUICanvas;
        private Transform _playerUICanvas;

        private UnitUIView _currentUnitUI;

        public UnitUIFactory()
        {
            _unitUICanvas = GameObject.Find("UnitUICanvas").transform;
            _playerUICanvas = GameObject.Find("PlayerUICanvas").transform;
        }

        public PlayerUIView CreatePlayerUI(Player player)
        {
            PlayerUIView view = _container.InstantiatePrefab(_playerUIPrefab, _playerUICanvas).GetComponent<PlayerUIView>();
            view.Initialize(player);
            return view;
        }
        
        public UnitUIFactory CreateUI(Unit unit, Transform unitVisual)
        {
            
            _currentUnitUI = _container.InstantiatePrefab(_unitUIPrefab, _unitUICanvas).GetComponent<UnitUIView>();
            _currentUnitUI.Initialize(unit, unitVisual);
            return this;
        }

        public UnitUIFactory AddHealth(Unit unit)
        {
            UnitHealthUIView healthUI = _container.InstantiatePrefab(_unitHealthUIPrefab, _currentUnitUI.transform).GetComponent<UnitHealthUIView>();
            healthUI.Initialize(unit);
            return this;
        }

        public UnitUIFactory AddStateUI(Enemy enemy)
        {
            EnemyStateUIView stateUI = _container.InstantiatePrefab(_enemyStateUIPrefab, _currentUnitUI.transform).GetComponent<EnemyStateUIView>();
            stateUI.Initialize(enemy);
            return this;
        }
        
        public UnitUIFactory AddXpUI(Player player)
        {
            XpUIView view = _container.InstantiatePrefab(_xpUIPrefab, _currentUnitUI.transform).GetComponent<XpUIView>();
            view.Initialize(player);
            return this;
        }
        
        public UnitUIFactory AddTurnDelayUI(Enemy enemy)
        {
            EnemyTurnClockUIView delayUI = _container.InstantiatePrefab(_enemyTurnClockUIPrefab, _currentUnitUI.transform).GetComponent<EnemyTurnClockUIView>();
            delayUI.Initialize(enemy);
            return this;
        }

        public UnitUIFactory AddInteractionButtonUI(Interactable interactable)
        {
            InteractableUIView interactUI = _container.InstantiatePrefab(_interactableUIPrefab, _currentUnitUI.transform).GetComponent<InteractableUIView>();
            interactUI.Initialize(interactable);
            return this;
        }
    }
}