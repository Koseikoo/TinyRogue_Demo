using System.Collections.Generic;
using Models;
using UnityEngine;

public static class SegmentHelper
{
    public static bool IsInsideSegment(this Segment segment, List<Segment> segments)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (Vector3.Distance(segments[i].CenterTile.WorldPosition, segment.CenterTile.WorldPosition) < segments[i].Radius.GetSegmentDistance(segment.Radius))
                return true;
        }
        return false;
    }
        
    public static bool IsWithinPolygon(this Vector3 position, Vector3[] polygon, float radius)
    {
        int numPointsOnCircle = 16;

        for (int i = 0; i < numPointsOnCircle; i++)
        {
            float angle = 2 * Mathf.PI * i / numPointsOnCircle;
            float x = position.x + radius * Mathf.Cos(angle);
            float z = position.z + radius * Mathf.Sin(angle);

            Vector3 pointOnCircle = new Vector3(x, position.y, z);
            if (!pointOnCircle.IsInsidePolygon(polygon))
                return false;
        }
        return true;
    }
}