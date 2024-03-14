using UniRx;

namespace Models
{
    public class Recipe
    {
        public RecipeType Type;
        public ResourceCost[] Cost;
        public ItemType Output;

        public FloatReactiveProperty CraftProgress01 = new();
    }

    public struct ResourceCost
    {
        public ItemType Type;
        public int Amount;
        
        public ResourceCost(ItemType type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }
}