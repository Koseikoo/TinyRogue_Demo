using System.Collections.Generic;
using Modals;
using Models;
using UniRx;
using UnityEngine;

public static class PersistentPlayerState
{
    public static IntReactiveProperty Heritage = new();
    public static IntReactiveProperty CurrentRunHeritage = new();

    public static List<RecipeDefinition> UnlockedModRecipes = new();
    public static List<RecipeDefinition> UnlockedEquipmentRecipes = new();
    
    public static List<IslandInfo> CurrentIslands = new();

    public static int WeaponSlots;

    public static void IncreaseHeritage(UnitType type, int level)
    {
        CurrentRunHeritage.Value += level + 1;
    }

    public static void ApplyHeritage()
    {
        Debug.Log($"Heritage: {Heritage.Value}, CurrentRun: {CurrentRunHeritage.Value}");
        Heritage.Value += CurrentRunHeritage.Value;
        CurrentRunHeritage.Value = 0;
    }

    public static void Set(int heritage,List<RecipeDefinition> unlockedModRecipes, List<RecipeDefinition> unlockedEquipmentRecipes, int weaponSlots)
    {
        Heritage.Value = heritage;
        UnlockedModRecipes = new(unlockedModRecipes);
        UnlockedEquipmentRecipes = new(unlockedEquipmentRecipes);
        WeaponSlots = weaponSlots;
    }
}