using System.Collections.Generic;
using Models;
using UnityEngine;

public static class WeaponHelper
{
    private const int BaseXp = 10;
    private const float NextLevelMult = 1.2f;

    public static int GetLevel(int xp)
    {
        int level = 1;
        int nextLevelXp = GetLevelXp(level);

        while (xp >= nextLevelXp)
        {
            level++;
            nextLevelXp = GetLevelXp(level);
        }
        
        return level;
    }

    /// <summary>
    /// Get Required XP To Reach Next Level
    /// </summary>
    public static int GetLevelXp(int level)
    {
        level++;
        if (level == 1)
        {
            return 0;
        }

        int xp = BaseXp;
        for (int i = 1; i < level; i++)
        {
            xp += Mathf.RoundToInt(xp * NextLevelMult);
        }

        return xp;
    }
    
    public static List<Tile> BasePattern(Vector3 direction)
    {
        List<Tile> pattern = direction.GetTilesInDirection(1);
        return pattern;
    }
}