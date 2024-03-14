using System;
using Factories;
using UnityEngine;
using Views;

namespace Container
{
    public class UnitViewContainer
    {
        private UnitView _obstaclePrefab;
        private UnitView[] _destructiblePrefabs;
        
        private InteractableView _helmPrefab;
        private InteractableView _chestPrefab;

        private EnemyView _spider_1;
        private EnemyView _mushroom_1;
        private EnemyView _rat_1;
        private EnemyView _orc_1;
        private EnemyView _wolf_1;
        private EnemyView _golem_1;
        private EnemyView _specter;
        
        private EnemyView _fisherman;
        private EnemyView _werewolf;

        public UnitViewContainer(UnitView obstaclePrefab,
            UnitView[] destructiblePrefabs,
            InteractableView helmPrefab,
            InteractableView chestPrefab,
            EnemyView spider1,
            EnemyView mushroom1,
            EnemyView rat1,
            EnemyView orc1,
            EnemyView wolf1,
            EnemyView golem1,
            EnemyView specter,
            EnemyView fisherman,
            EnemyView werewolf)
        {
            _obstaclePrefab = obstaclePrefab;
            _destructiblePrefabs = destructiblePrefabs;

            _helmPrefab = helmPrefab;
            _chestPrefab = chestPrefab;

            _spider_1 = spider1;
            _mushroom_1 = mushroom1;
            _rat_1 = rat1;
            _orc_1 = orc1;
            _wolf_1 = wolf1;
            _golem_1 = golem1;
            _specter = specter;

            _fisherman = fisherman;
            _werewolf = werewolf;

        }

        public UnitView GetUnitPrefab(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Obstacle => _obstaclePrefab,
                UnitType.TreeDestructible => _destructiblePrefabs.PickRandom(),
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, $"No Prefab for Section {unitType}")
            };
        }
        
        public InteractableView GetInteractablePrefab(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.HelmInteractable => _helmPrefab,
                UnitType.ChestInteractable => _chestPrefab,
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, $"No Prefab for Interactable {unitType}")
            };
        }
        
        public EnemyView GetEnemyPrefab(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.SpiderEnemy => _spider_1,
                UnitType.Mushroom => _mushroom_1,
                UnitType.Rat => _rat_1,
                UnitType.Orc => _orc_1,
                UnitType.WolfEnemy => _wolf_1,
                UnitType.GolemEnemy => _golem_1,
                UnitType.SpecterEnemy => _specter,
                UnitType.FishermanMiniBoss => _fisherman,
                UnitType.WerewolfBoss => _werewolf,
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, $"No Prefab for Interactable {unitType}")
            };
        }
    }
}