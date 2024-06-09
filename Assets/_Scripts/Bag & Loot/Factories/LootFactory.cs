using System.Collections.Generic;
using Installer;
using Models;
using UnityEngine;
using Views;
using Zenject;

namespace Factories
{
    public class LootFactory
    {
        private const float GoldCoinSpread = .05f;
        [Inject] private LootView _lootViewPrefab;
        [Inject] private GoldCoinView _goldCoinPrefab;
        [Inject] private ItemContainer _itemContainer;
        [Inject] private DiContainer _container;

        private List<LootView> _lootViewPool = new();
        private List<GoldCoinView> _goldCoinPool = new();

        public LootView CreateLootView(LootType type, Vector3 position)
        {
            LootView view = GetLootView();
            view.transform.position = position;
            view.Initialize(type);
            return view;
        }

        public GoldCoinView CreateGoldCoin(Vector3 spawnPosition, bool merchantDrop)
        {
            GoldCoinView view = GetGoldCoin();
            view.MerchantDrop = merchantDrop;
            view.Initialize(spawnPosition + (Random.onUnitSphere * GoldCoinSpread));
            return view;
        }
        
        public List<GoldCoinView> GetDroppedGoldCoin()
        {
            List<GoldCoinView> droppedCoins = new();
            foreach (GoldCoinView view in _goldCoinPool)
            {
                if(!view.gameObject.activeSelf || !view.MerchantDrop)
                {
                    continue;
                }

                droppedCoins.Add(view);
            }

            return droppedCoins;
        }
        
        public List<LootView> GetXPViews(Tile start, int xp)
        {
            List<LootView> xpOrbs = new();
            for (int i = 0; i < xp; i++)
            {
                LootView view = CreateLootView(LootType.Gold, start.WorldPosition);
                xpOrbs.Add(view);
            }

            return xpOrbs;
        }

        private LootView GetLootView()
        {
            foreach (LootView view in _lootViewPool)
            {
                if(view.gameObject.activeSelf)
                {
                    continue;
                }

                return view;
            }

            LootView newView = _container.InstantiatePrefab(_lootViewPrefab).GetComponent<LootView>();
            _lootViewPool.Add(newView);
            return newView;
        }

        private GoldCoinView GetGoldCoin()
        {
            foreach (GoldCoinView view in _goldCoinPool)
            {
                if(view.gameObject.activeSelf)
                {
                    continue;
                }

                return view;
            }

            GoldCoinView newView = _container.InstantiatePrefab(_goldCoinPrefab).GetComponent<GoldCoinView>();
            _goldCoinPool.Add(newView);
            return newView;
        }
    }
}