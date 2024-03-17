using System;

namespace Container
{
    public class EnemyDefinitionContainer
    {
        private EnemyDefinition _testEnemy;
        private EnemyDefinition _spider;
        private EnemyDefinition _mushroom;
        private EnemyDefinition _rat;
        private EnemyDefinition _orc;
        private EnemyDefinition _wolf;
        private EnemyDefinition _golem;
        private EnemyDefinition _specter;
        private EnemyDefinition _fisherman;
        private EnemyDefinition _werewolf;

        public EnemyDefinitionContainer(EnemyDefinition testEnemy,
            EnemyDefinition spider,
            EnemyDefinition mushroom,
            EnemyDefinition rat,
            EnemyDefinition orc,
            EnemyDefinition wolf,
            EnemyDefinition golem,
            EnemyDefinition specter,
            EnemyDefinition fisherman,
            EnemyDefinition werewolf)
        {
            _testEnemy = new(testEnemy);
            _spider = new(spider);
            _mushroom = new(mushroom);
            _rat = new(rat);
            _orc = new(orc);
            _wolf = new(wolf);
            _golem = new(golem);
            _specter = new(specter);
            _fisherman = new(fisherman);
            _werewolf = new(werewolf);
        }

        public EnemyDefinition GetEnemyDefinition(UnitType type)
        {
            return type switch
            {
                UnitType.TestEnemy => _testEnemy,
                UnitType.SpiderEnemy => _spider,
                UnitType.MushroomEnemy => _mushroom,
                UnitType.RatEnemy => _rat,
                UnitType.OrcEnemy => _orc,
                UnitType.WolfEnemy => _wolf,
                UnitType.GolemEnemy => _golem,
                UnitType.SpecterEnemy => _specter,
                UnitType.FishermanMiniBoss => _fisherman,
                UnitType.WerewolfBoss => _werewolf,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"No enemy definition for type {type}")
            };
        }
    }
}