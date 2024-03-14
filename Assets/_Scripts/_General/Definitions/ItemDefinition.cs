using System;
using System.Collections.Generic;
using Models;
using UnityEngine;

[System.Serializable]
public class ItemDefinition
{
    public ItemType Type;
    public int Stack;
    public List<UnitType> DroppedFrom;
    public float DropChance;

    public ItemDefinition(ItemDefinition definition)
    {
        Type = definition.Type;
        Stack = definition.Stack;
        DroppedFrom = definition.DroppedFrom == null ? new() : new (definition.DroppedFrom);
        DropChance = definition.DropChance;
    }
}