using Models;
using UnityEngine.Serialization;

public enum RecipeType
{
    Equipment,
    Mod
}

[System.Serializable]
public class RecipeDefinition
{
    public RecipeType Type;
    public ResourceCostDefinition[] Cost;
    public ItemType Output;

    public RecipeDefinition(RecipeDefinition definition)
    {
        Type = definition.Type;
        Cost = definition.Cost;
        Output = definition.Output;
    }

    public Recipe GetRecipeInstance()
    {
        ResourceCost[] cost = new ResourceCost[Cost.Length];
        for (int i = 0; i < Cost.Length; i++)
        {
            cost[i] = Cost[i].GetResourceCost();
        }

        Recipe recipe = new Recipe();
        recipe.Type = Type;
        recipe.Cost = cost;
        recipe.Output = Output;
        return recipe;
    }
}

[System.Serializable]
public class ResourceCostDefinition
{
    public ItemType Type;
    public int Amount;

    public ResourceCost GetResourceCost()
    {
        return new(Type, Amount);
    }
}