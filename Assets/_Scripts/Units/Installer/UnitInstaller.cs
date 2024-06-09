using System;
using Container;
using Factories;
using UnityEngine;
using UnityEngine.Serialization;
using Views;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(fileName = "UnitInstaller", menuName = "Installer/UnitInstaller")]
    public class UnitInstaller : ScriptableObjectInstaller<UnitInstaller>
    {
        public const string UnitParent = "UnitParent";
        
        [Header("World")]
        [SerializeField] private PlayerView _playerPrefab;

        [Header("UI")]
        [SerializeField] private UnitUIView _unitUIPrefab;
        [SerializeField] private UnitHealthUIView _unitHealthUIPrefab;
        [SerializeField] private EnemyTurnDelayUIView enemyTurnDelayUIPrefab;
        [SerializeField] private EnemyTurnClockUIView enemyTurnClockUIPrefab;
        [SerializeField] private EnemyStateUIView _enemyStateUIPrefab;
        [SerializeField] private XpUIView _xpUIPrefab;
        [SerializeField] private InteractableUIView _interactableUIPrefab;
        
        [SerializeField] private PlayerUIView _playerUIPrefab;
        [Header("Sprites")]
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite _targetFoundSprite;
        [SerializeField] private Sprite _aimAtTargetSprite;
        
        [Header("Unit (NEW)")]
        public EnemyDefinition[] Enemies;
        public InteractableDefinition[] Interactables;
        public UnitDefinition[] Units;
        
        
        private Transform _unitParent;

        public override void InstallBindings()
        {
            _unitParent = GameObject.Find(UnitParent).transform;
            
            Container.Bind<PlayerView>().FromInstance(_playerPrefab).AsSingle();
            Container.Bind<UnitUIView>().FromInstance(_unitUIPrefab).AsSingle();
            Container.Bind<UnitHealthUIView>().FromInstance(_unitHealthUIPrefab).AsSingle();
            Container.Bind<EnemyTurnDelayUIView>().FromInstance(enemyTurnDelayUIPrefab).AsSingle();
            Container.Bind<EnemyTurnClockUIView>().FromInstance(enemyTurnClockUIPrefab).AsSingle();
            Container.Bind<XpUIView>().FromInstance(_xpUIPrefab).AsSingle();
            Container.Bind<EnemyStateUIView>().FromInstance(_enemyStateUIPrefab).AsSingle();
            Container.Bind<PlayerUIView>().FromInstance(_playerUIPrefab).AsSingle();
            Container.Bind<InteractableUIView>().FromInstance(_interactableUIPrefab).AsSingle();

            Container.Bind<Transform>().WithId(UnitParent).FromInstance(_unitParent).AsSingle();
            
            Container.Bind<EnemyStateIconContainer>().FromInstance(new(_idleSprite, _targetFoundSprite, _aimAtTargetSprite)).AsSingle();
            Container.Bind<UnitContainer>().FromInstance(new(Enemies, Interactables, Units)).AsSingle();

            Container.Bind<UnitActionContainer>().AsSingle();
            Container.Bind<UnitDeathActionContainer>().AsSingle();
            Container.Bind<UnitFactory>().AsSingle();
            Container.Bind<UnitViewFactory>().AsSingle();
            Container.Bind<UnitUIFactory>().AsSingle();
            
        }
    }
}