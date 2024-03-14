using System.Collections.Generic;
using Models;

[System.Serializable]
public class TrapDefinition
{
    public List<ModDefinition> Mods;

    public TrapDefinition(TrapDefinition definition)
    {
        Mods = new();
        for (int i = 0; i < definition.Mods.Count; i++)
        {
            Mods.Add(new(definition.Mods[i]));
        }
    }

    public Trap GetTrapInstance()
    {
        Trap trap = new();
        foreach (ModDefinition mod in Mods)
            trap.AddMod(mod.ItemDefinition.Type.GetModInstance(mod.Power));
        return trap;
    }
}