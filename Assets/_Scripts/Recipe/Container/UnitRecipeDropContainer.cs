using System.Collections.Generic;
using System.Linq;
using Factory;
using Models;
using UnityEngine;
using Zenject;

namespace Container
{
    [System.Serializable]
    public class UnitRecipeDrop
    {
        public UnitType unitType;
        public RecipeDefinition recipe;
        public float dropChance;
    }
    public class UnitRecipeDropContainer
    {
        private Dictionary<UnitType, UnitRecipeDrop> _recipeDrops = new();
        public UnitRecipeDropContainer(UnitRecipeDrop[] recipeDrops)
        {
            for (int i = 0; i < recipeDrops.Length; i++)
            {
                _recipeDrops[recipeDrops[i].unitType] = recipeDrops[i];
            }
        }

        public RecipeDefinition TryUnlockRecipe(UnitType type)
        {
            if(!_recipeDrops.ContainsKey(type))
                return null;
            
            UnitRecipeDrop drop =_recipeDrops[type];
            bool equipmentUnlocked =
                PersistentPlayerState.UnlockedEquipmentRecipes.FirstOrDefault(recipe =>
                    recipe.Output == drop.recipe.Output) != null;
            bool modUnlocked = PersistentPlayerState.UnlockedModRecipes.FirstOrDefault(recipe =>
                recipe.Output == drop.recipe.Output) != null;

            if (Random.value < drop.dropChance && !equipmentUnlocked && !modUnlocked)
            {
                UnlockRecipe(drop.recipe);
                return drop.recipe;
            }

            return null;
        }

        private void UnlockRecipe(RecipeDefinition definition)
        {
            if(definition.Type == RecipeType.Mod)
                PersistentPlayerState.UnlockedModRecipes.Add(definition);
            else if(definition.Type == RecipeType.Equipment)
                PersistentPlayerState.UnlockedEquipmentRecipes.Add(definition);
            
            Debug.Log($"Unlock {definition.Output}");
        }
    }
}