using Models;
[System.Serializable]
public class ResourceDefinition
{
    public ItemDefinition ItemDefinition;
    public ResourceType ResourceType;

    public ResourceDefinition(ResourceDefinition definition)
    {
        ItemDefinition = new(definition.ItemDefinition);
        ResourceType = definition.ResourceType;
    }
}