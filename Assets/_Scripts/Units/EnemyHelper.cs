using System;
using System.Collections.Generic;
using Models;
using UnityEngine;
using AnimationState = Models.AnimationState;

public static class EnemyHelper
{
    public static void AttackUnit(this Enemy enemy, Unit unit)
    {
        Vector3 attackDirection = unit.Tile.Value.WorldPosition - enemy.Tile.Value.WorldPosition;
        unit.Attack(enemy.ModSlots.GetMods(), attackDirection, enemy);
    }
    
    public static void TryAttackTarget(this Enemy enemy, List<Tile> attackPath, bool damageAllTiles = false)
    {
        for (int i = 0; i < attackPath.Count; i++)
        {
            if (attackPath[i].HasUnit)
            {
                enemy.AttackUnit(attackPath[i].CurrentUnit.Value);
                if(!damageAllTiles)
                    break;
            }
                    
        }
    }

    public static bool HasAnyUnit(this List<Tile> path, Func<Tile, bool> tileCondition = null)
    {
        tileCondition ??= tile => tile.HasUnit;
        for (int i = 0; i < path.Count; i++)
        {
            if (tileCondition(path[i]))
                return true;
        }

        return false;
    }
    
    public static bool HasUnit(this List<Tile> path, Unit unit)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].CurrentUnit.Value == unit)
                return true;
        }

        return false;
    }
    
    public static void FollowTarget(this Enemy enemy, Tile nextTile)
    {
        if (nextTile.HasUnit)
        {
            enemy.AttackUnit(nextTile.CurrentUnit.Value);
            enemy.AnimationCommand.Execute(AnimationState.Attack1);
        }
        else
        {
            nextTile.MoveUnit(enemy);
        }
    }
}