using System.Collections.Generic;
using Models;
using UnityEngine;

public enum WeaponSkillType
{
    TypeSelection,
    UpgradeSelection
}

public enum SkillName
{
    None,
    DamageUp,
    DashHit,
    
    LongSword,
    PiercingDamage,
    Bleeding,
    
    BroadSword,
    KnockBack,
    Concussion
}

[CreateAssetMenu(menuName = "Skills/WeaponSkill", fileName = "WeaponSkill")]
public class PlayerSkill : ScriptableObject
{
    public SkillName Name;
    [TextArea] public string Description;
    public WeaponSkillType Type;
    public Sprite Sprite;
    public int Cost;
}

