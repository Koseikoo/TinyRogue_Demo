using System.Collections.Generic;
using UnityEngine;
using Views;

[System.Serializable]

public class SegmentUnitDefinition
{
    public List<Transform> Points;
    public UnitView View;
    public UnitDefinition Unit;
}