[System.Serializable]
public class InteractableDefinition : UnitDefinition
{
    public string InteractButtonText;
    
    public InteractableDefinition(InteractableDefinition definition) : base(definition)
    {
        InteractButtonText = definition.InteractButtonText;
    }
}