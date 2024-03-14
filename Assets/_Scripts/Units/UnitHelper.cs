using System.Collections.Generic;
using Models;
using UniRx;
using UnityEngine;

public static class UnitHelper
{
    private const float LevelHealthScale = .5f;
    private const float LevelModPowerScale = 1f;
    
    public static EnemyDefinition ScaledWithLevel(this EnemyDefinition definition, int level)
    {
        EnemyDefinition scaledDefinition = new(definition);

        scaledDefinition.Unit = definition.Unit.ScaledWithLevel(level);
        for (int i = 0; i < scaledDefinition.Mods.Count; i++)
        {
            scaledDefinition.Mods[i].Power = definition.Mods[i].Power + level;
        }

        return scaledDefinition;
    }
    
    public static UnitDefinition ScaledWithLevel(this UnitDefinition definition, int level)
    {
        UnitDefinition scaledDefinition = new(definition);
        scaledDefinition.MaxHealth = definition.MaxHealth + level;
        scaledDefinition.DropXp = definition.DropXp + level;
        return scaledDefinition;
    }

    public static int GetAttackDamage(this IAttacker attacker)
    {
        int damage = 0;

        foreach (Slot slot in attacker.ModSlots)
        {
            if(slot.Item.Value is Mod mod)
                damage += mod.GetModDamage();
        }

        return damage;
    }
}