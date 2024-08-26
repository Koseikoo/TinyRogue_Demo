using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

public class WeaponSkillHelper
{
    private static Dictionary<SkillName, Action<WeaponData>> skillDict = new()
    {
        {SkillName.None, null},
        {SkillName.DamageUp, OnDamageUp},
        {SkillName.LongSword, OnLongSword},
        {SkillName.BroadSword, OnBroadSword},
        {SkillName.PiercingDamage, OnPiercingDamage},
        {SkillName.Bleeding, OnBleeding},
        {SkillName.KnockBack, OnKnockBack},
        {SkillName.Concussion, OnConcussion},
        {SkillName.DashHit, OnDashHit}
    };

    private static void OnDashHit(WeaponData data)
    {
        
    }

    public static Action<WeaponData> GetUnlockAction(WeaponSkill skill)
    {
        Action<WeaponData> action = skillDict[skill.Name];
        return action;
    }

    private static void OnDamageUp(WeaponData data)
    {
        data.Damage++;
    }
    
    private static void OnLongSword(WeaponData data)
    {
        data.SetAttackPattern(LongSwordPattern);
    }

    private static void OnBroadSword(WeaponData data)
    {
        data.SetAttackPattern(BroadSwordPattern);
    }

    private static void OnPiercingDamage(WeaponData data)
    {
        data.Piercing = true;
    }

    private static void OnBleeding(WeaponData data)
    {
        
    }

    private static void OnKnockBack(WeaponData data)
    {
        
    }

    private static void OnConcussion(WeaponData data)
    {
        
    }
    
    public static List<Tile> BasePattern(Vector3 direction)
    {
        List<Tile> pattern = direction.GetTilesInDirection(1);
        return pattern;
    }
        
    public static List<Tile> LongSwordPattern(Vector3 direction)
    {
        List<Tile> pattern = direction.GetTilesInDirection(2);
        return pattern;
    }
        
    public static List<Tile> BroadSwordPattern(Vector3 direction)
    {
        Vector3 leftDirection = direction.RotateVector(Vector3.up, -60);
        Vector3 rightDirection = direction.RotateVector(Vector3.up, 60);
            
        List<Tile> pattern = new()
        {
            leftDirection.GetTileInDirection(),
            direction.GetTileInDirection(),
            rightDirection.GetTileInDirection()
        };
        return pattern;
    }
}