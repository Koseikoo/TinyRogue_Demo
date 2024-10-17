using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

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
            UnitDefinition definition = null;
            
            if (_unitDefinitions.TryGetValue(type, out UnitDefinition unit))
            {
                definition = unit;
            }
            else if (_enemyDefinitions.TryGetValue(type, out EnemyDefinition enemy))
            {
                definition = enemy;
            }
            else if (_interactableDefinitions.TryGetValue(type, out InteractableDefinition interactable))
            {
                definition = interactable;
            }

            if (definition == null)
            {
                Debug.LogError($"No Unit of Type {type}");
            }
            return definition;
        }
        
        public Loot GetRandomUnitLoot(GameUnit gameUnit, int amount)
        {
            List<Mod> mods = new();
            List<Item> items = new();
            List<Resource> resources = new();
            List<Equipment> equipments = new();
            UnitDefinition def = GetUnitDefinition(gameUnit.Type);

            if (def.ItemsToDrop.Count == 0)
            {
                return new Loot(0);
            }

            for (int i = 0; i < amount; i++)
            {
                ItemDefinition itemDefinition = PickItemByChance(def.ItemsToDrop);
                Item item = itemDefinition.Type.GetItemInstance(itemDefinition.Amount);

                if (item.Type.ToString().ToLower().Contains("mod"))
                {
                    Mod mod = item.Type.GetModInstance(1);
                    mods.Add(mod);
                }
                else if (item.Type.ToString().ToLower().Contains("resource"))
                {
                    Resource resource = item.Type.GetResourceInstance(1);
                    resources.Add(resource);
                }
                else if (item.Type.ToString().ToLower().Contains("equipment"))
                {
                    Equipment equipment = item.Type.GetEquipmentInstance();
                    equipments.Add(equipment);
                }
                else
                {
                    items.Add(item);
                }
            }
            
            Loot loot = new(0, mods, items, resources);
            return loot;
        }
        
        private ItemDefinition PickItemByChance(List<ItemDefinition> items)
        {
            List<(float chance, ItemDefinition definition)> chances = new();
            float chanceSum = 0;
            foreach (ItemDefinition item in items)
            {
                chances.Add(new(chanceSum + item.DropChance, item));
                chanceSum += item.DropChance;
            }

            float rand = UnityEngine.Random.value * chanceSum;

            for (int i = 0; i < chances.Count; i++)
            {
                if (rand < chances[i].chance)
                {
                    return chances[i].definition;
                }
            }
            throw new Exception("No Item was Picked!");
        }
        
        public EnemyDefinition GetEnemyDefinition(UnitType type)
        {
            if (_enemyDefinitions.TryGetValue(type, out EnemyDefinition definition))
            {
                return definition;
            }
            throw new Exception($"No Enemy of Type {type}");
        }
        
        public InteractableDefinition GetInteractableDefinition(UnitType type)
        {
            if (_interactableDefinitions.TryGetValue(type, out InteractableDefinition definition))
            {
                return definition;
            }
            throw new Exception($"No Interactable of Type {type}");
        }
    }
}