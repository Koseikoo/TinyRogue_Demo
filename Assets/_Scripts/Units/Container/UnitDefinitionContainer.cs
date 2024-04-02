using System;

public enum UnitType
{
    Player = 0,
    IslandHeart = 26,
    
    //Enemies
    TestEnemy = 1,
    MushroomEnemy = 3,
    RatEnemy = 4,
    OrcEnemy = 9,
    SpiderEnemy = 2,
    WolfEnemy = 14,
    BigWolfEnemy = 25,
    GolemEnemy = 15,
    SpecterEnemy = 16,
    
    //Bosse
    FishermanMiniBoss = 17,
    WerewolfBoss = 18,
    
    PathBlocker = 5,
    Tree = 6,
    Grave = 19,
    Pillar = 21,
    Wheat = 20,
    CampWall = 22,
    CampFire = 23,
    Cave = 24,
    
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
        private UnitDefinition _treeDefinition;
        private UnitDefinition _pillarDefinition;
        private UnitDefinition _campWallDefinition;
        private UnitDefinition _campFireDefinition;

        public UnitDefinitionContainer(
            UnitDefinition obstacleDefinition, 
            UnitDefinition treeDefinition, 
            UnitDefinition pillarDefinition,
            UnitDefinition campWallDefinition,
            UnitDefinition campFireDefinition
            )
        {
            _obstacleDefinition = new(obstacleDefinition);
            _treeDefinition = new(treeDefinition);
            _pillarDefinition = new(pillarDefinition);
            _campWallDefinition = new(campWallDefinition);
            _campFireDefinition = new(campFireDefinition);
        }

        public UnitDefinition GetUnitDefinition(UnitType type)
        {
            return type switch
            {
                UnitType.PathBlocker => _obstacleDefinition,
                UnitType.Tree => _treeDefinition,
                UnitType.Pillar => _pillarDefinition,
                UnitType.CampWall => _campWallDefinition,
                UnitType.CampFire => _campFireDefinition,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"No unit definition for type {type}")
            };
        }
    }
}