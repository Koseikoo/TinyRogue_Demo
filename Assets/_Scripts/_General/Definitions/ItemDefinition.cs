using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

[System.Serializable]
public class ItemDefinition
{
    public ItemType Type;
    public int Amount;
    public float DropChance;

    public ItemDefinition(ItemDefinition definition)
    {
        Type = definition.Type;
        Amount = definition.Amount;
        DropChance = definition.DropChance;
    }
}