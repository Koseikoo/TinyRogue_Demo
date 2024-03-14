using System;
using Models;

public static class SlotHelper
{
    public static bool HasFreeSlot(this IAttacker attacker)
    {
        var temp = attacker.ModSlots.FirstFreeSlot();
        return temp != null;
    }

    public static bool HasMod(this IAttacker attacker, Mod mod)
    {
        var temp = attacker.ModSlots.FirstWithType(mod.Type);
        return temp != null;
    }
    
    public static void AddMod(this IAttacker attacker, Mod mod)
    {
        var temp = attacker.ModSlots.FirstFreeSlot();
        if (temp != null)
        {
            temp.SetItem(mod);
        }
        else
        {
            throw new Exception("No Free Slot To Add Mod");
        }
    }

    public static void RemoveMod(this IAttacker attacker, Mod mod)
    {
        var temp = attacker.ModSlots.FirstWithType(mod.Type);
        if (temp != null)
        {
            temp.RemoveItem();
        }
        else
        {
            throw new Exception($"No Slot with Mod {mod.Type}");
        }
    }
}