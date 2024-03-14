using System.Linq;
using UnityEngine;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(fileName = "ItemInstaller", menuName = "Installer/ItemInstaller")]
    public class ItemInstaller : ScriptableObjectInstaller<ItemInstaller>
    {
        [SerializeField] private ItemDefinition DamageModDefinition;
        [SerializeField] private ItemDefinition PoisonModDefinition;
        [SerializeField] private ItemDefinition TestRelicDefinition;
        [SerializeField] private ItemDefinition HealthPotionDefinition;
        
        [Header("Resources")]
        [SerializeField] private ItemDefinition MonsterResourceDefinition;
        [SerializeField] private ItemDefinition PlantResourceDefinition;
        [SerializeField] private ItemDefinition StoneResourceDefinition;
        [SerializeField] private ItemDefinition WoodResourceDefinition;
        
        [Header("Equipment")]
        [SerializeField] private EquipmentDefinition TestArmor;
        [SerializeField] private EquipmentDefinition TestFace;
        [SerializeField] private EquipmentDefinition TestHelmet;
        
        [Header("Sprites")]
        [Header("Mods")]
        [SerializeField] private Sprite damageModSprite;
        [SerializeField] private Sprite poisonModSprite;
        [Header("Items")]
        [SerializeField] private Sprite testRelictSprite;
        [SerializeField] private Sprite HealthPotionSprite;
        [Header("Resources")]
        [SerializeField] private Sprite MonsterResource;
        [SerializeField] private Sprite PlantResource;
        [SerializeField] private Sprite WoodResource;
        [SerializeField] private Sprite StoneResource;
        [Header("Equipment")]
        [SerializeField] private Sprite TestArmorSprite;
        [SerializeField] private Sprite TestFaceSprite;
        [SerializeField] private Sprite TestHelmetSprite;
        public override void InstallBindings()
        {
            ItemDefinition[] items = new[]
            {
                DamageModDefinition,
                PoisonModDefinition,
                TestRelicDefinition,
                HealthPotionDefinition,
                MonsterResourceDefinition,
                PlantResourceDefinition,
                StoneResourceDefinition,
                WoodResourceDefinition
            };

            EquipmentDefinition[] equipment = new[]
            {
                TestArmor,
                TestFace,
                TestHelmet
            };
            
            Container.Bind<ItemContainer>().FromInstance(new(items, equipment)).AsSingle();
            
            Container.Bind<ItemIconContainer>().FromInstance(new(
                damageModSprite,
                poisonModSprite,
                testRelictSprite,
                HealthPotionSprite,
                MonsterResource,
                PlantResource,
                WoodResource,
                StoneResource,
                TestArmorSprite,
                TestFaceSprite,
                TestHelmetSprite)).AsSingle();
        }
    }
}