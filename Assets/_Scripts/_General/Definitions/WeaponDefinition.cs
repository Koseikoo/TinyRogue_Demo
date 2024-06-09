using System.Collections.Generic;

[System.Serializable]
public class WeaponDefinition
{
    public int Range;
    public int MaxAttackCharges;
    public int MaxMoveCharges;
    public List<ModDefinition> Mods;
}