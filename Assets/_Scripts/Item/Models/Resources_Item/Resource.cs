namespace Models
{
    public enum ResourceType
    {
        Monster,
        Plant,
        Stone,
        Wood
    }
    
    public class Resource : Item
    {
        public readonly ResourceType ResourceType;
        
        public Resource(ItemType type, int stack, ResourceType resourceType) : base(type, stack)
        {
            ResourceType = resourceType;
        }
    }
}