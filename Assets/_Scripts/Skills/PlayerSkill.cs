using System.Collections.Generic;
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

[CreateAssetMenu(fileName = "WeaponSkill")]
public class PlayerSkill : ScriptableObject
{
    public SkillName Name;
    [TextArea] public string Description;
    public List<PlayerSkill> ConnectedSkills;
    public WeaponSkillType Type;
    public Sprite Sprite;
    public int UnlockCost;
}

