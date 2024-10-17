using System;
using System.Collections.Generic;
using Models;
using UnityEngine;
using AnimationState = Models.AnimationState;

public static class EnemyHelper
{
    public static void AttackUnit(this Enemy enemy, GameUnit gameUnit)
    {
        Vector3 attackDirection = gameUnit.Tile.Value.FlatPosition - enemy.Tile.Value.FlatPosition;
        gameUnit.Attack(enemy.ModSlots.GetMods(), attackDirection, enemy);
    }
    
    public static void TryAttackTarget(this Enemy enemy, List<Tile> attackPath, bool damageAllTiles = false)
    {
        for (int i = 0; i < attackPath.Count; i++)
        {
            if (attackPath[i].HasUnit)
            {
                enemy.AttackUnit(attackPath[i].Unit.Value);
                if(!damageAllTiles)
                {
                    break;
                }
            }
                    
        }
    }

    public static bool HasAnyUnit(this List<Tile> path, Func<Tile, bool> tileCondition = null)
    {
        tileCondition ??= tile => tile.HasUnit;
        for (int i = 0; i < path.Count; i++)
        {
            if (tileCondition(path[i]))
            {
                return true;
            }
        }

        return false;
    }
    
    public static bool HasUnit(this List<Tile> path, GameUnit gameUnit)
    {
        for (int i = 0; i < path.Count; i++)
        {
            if (path[i].Unit.Value == gameUnit)
            {
                return true;
            }
        }

        return false;
    }
    
    public static void FollowTarget(this Enemy enemy, Tile nextTile)
    {
        if (nextTile.HasUnit)
        {
            enemy.AttackUnit(nextTile.Unit.Value);
            enemy.AnimationCommand.Execute(AnimationState.Attack1);
        }
        else
        {
            nextTile.MoveUnitWithAction(enemy);
        }
    }
}