using System;
using System.Collections.Generic;

namespace Container
{
    public class UnitContainer
    {
        private Dictionary<UnitType, EnemyDefinition> _enemyDefinitions = new();
        private Dictionary<UnitType, InteractableDefinition> _interactableDefinitions = new();
        private Dictionary<UnitType, UnitDefinition> _unitDefinitions = new();
        
        public UnitContainer(
            EnemyDefinition[] enemies,
            InteractableDefinition[] interactables,
            UnitDefinition[] units)
        {
            foreach (EnemyDefinition enemy in enemies)
                _enemyDefinitions[enemy.Type] = new(enemy);
            
            foreach (InteractableDefinition interactable in interactables)
                _interactableDefinitions[interactable.Type] = new(interactable);
            
            foreach (UnitDefinition unit in units)
                _unitDefinitions[unit.Type] = new(unit);
        }

        public UnitDefinition GetUnitDefinition(UnitType type)
        {
            if (_unitDefinitions.TryGetValue(type, out var definition))
                return definition;
            throw new Exception($"No Unit of Type {type}");
        }
        
        public EnemyDefinition GetEnemyDefinition(UnitType type)
        {
            if (_enemyDefinitions.TryGetValue(type, out var definition))
                return definition;
            throw new Exception($"No Enemy of Type {type}");
        }
        
        public InteractableDefinition GetInteractableDefinition(UnitType type)
        {
            if (_interactableDefinitions.TryGetValue(type, out var definition))
                return definition;
            throw new Exception($"No Interactable of Type {type}");
        }
    }
}