using System.Collections.Generic;
using Models;
using UnityEngine.Serialization;
using Views;

[System.Serializable]
public class UnitDefinition
{
    public string name;
    
    public UnitType Type;
    public UnitView Prefab;
    public int MaxHealth;
    public bool Invincible;
    public int DropXp;
    public List<ItemDefinition> ItemsToDrop = new();
    
    public UnitDefinition(UnitDefinition definition)
    {
        Type = definition.Type;
        MaxHealth = definition.MaxHealth;
        Invincible = definition.Invincible;
        DropXp = definition.DropXp;
        Prefab = definition.Prefab;
        ItemsToDrop = new(definition.ItemsToDrop);
    }
}