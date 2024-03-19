using System;
using Models;
using UnityEngine;

public static class LootHelper
{
    public static void AddToDroppedLoot(this Item item, Vector3 dropPosition, Unit rewardTo = null)
    {
        if (item == null)
            throw new Exception("item is null!");
        
        Loot loot = new(0);
        
        if(item is Mod mod)
            loot.Mods.Add(mod);
        else if(item is Equipment equipment)
            loot.Equipment.Add(equipment);
        else if(item is Resource resource)
            loot.Resources.Add(resource);
        else
            loot.Items.Add(item);
        
        loot.RewardTo(rewardTo ?? GameStateContainer.Player, dropPosition);
    }
}