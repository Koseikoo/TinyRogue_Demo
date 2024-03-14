using System;
using UnityEngine;

namespace Installer
{
    public class ItemIconContainer
    {
        private Sprite _damageMod;
        private Sprite _poisonMod;
        private Sprite _testRelict;
        private Sprite _healthPotion;
        
        private Sprite _monsterResource;
        private Sprite _plantResource;
        private Sprite _woodResource;
        private Sprite _stoneResource;
        
        private Sprite _startArmor;
        private Sprite _startFace;
        private Sprite _startHelmet;

        public ItemIconContainer(
            Sprite damageMod, 
            Sprite poisonMod, 
            Sprite testRelict,
            Sprite healthPotion,
            Sprite monsterResource,
            Sprite plantResource,
            Sprite stoneResource,
            Sprite woodResource,
            Sprite startArmor,
            Sprite startFace,
            Sprite startHelmet)
        {
            _damageMod = damageMod;
            _poisonMod = poisonMod;
            _testRelict = testRelict;
            _healthPotion = healthPotion;

            _monsterResource = monsterResource;
            _plantResource = plantResource;
            _stoneResource = stoneResource;
            _woodResource = woodResource;

            _startArmor = startArmor;
            _startFace = startFace;
            _startHelmet = startHelmet;
        }
        
        public Sprite GetItemIcon(ItemType type)
        {
            return type switch
            {
                ItemType.StartArmorEquipment => _startArmor,
                ItemType.StartFaceEquipment => _startFace,
                ItemType.StartHelmetEquipment => _startHelmet,
                ItemType.DamageMod => _damageMod,
                ItemType.PoisonMod => _poisonMod,
                ItemType.TestRelic => _testRelict,
                ItemType.HealthPotion => _healthPotion,
                ItemType.MonsterResource => _monsterResource,
                ItemType.PlantResource => _plantResource,
                ItemType.StoneResource => _stoneResource,
                ItemType.WoodResource => _woodResource,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"No Sprite for type {type}")
            };
        }
    }
}