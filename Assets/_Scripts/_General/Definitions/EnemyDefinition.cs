using System.Collections.Generic;

[System.Serializable]
public class EnemyDefinition : UnitDefinition
{
    public int AttackRange;
    public int ScanRange;
    public int TurnDelay;
    public List<ModDefinition> Mods;
    public EnemyDefinition(EnemyDefinition definition) : base(definition)
    {
        AttackRange = definition.AttackRange;
        ScanRange = definition.ScanRange;
        TurnDelay = definition.TurnDelay;
        Mods = new();
        for (int i = 0; i < definition.Mods.Count; i++)
        {
            Mods.Add(new(definition.Mods[i]));
        }
    }
}