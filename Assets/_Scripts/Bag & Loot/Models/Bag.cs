using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Models
{
    public class Bag
    {
        private const int CategorySlots = 20;
        public Unit Owner { get; private set; }
        public IntReactiveProperty Gold = new();
        public ReactiveCommand<Loot> OnLootAdded = new();
        

        public List<Slot> Mods = new();
        public List<Slot> Items = new();
        public List<Slot> Resources = new();
        public List<Slot> Equipment = new();
        private List<Slot> allSlots = new();

        public BoolReactiveProperty ShowUI = new();

        private Transform _bagPoint;
        public Transform BagPoint => _bagPoint;

        public bool HasGold(int gold) => Gold.Value >= gold;
        
        public Bag(Unit owner)
        {
            Owner = owner;

            for (int i = 0; i < CategorySlots; i++)
            {
                Mods.Add(new());
                Items.Add(new());
                Resources.Add(new());
                Equipment.Add(new());
            }
            
            allSlots.AddRange(Mods);
            allSlots.AddRange(Items);
            allSlots.AddRange(Resources);
            allSlots.AddRange(Equipment);
        }
        
        public void SetBagPoint(Transform point)
        {
            _bagPoint = point;
        }

        public void AddLoot(Loot loot)
        {
            Gold.Value += loot.Gold;
            
            foreach (Mod mod in loot.Mods)
            {
                var slot = Mods.FirstFreeSlot();
                if (slot == null)
                    throw new Exception("Mods are Full");
                
                slot.SetItem(mod);
            }
            
            foreach (Item item in loot.Items)
            {
                var slot = Items.FirstWithType(item.Type);
                if (slot == null)
                    slot = Items.FirstFreeSlot();
                
                if(slot == null)
                    throw new Exception("Items are Full");
                
                slot.SetItem(item);
            }

            foreach (Resource resource in loot.Resources)
            {
                var slot = Resources.FirstWithType(resource.Type);
                if (slot == null)
                    slot = Resources.FirstFreeSlot();
                
                if (slot == null)
                    throw new Exception("Resources are Full");
                
                slot.SetItem(resource);
            }
            
            foreach (Equipment equipment in loot.Equipment)
            {
                var slot = Equipment.FirstWithType(equipment.Type);
                if (slot == null)
                    slot = Equipment.FirstFreeSlot();
                
                if (slot == null)
                    throw new Exception("Equipments are Full");
                
                slot.SetItem(equipment);
            }
            
            IslandLootContainer.DroppedLoot.Remove(loot);
            OnLootAdded.Execute(loot);
        }

        public void RemoveGold(int gold)
        {
            Gold.Value -= gold;
        }

        public void RemoveItem(ItemType type, int amount = 1)
        {
            var temp = Items.FirstWithType(type);

            if (temp == null)
                temp = Mods.FirstWithType(type);
            
            if (temp == null)
                temp = Resources.FirstWithType(type);
            
            if (temp == null)
                throw new Exception("Item not in Bag");
            
            temp.RemoveAmount(amount);
        }
    }
}