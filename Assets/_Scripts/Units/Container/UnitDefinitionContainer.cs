using System;

public enum UnitType
{
    Player = 0,
    
    //Enemies
    TestEnemy = 1,
    SpiderEnemy = 2,
    WolfEnemy = 14,
    GolemEnemy = 15,
    SpecterEnemy = 16,
    
    //Bosse
    FishermanMiniBoss = 17,
    WerewolfBoss = 18,
    
    Mushroom = 3,
    Rat = 4,
    Orc = 9,
    
    Obstacle = 5,
    TreeDestructible = 6,
    GraveDestructible = 19,
    WheatDestructible = 20,
    
    //Interactables
    HelmInteractable = 7,
    ChestInteractable = 8,
    
    // Other
    OrcFenceVisual = 10,
    OrcTentVisual = 11,
    CampFireVisual = 12,
    TreeStumpVisual = 13,
}

namespace Container
{
    public class UnitDefinitionContainer
    {
        private UnitDefinition _obstacleDefinition;
        private UnitDefinition _destructibleDefinition;

        public UnitDefinitionContainer(UnitDefinition obstacleDefinition, UnitDefinition destructibleDefinition)
        {
            _obstacleDefinition = new(obstacleDefinition);
            _destructibleDefinition = new(destructibleDefinition);
        }

        public UnitDefinition GetUnitDefinition(UnitType type)
        {
            return type switch
            {
                UnitType.Obstacle => _obstacleDefinition,
                UnitType.TreeDestructible => _destructibleDefinition,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"No unit definition for type {type}")
            };
        }
    }
}