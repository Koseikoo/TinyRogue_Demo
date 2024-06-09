using System;
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
    
    LongSword,
    PiercingDamage,
    Bleeding,
    
    BroadSword,
    KnockBack,
    Concussion
}

[CreateAssetMenu(fileName = "WeaponSkill")]
public class WeaponSkill : ScriptableObject
{
    public SkillName Name;
    [TextArea] public string Description;
    public List<WeaponSkill> ConnectedSkills;
    public WeaponSkillType Type;
    public Sprite Sprite;
    public int UnlockCost;
}

