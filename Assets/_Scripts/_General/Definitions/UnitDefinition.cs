using Models;
using UnityEngine.Serialization;

[System.Serializable]
public class UnitDefinition
{
    public UnitType Type;
    public int MaxHealth;
    public bool Invincible;
    public int DropXp;
    
    public UnitDefinition(UnitDefinition definition)
    {
        Type = definition.Type;
        MaxHealth = definition.MaxHealth;
        Invincible = definition.Invincible;
        DropXp = definition.DropXp;
    }

    public Unit GetInstance()
    {
        Unit unit = new();
        unit.Type = Type;
        unit.MaxHealth = MaxHealth;
        unit.Health.Value = MaxHealth;
        unit.IsInvincible.Value = Invincible;
        unit.DropXp = DropXp;
        return unit;
    }
}