using System;
using Factories;
using UnityEngine;
using Views;
using Zenject;

public enum LootType
{
    Gold,
    Mod,
    Item,
    Resource
}

public enum CurrencyType
{
    Gold
}

namespace Installer
{
    /// <summary>
    /// Loot is Visualized with Materials on a Sphere.
    /// Every Loot Visual in the World is a Sphere and the Material Type/Color Indicates the Type of Loot
    ///
    /// Gold => Yellow
    /// Mod => Red
    /// Item => ...
    /// 
    /// </summary>
    [CreateAssetMenu(fileName = "LootInstaller", menuName = "Installer/LootInstaller")]
    public class LootInstaller : ScriptableObjectInstaller<LootInstaller>
    {
        [SerializeField] private LootView lootViewPrefab;
        [SerializeField] private GoldCoinView goldCoinPrefab;
        [Header("Materials")]
        [SerializeField] private Material goldMaterial;
        [SerializeField] private Material modMaterial;
        [SerializeField] private Material itemMaterial;
        [SerializeField] private Material resourceMaterial;

        [Header("Icons")]
        [Header("Currency")]
        [SerializeField] private Sprite goldSprite;

        public override void InstallBindings()
        {
            Container.Bind<LootView>().FromInstance(lootViewPrefab).AsSingle();
            Container.Bind<GoldCoinView>().FromInstance(goldCoinPrefab).AsSingle();
            Container.Bind<LootMaterialContainer>().FromInstance(new(
                goldMaterial,
                modMaterial,
                itemMaterial,
                resourceMaterial)).AsSingle();

            Container.Bind<CurrencyIconContainer>().FromInstance(new(goldSprite)).AsSingle();

            Container.Bind<LootFactory>().AsSingle().NonLazy();
        }
    }

    public class LootMaterialContainer
    {
        private Material _goldMaterial;
        private Material _modMaterial;
        private Material _itemMaterial;
        private Material _resourceMaterial;

        public LootMaterialContainer(
            Material goldMaterial,
            Material modMaterial,
            Material itemMaterial,
            Material resourceMaterial)
        {
            _goldMaterial = goldMaterial;
            _modMaterial = modMaterial;
            _itemMaterial = itemMaterial;
            _resourceMaterial = resourceMaterial;
        }

        public Material GetMaterial(LootType type)
        {
            return type switch
            {
                LootType.Gold => _goldMaterial,
                LootType.Mod => _modMaterial,
                LootType.Item => _itemMaterial,
                LootType.Resource => _resourceMaterial,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"Material of type {type.ToString()} does not exist")
            };
        }
    }

    public class CurrencyIconContainer
    {
        public Sprite _gold;

        public CurrencyIconContainer(Sprite gold)
        {
            _gold = gold;
        }

        public Sprite GetCurrencyIcon(CurrencyType type)
        {
            return type switch
            {
                CurrencyType.Gold => _gold,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, $"No Currency Sprite of type {type}")
            };
        }
    }
}