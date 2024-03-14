using UnityEngine;

public static class MathHelper
{
    public static Vector3 GetClosestPointOnLine(Vector3 linePoint1, Vector3 linePoint2, Vector3 pointA)
    {
        Vector3 lineDirection = linePoint2 - linePoint1;
        float t = Vector3.Dot(pointA - linePoint1, lineDirection) / Vector3.Dot(lineDirection, lineDirection);
        t = Mathf.Clamp01(t);

        Vector3 closestPoint = linePoint1 + t * lineDirection;
        return closestPoint;
    }
    
    public static Vector3 BezierLerp(Vector3 a, Vector3 p0, Vector3 b, float t)
    {
        Vector3 ap0 = Vector3.Lerp(a, p0, t);
        Vector3 p0b = Vector3.Lerp(p0, b, t);
        Vector3 ab = Vector3.Lerp(ap0, p0b, t);
        return ab;
    }

    public static Vector3 CubicBezierLerp(Vector3 a, Vector3 p0, Vector3 p1, Vector3 b, float t)
    {
        Vector3 ap1 = BezierLerp(a, p0, p1, t);
        Vector3 p0b = BezierLerp(p0, p1, b, t);
        Vector3 ab = Vector3.Lerp(ap1, p0b, t);
        return ab;
    }
    
    public static Vector3 RotatePointAroundPoint(Vector3 point, Vector3 pivot, Vector3 axis, float angle)
    {
        Vector3 translatedPoint = point - pivot;
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        Vector3 rotatedTranslatedPoint = rotation * translatedPoint;
        Vector3 rotatedPoint = rotatedTranslatedPoint + pivot;

        return rotatedPoint;
    }

    public static Vector3 RotateVector(this Vector3 vector, Vector3 axis, float angle)
    {
        Quaternion rotation = Quaternion.AngleAxis(angle, axis);
        Vector3 rotatedVector = rotation * vector;
        return rotatedVector;
    }

    public static Vector2 RotateVector(this Vector2 vector, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        Vector2 rotatedVector = rotation * vector;
        return rotatedVector;
    }
}