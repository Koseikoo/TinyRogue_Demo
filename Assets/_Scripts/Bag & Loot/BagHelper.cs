using System;
using System.Collections.Generic;
using System.Linq;
using Models;

public static class BagHelper
{
    public static bool HasResources(this Bag bag, ItemType type, int amount)
    {
        int currentAmount = 0;
        var matches = bag.Resources.MatchingItems(item => item.Type == type);
        if (matches.Count == 0)
            return false;
        
        for (int i = 0; i < matches.Count; i++)
            currentAmount += matches[i].Stack.Value;

        return currentAmount >= amount;
    }
    public static List<Mod> MatchingMods(this List<Slot> modSlots, Func<Mod, bool> matchCondition)
    {
        List<Mod> matches = new();

        var slots = modSlots.FindAll(slot =>
        {
            if (slot.IsOccupied && slot.Item.Value is Mod mod)
                return matchCondition(mod);

            return false;
        });

        foreach (var slot in slots)
        {
            matches.Add(slot.Item.Value as Mod);
        }

        return matches;
    }
    
    public static List<Item> MatchingItems(this List<Slot> itemSlots, Func<Item, bool> matchCondition)
    {
        List<Item> matches = new();

        var slots = itemSlots.FindAll(slot =>
        {
            if (slot.IsOccupied)
                return matchCondition(slot.Item.Value);

            return false;
        });

        foreach (var slot in slots)
        {
            matches.Add(slot.Item.Value);
        }

        return matches;
    }
    
    public static List<Mod> GetMods(this List<Slot> slots)
    {
        List<Mod> mods = new();

        foreach (var slot in slots)
        {
            if(slot.IsOccupied && slot.Item.Value is Mod mod)
                mods.Add(mod);
        }

        return mods;
    }
    
    public static Slot FirstFreeSlot(this List<Slot> slots)
    {
        var temp = slots.FirstOrDefault(slot => !slot.IsOccupied);
        return temp;
    }

    public static Slot FirstWithType(this List<Slot> slots, ItemType type)
    {
        var temp = slots.FirstOrDefault(slot => slot.IsOccupied && slot.Item.Value.Type == type);
        return temp;
    }
}