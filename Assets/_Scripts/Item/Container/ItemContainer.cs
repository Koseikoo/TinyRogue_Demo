using System;
using System.Collections.Generic;
using System.Linq;
using Container;
using Models;
using Zenject;
using Random = UnityEngine.Random;

namespace Installer
{
    public class ItemContainer
    {
        private List<ItemDefinition> _itemDefinitions = new();
        private List<EquipmentDefinition> _equipmentDefinitions = new();

        [Inject] private UnitContainer _unitContainer;

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
            {
                throw new Exception($"{type} definition is Missing in ItemContainer");
            }
            return new Equipment(type)
            {
                AttackBonus = match.AttackBonus,
                ArmorBonus = match.ArmorBonus,
                HealthBonus = match.HealthBonus
            };
        }

        public Resource GetResource(ItemType type, int amount = 1)
        {
            ItemDefinition match = _itemDefinitions.FirstOrDefault(item => item.Type == type);
            if (match == null)
            {
                throw new Exception($"{type} definition is Missing in ItemContainer");
            }
            Resource resource = type.GetResourceInstance(amount);
            return resource;
        }
    }
}