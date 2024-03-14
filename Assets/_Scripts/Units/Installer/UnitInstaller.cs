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
        [Header("World")]
        [SerializeField] private PlayerView _playerPrefab;
        [SerializeField] private UnitView _obstaclePrefab;
        [SerializeField] private UnitView[] _destructiblePrefabs;
        
        [Header("Interact Unit Views")]
        [SerializeField] private InteractableView _helmPrefab;
        [SerializeField] private InteractableView _chestPrefab;
        
        [Header("Enemy Views")]
        [SerializeField] private EnemyView _spiderPrefab;
        [SerializeField] private EnemyView _mushroomPrefab;
        [SerializeField] private EnemyView _ratPrefab;
        [SerializeField] private EnemyView _orcPrefab;
        [SerializeField] private EnemyView _wolfPrefab;
        [SerializeField] private EnemyView _golemPrefab;
        [SerializeField] private EnemyView _specterPrefab;
        [SerializeField] private EnemyView _fishermanPrefab;
        [SerializeField] private EnemyView _werewolfPrefab;

        [Header("UI")]
        [SerializeField] private UnitUIView _unitUIPrefab;
        [SerializeField] private UnitHealthUIView _unitHealthUIPrefab;
        [SerializeField] private EnemyTurnDelayUIView enemyTurnDelayUIPrefab;
        [SerializeField] private EnemyTurnClockUIView enemyTurnClockUIPrefab;
        [SerializeField] private EnemyStateUIView _enemyStateUIPrefab;
        [SerializeField] private InteractableUIView _interactableUIPrefab;
        [SerializeField] private XpBarUIView _xpBarUIPrefab;
        
        [SerializeField] private PlayerUIView _playerUIPrefab;
        [Header("Sprites")]
        [SerializeField] private Sprite _idleSprite;
        [SerializeField] private Sprite _targetFoundSprite;
        [SerializeField] private Sprite _aimAtTargetSprite;
        
        [Header("Enemy Definitions")]
        [SerializeField] private EnemyDefinition _testEnemyDefinition;
        [SerializeField] private EnemyDefinition _spiderDefinition_1;
        [SerializeField] private EnemyDefinition _mushroomDefinition_1;
        [SerializeField] private EnemyDefinition _ratDefinition_1;
        [SerializeField] private EnemyDefinition _orcDefinition_1;
        [SerializeField] private EnemyDefinition _wolfDefinition_1;
        [SerializeField] private EnemyDefinition _golemDefinition_1;
        [SerializeField] private EnemyDefinition _specterDefinition;
        [SerializeField] private EnemyDefinition _fishermanDefinition;
        [SerializeField] private EnemyDefinition _werewolfDefinition;
        
        [Header("Unit Definitions")]
        [SerializeField] private UnitDefinition _obstacleDefinition;
        [SerializeField] private UnitDefinition _destructibleDefinition;
        
        [Header("Interactable Definitions")]
        [SerializeField] private InteractableDefinition _helmDefinition;
        [SerializeField] private InteractableDefinition _chestDefinition;

        public override void InstallBindings()
        {
            Container.Bind<PlayerView>().FromInstance(_playerPrefab).AsSingle();
            Container.Bind<UnitUIView>().FromInstance(_unitUIPrefab).AsSingle();
            Container.Bind<UnitHealthUIView>().FromInstance(_unitHealthUIPrefab).AsSingle();
            Container.Bind<EnemyTurnDelayUIView>().FromInstance(enemyTurnDelayUIPrefab).AsSingle();
            Container.Bind<EnemyTurnClockUIView>().FromInstance(enemyTurnClockUIPrefab).AsSingle();
            Container.Bind<EnemyStateUIView>().FromInstance(_enemyStateUIPrefab).AsSingle();
            Container.Bind<PlayerUIView>().FromInstance(_playerUIPrefab).AsSingle();
            Container.Bind<InteractableUIView>().FromInstance(_interactableUIPrefab).AsSingle();
            Container.Bind<XpBarUIView>().FromInstance(_xpBarUIPrefab).AsSingle();
            
            Container.Bind<EnemyStateIconContainer>().FromInstance(new(_idleSprite, _targetFoundSprite, _aimAtTargetSprite)).AsSingle();
            Container.Bind<EnemyDefinitionContainer>().FromInstance(new(
                _testEnemyDefinition,
                _spiderDefinition_1,
                _mushroomDefinition_1,
                _ratDefinition_1,
                _orcDefinition_1,
                _wolfDefinition_1,
                _golemDefinition_1,
                _specterDefinition,
                _fishermanDefinition,
                _werewolfDefinition)).AsSingle();

            Container.Bind<UnitDefinitionContainer>().FromInstance(new(_obstacleDefinition, _destructibleDefinition)).AsSingle();
            Container.Bind<InteractableDefinitionContainer>().FromInstance(new(_helmDefinition, _chestDefinition)).AsSingle();
            Container.Bind<UnitViewContainer>()
                .FromInstance(new(_obstaclePrefab, 
                    _destructiblePrefabs, 
                    _helmPrefab, 
                    _chestPrefab, 
                    _spiderPrefab, 
                    _mushroomPrefab, 
                    _ratPrefab,
                    _orcPrefab,
                    _wolfPrefab,
                    _golemPrefab,
                    _specterPrefab,
                    _fishermanPrefab,
                    _werewolfPrefab))
                .AsSingle();
            
            Container.Bind<UnitActionContainer>().AsSingle();
            Container.Bind<UnitDeathActionContainer>().AsSingle();
            Container.Bind<UnitFactory>().AsSingle();
            Container.Bind<UnitViewFactory>().AsSingle();
            Container.Bind<UnitUIFactory>().AsSingle();
        }
    }
}