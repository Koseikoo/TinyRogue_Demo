using System;
using System.Collections.Generic;
using Models;
using UnityEngine;
using Views;
using Zenject;

[System.Serializable]
public class SegmentDefinition
{
    public SegmentType Type;
    public float Radius;
    public int MaxUnits;
    public SegmentView Prefab;
    public List<SegmentUnitDefinition> SegmentUnitDefinitions;

    public SegmentDefinition(SegmentDefinition definition)
    {
        Type = definition.Type;
        Radius = definition.Radius;
        MaxUnits = definition.MaxUnits;
        Prefab = definition.Prefab;
        SegmentUnitDefinitions = new(definition.SegmentUnitDefinitions);
    }

    public Segment CreateSegment(DiContainer container)
    {
        return Type switch
        {
            SegmentType.Forrest => container.Instantiate<DefeatSegment>(new object[]{this}),
            SegmentType.EnemyCamp => container.Instantiate<DefeatSegment>(new object[]{this}),
            SegmentType.Village => container.Instantiate<DefeatSegment>(new object[]{this}),
            SegmentType.Start => container.Instantiate<Segment>(new object[]{this}),
            SegmentType.End => container.Instantiate<Segment>(new object[]{this}),
            SegmentType.Ruin => container.Instantiate<DefeatSegment>(new object[]{this}),
            SegmentType.Boss => container.Instantiate<DefeatSegment>(new object[]{this}),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}