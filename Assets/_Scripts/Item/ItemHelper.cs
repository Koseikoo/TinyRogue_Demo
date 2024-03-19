using System;
using Models;
using UnityEngine;

public static class ItemHelper
{
    public static int GetValue(this ItemType type)
    {
        return type switch
        {
            ItemType.DamageMod => 5,
            ItemType.TestRelic => 10,
            ItemType.HealthPotion => 20,
            ItemType.MonsterResource => 2,
            ItemType.PlantResource => 2,
            ItemType.StoneResource => 2,
            ItemType.WoodResource => 2,
            _ => throw new Exception($"No Value for {type} defined!")
        };
    }

    public static bool IsMod(this ItemType type)
    {
        return type.ToString()
            .ToLower()
            .Contains("mod");
    }
    
    public static Mod GetModInstance(this ItemType type, int power)
    {
        return type switch
        {
            ItemType.DamageMod => new DamageMod(type, 1, power),
            ItemType.PoisonMod => new PoisonMod(type, 1, power),
            _ => throw new Exception($"No Mod of Type {type}")
        };
    }
    
    public static Resource GetResourceInstance(this ItemType type, int amount)
    {
        return type switch
        {
            ItemType.MonsterResource => new Resource(type, amount, ResourceType.Monster),
            ItemType.PlantResource => new Resource(type, amount, ResourceType.Plant),
            ItemType.StoneResource => new Resource(type, amount, ResourceType.Stone),
            ItemType.WoodResource => new Resource(type, amount, ResourceType.Wood),
            _ => throw new Exception($"No Resource of Type {type}")
        };
    }
    
    public static Equipment GetEquipmentInstance(this ItemType type)
    {
        return type switch
        {
            ItemType.StartArmorEquipment => new Equipment(type),
            ItemType.StartFaceEquipment => new Equipment(type),
            ItemType.StartHelmetEquipment => new Equipment(type),
            _ => throw new Exception($"No Resource of Type {type}")
        };
    }
    
    public static Item GetItemInstance(this ItemType type, int amount)
    {
        return new Item(type, amount);
    }

    public static Action GetItemUseAction(this ItemType type)
    {
        return type switch
        {
            ItemType.HealthPotion => () => GameStateContainer.Player.Health.Value =
                Mathf.Min(GameStateContainer.Player.Health.Value + 3, GameStateContainer.Player.MaxHealth),
            _ => null
        };
    }
    
    
}