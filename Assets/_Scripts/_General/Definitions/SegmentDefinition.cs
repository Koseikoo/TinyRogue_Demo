using System;
using System.Collections.Generic;
using Models;
using UnityEngine;
using Views;
using Zenject;

[System.Serializable]
public class SegmentDefinition
{
    public SegmentView Prefab;

    public SegmentDefinition(SegmentDefinition definition)
    {
        Prefab = definition.Prefab;
    }
}