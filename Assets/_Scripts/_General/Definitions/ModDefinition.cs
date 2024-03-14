[System.Serializable]
public class ModDefinition
{
    public ItemDefinition ItemDefinition;
    public int Power;

    public ModDefinition(ModDefinition definition)
    {
        ItemDefinition = new(definition.ItemDefinition);
        Power = definition.Power;
    }
}