using Models;
using UnityEngine;

public static class WeaponHelper
{
    private const int BaseXp = 10;
    private const float NextLevelMult = 1.2f;
    public static int CalculateDamage(this Weapon weapon, Tile tile)
    {
        if (!tile.HasUnit)
            return 0;

        int damage = 0;

        foreach (Slot slot in weapon.ModSlots)
        {
            if (slot.IsOccupied && slot.Item.Value is Mod mod)
            {
                damage += mod.GetModDamage();
            }
        }

        return damage;
    }
    
    public static int GetLevel(int xp)
    {
        int level = 1;
        int nextLevelXp = GetLevelXp(level+1);

        while (xp >= nextLevelXp)
        {
            level++;
            nextLevelXp = GetLevelXp(level+1);
        }
        
        return level;
    }

    public static float Progress(this Weapon weapon)
    {
        int levelXp = GetLevelXp(weapon.Level.Value);
        int nextLevelXp = GetLevelXp(weapon.Level.Value + 1);
        return Mathf.InverseLerp(levelXp, nextLevelXp, weapon.Xp.Value);
    }

    public static int GetLevelXp(int level)
    {
        if (level == 1)
            return 0;
        
        int xp = BaseXp;
        for (int i = 1; i < level; i++)
        {
            xp += Mathf.RoundToInt(xp * NextLevelMult);
        }

        return xp;
    }
}