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
        public GameUnit RewardedTo;

        public Loot(int gold, List<Mod> mods = null, List<Item> items = null, List<Resource> resources = null, List<Equipment> equipments = null)
        {
            Gold = gold;
            Mods = mods ?? new();
            Items = items ?? new();
            Resources = resources ?? new();
            Equipment = equipments ?? new();
        }

        public void RewardTo(GameUnit rewardTo, Vector3 dropPosition)
        {
            DropPosition = dropPosition;
            RewardedTo = rewardTo;
            WorldLootContainer.AddToLootDrops(this);
        }
    }
}