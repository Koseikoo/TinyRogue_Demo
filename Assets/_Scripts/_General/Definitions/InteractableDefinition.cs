[System.Serializable]
public class InteractableDefinition
{
    public UnitDefinition Unit;
    public string InteractButtonText;
    
    public InteractableDefinition(InteractableDefinition definition)
    {
        Unit = new(definition.Unit);
        InteractButtonText = definition.InteractButtonText;
    }
}