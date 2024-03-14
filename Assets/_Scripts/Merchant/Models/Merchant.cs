using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Models
{
    public class Merchant
    {
        public IntReactiveProperty CurrentTradeGold = new();
        public BoolReactiveProperty InTrade = new();
        public ReactiveProperty<Item> LastTradedItem = new();
        public int LastTradedIndex;
        
        public ReactiveCollection<Slot> SellSlots = new();
        private List<ItemType> _resourcePool = new();
        private List<ItemType> _itemPool = new();
        private bool _shopFilled;


        private Player _player;
        public Player Player => _player;

        public Merchant()
        {
            for (int i = 0; i < 3; i++)
                SellSlots.Add(new());
            
            InitializePools();
        }

        public void StartTrade(Player player)
        {
            _player = player;
            _player.Bag.ShowUI.Value = true;
            InTrade.Value = true;
            PopulateSellSlots();
        }

        public void AddSelectedItemToTrade(int index)
        {
            if(GameStateContainer.SelectedSlot == null)
                return;

            CurrentTradeGold.Value += GameStateContainer.SelectedSlot.Item.Value.Value;
            LastTradedIndex = index;
            LastTradedItem.Value = GameStateContainer.SelectedSlot.Item.Value;
            
            _player.Bag.RemoveItem(GameStateContainer.SelectedSlot.Item.Value.Type);
            GameStateContainer.SelectedSlot.IsSelected.Value = false;
            GameStateContainer.SelectedSlot = null;
        }

        public void EndTrade()
        {
            _player.Bag.AddLoot(new(CurrentTradeGold.Value));
            CurrentTradeGold.Value = 0;
            _player.Bag.ShowUI.Value = false;
            InTrade.Value = false;
        }
        
        private void InitializePools()
        {
            // Resource Pool
            int values = Enum.GetValues(typeof(ItemType)).Length;
            for (int i = 0; i < values; i++)
            {
                ItemType type = (ItemType)i;
                if(type.ToString().ToLower().Contains("resource"))
                    _resourcePool.Add(type);
            }
            
            // Item Pool
            _itemPool.Add(ItemType.HealthPotion);
        }
        
        private void PopulateSellSlots()
        {
            if(_shopFilled)
                return;

            _shopFilled = true;
            
            SellSlots[0].SetItem(_itemPool.PickRandom().GetItemInstance(1));
            for (int i = 1; i < SellSlots.Count; i++)
            {
                ItemType resource = _resourcePool.PickRandom();
                SellSlots[i].SetItem(resource.GetResourceInstance(1));
            }
        }
    }
}