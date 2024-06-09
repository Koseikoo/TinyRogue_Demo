using System;
using UnityEngine;

public static class Utils
{
    public static Bounds GetBounds(this Vector3[] polygon)
    {
        Vector3 minPosition = default;
        Vector3 maxPosition = default;

        for (int i = 0; i < polygon.Length; i++)
        {
            if (polygon[i].x < minPosition.x)
                minPosition.x = polygon[i].x;
        
            if (polygon[i].z < minPosition.z)
                minPosition.z = polygon[i].z;
        
        
            if (polygon[i].x > maxPosition.x)
                maxPosition.x = polygon[i].x;
        
            if (polygon[i].z > maxPosition.z)
                maxPosition.z = polygon[i].z;
        }

        var center = Vector3.Lerp(minPosition, maxPosition, .5f);

        Bounds bounds = new(center, maxPosition - minPosition);
        return bounds;
    }

    public static Bounds GetBounds(this float size)
    {
        Vector3 minPosition = new Vector3(-size, 0, -size);
        Vector3 maxPosition = new Vector3(size, 0, size);

        return new Bounds(Vector3.zero, maxPosition - minPosition);
    }
    
    public static bool IsInsidePolygon(this Vector3 position, Vector3[] polygon)
    {
        int n = polygon.Length;
        bool inside = false;
        Vector3 p1, p2;

        Vector3 p0 = polygon[n - 1];

        for (int i = 0; i < n; i++)
        {
            p1 = polygon[i];
            p2 = polygon[(i + 1) % n];

            bool inYRange = position.z > Math.Min(p1.z, p2.z) &&
                            position.z <= Math.Max(p1.z, p2.z) &&
                            position.x <= Math.Max(p1.x, p2.x) &&
                            p1.z != p2.z;

            if (inYRange)
            {
                double xIntersection = (position.z - p1.z) * (p2.x - p1.x) / (p2.z - p1.z) + p1.x;
                if (p1.x == p2.x || position.x <= xIntersection)
                {
                    inside = !inside;
                }
            }

            p0 = p1;
        }

        return inside;
    }
}