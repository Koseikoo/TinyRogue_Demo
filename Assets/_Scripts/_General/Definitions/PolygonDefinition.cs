using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PolygonDefinition
{
    public List<Vector3> Polygon;

    public PolygonDefinition(PolygonDefinition definition)
    {
        Polygon = new(definition.Polygon);
    }
}