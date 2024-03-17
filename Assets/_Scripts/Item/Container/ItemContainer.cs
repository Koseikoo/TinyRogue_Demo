using System;
using System.Collections.Generic;
using System.Linq;
using Models;
using Random = UnityEngine.Random;

namespace Installer
{
    public class ItemContainer
    {
        private List<ItemDefinition> _itemDefinitions = new();
        private List<EquipmentDefinition> _equipmentDefinitions = new();

        public ItemContainer(
            ItemDefinition[] items,
            EquipmentDefinition[] equipment
            )
        {
            for (int i = 0; i < items.Length; i++)
            {
                _itemDefinitions.Add(new(items[i]));
            }
            
            for (int i = 0; i < equipment.Length; i++)
            {
                _equipmentDefinitions.Add(new(equipment[i]));
            }
        }

        public Equipment GetEquipment(ItemType type)
        {
            EquipmentDefinition match = _equipmentDefinitions.FirstOrDefault(item => item.ItemDefinition.Type == type);
            if (match == null)
                throw new Exception($"{type} definition is Missing in ItemContainer");
            return new Equipment(type)
            {
                AttackBonus = match.AttackBonus,
                ArmorBonus = match.ArmorBonus,
                HealthBonus = match.HealthBonus
            };
        }

        public Item GetItem(ItemType type, int amount = 1)
        {
            ItemDefinition match = _itemDefinitions.FirstOrDefault(item => item.Type == type);
            if (match == null)
                throw new Exception($"{type} definition is Missing in ItemContainer");
            Item item = type.GetItemInstance(amount);
            return item;
        }
        
        public Resource GetResource(ItemType type, int amount = 1)
        {
            ItemDefinition match = _itemDefinitions.FirstOrDefault(item => item.Type == type);
            if (match == null)
                throw new Exception($"{type} definition is Missing in ItemContainer");
            Resource resource = type.GetResourceInstance(amount);
            return resource;
        }
        
        public Loot GetRandomUnitLoot(Unit unit, int amount)
        {
            List<Mod> mods = new();
            List<Item> items = new();
            List<Resource> resources = new();
            List<Equipment> equipments = new();
            
            var itemMatches = _itemDefinitions.FindAll(item => item.DroppedFrom.Contains(unit.Type));
            if (itemMatches.Count == 0)
                return new Loot(0);
            
            for (int i = 0; i < amount; i++)
            {
                Item item = PickItemByChance(itemMatches).Type.GetItemInstance(1);

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

        private EquipmentDefinition PickEquipmentByChance(List<EquipmentDefinition> equipment)
        {
            List<ItemDefinition> definitions = equipment
                .Select(e => e.ItemDefinition)
                .ToList();

            ItemDefinition definition = PickItemByChance(definitions);
            return equipment.FirstOrDefault(e => e.ItemDefinition == definition);
        }

        private ItemDefinition PickItemByChance(List<ItemDefinition> items)
        {
            List<(float chance, ItemDefinition definition)> chances = new();
            float chanceSum = 0;
            foreach (var item in items)
            {
                chances.Add(new(chanceSum + item.DropChance, item));
                chanceSum += item.DropChance;
            }

            var rand = Random.value * chanceSum;

            for (int i = 0; i < chances.Count; i++)
            {
                if (rand < chances[i].chance)
                    return chances[i].definition;
            }
            throw new Exception("No Item was Picked!");
        }
    }
}