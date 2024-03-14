
[System.Serializable]
public class EquipmentDefinition
{
    public ItemDefinition ItemDefinition;
    public int AttackBonus;
    public int ArmorBonus;
    public int HealthBonus;

    public ModDefinition InfusedModDefinition;

    public EquipmentDefinition(EquipmentDefinition definition)
    {
        ItemDefinition = new(definition.ItemDefinition);
        AttackBonus = definition.AttackBonus;
        ArmorBonus = definition.ArmorBonus;
        HealthBonus = definition.HealthBonus;
        InfusedModDefinition = new(definition.InfusedModDefinition);
    }
}