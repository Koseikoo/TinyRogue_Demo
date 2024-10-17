using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Models
{
    public class Bag
    {
        private const int CategorySlots = 20;
        public GameUnit Owner { get; private set; }
        public IntReactiveProperty Gold = new();
        public ReactiveCommand OnBagItemsChanged = new();


        public List<Slot> Mods = new();
        public List<Slot> Items = new();
        public List<Slot> Resources = new();
        public List<Slot> Equipment = new();
        private List<Slot> allSlots = new();

        public BoolReactiveProperty ShowUI = new();

        private Transform _bagPoint;
        public Transform BagPoint => _bagPoint;

        public bool HasGold(int gold) => Gold.Value >= gold;
        
        public Bag(GameUnit owner)
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

        public void AddGold(int amount)
        {
            Gold.Value += amount;
        }

        public void AddMod(Mod mod)
        {
            Slot slot = Mods.FirstFreeSlot();
            if (slot == null)
            {
                throw new Exception("Mods are Full");
            }

            slot.SetItem(mod);
            OnBagItemsChanged.Execute();
        }

        public void AddItem(Item item)
        {
            Slot slot = Items.FirstWithType(item.Type);
            if (slot == null)
            {
                slot = Items.FirstFreeSlot();
            }

            if(slot == null)
            {
                throw new Exception("Items are Full");
            }

            slot.SetItem(item);
            OnBagItemsChanged.Execute();
        }

        public void AddResource(Resource resource)
        {
            Slot slot = Resources.FirstWithType(resource.Type);
            if (slot == null)
            {
                slot = Resources.FirstFreeSlot();
            }

            if (slot == null)
            {
                throw new Exception("Resources are Full");
            }

            slot.SetItem(resource);
            OnBagItemsChanged.Execute();
        }

        public void AddEquipment(Equipment equipment)
        {
            Slot slot = Equipment.FirstWithType(equipment.Type);
            if (slot == null)
            {
                slot = Equipment.FirstFreeSlot();
            }

            if (slot == null)
            {
                throw new Exception("Equipments are Full");
            }

            slot.SetItem(equipment);
            OnBagItemsChanged.Execute();
        }

        public void AddLoot(Loot loot)
        {
            AddGold(loot.Gold);
            
            foreach (Mod mod in loot.Mods)
                AddMod(mod);
            
            foreach (Item item in loot.Items)
                AddItem(item);

            foreach (Resource resource in loot.Resources)
                AddResource(resource);
            
            foreach (Equipment equipment in loot.Equipment)
                AddEquipment(equipment);
        }

        public int GetSummedItemValue()
        {
            int itemValueSum = allSlots
                .Where(slot => slot.IsOccupied)
                .Sum(slot => slot.Item.Value.Value * slot.Item.Value.Stack.Value);
            return itemValueSum;
        }

        public void RemoveGold(int gold)
        {
            Gold.Value -= gold;
        }

        public void RemoveItem(ItemType type, int amount = 1)
        {
            Slot temp = Items.FirstWithType(type);

            if (temp == null)
            {
                temp = Mods.FirstWithType(type);
            }

            if (temp == null)
            {
                temp = Resources.FirstWithType(type);
            }

            if (temp == null)
            {
                throw new Exception("Item not in Bag");
            }

            temp.RemoveAmount(amount);
            OnBagItemsChanged.Execute();
        }
    }
}