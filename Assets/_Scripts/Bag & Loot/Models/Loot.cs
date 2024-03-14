using System.Collections.Generic;
using UnityEngine;

// i love you hunni
namespace Models
{
    public class Loot
    {
        public int Gold;
        public List<Mod> Mods;
        public List<Item> Items;
        public List<Equipment> Equipment;
        public List<Resource> Resources;
        public Vector3 DropPosition;
        public Unit RewardedTo;
        public bool DelayDrop;

        public Loot(int gold, List<Mod> mods = null, List<Item> items = null, List<Resource> resources = null, List<Equipment> equipments = null)
        {
            Gold = gold;
            Mods = mods ?? new();
            Items = items ?? new();
            Resources = resources ?? new();
            Equipment = equipments ?? new();
        }

        public void RewardTo(Unit rewardTo, Vector3 dropPosition, bool delayDrop)
        {
            DelayDrop = delayDrop;
            DropPosition = dropPosition;
            RewardedTo = rewardTo;
            if(DelayDrop)
                IncreaseLootBasedOnCombo(rewardTo);
            IslandLootContainer.AddToLootDrops(this);
        }

        private void IncreaseLootBasedOnCombo(Unit unit)
        {
            if (unit is Player player)
            {
                int combo = player.Weapon.ActiveCombo.Count;
                Gold += combo;
            }
        }
    }
}