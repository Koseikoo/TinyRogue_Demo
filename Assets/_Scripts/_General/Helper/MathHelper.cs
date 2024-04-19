using UnityEngine;

public static class MathHelper
{
    private const int CurveLengthSegments = 20;
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
    
    public static float BezierCurveLength(Vector3 a, Vector3 p0, Vector3 b)
    {
        float length = 0;
        float lastT = 0;

        for (int i = 0; i < CurveLengthSegments; i++)
        {
            float t = (float)i / (CurveLengthSegments - 1);
            Vector3 startPoint = BezierLerp(a, p0, b, lastT);
            Vector3 endPoint = BezierLerp(a, p0, b, t);
            length += Vector3.Distance(startPoint, endPoint);
            lastT = t;
        }

        return length;
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
    
    public static Vector3 GetIntersectionPoint(Vector3 start, Vector3 direction)
    {
        float t = -start.y / direction.y;
        Vector3 intersectionPoint = start + t * direction;

        return intersectionPoint;
    }
    
    public static Vector3 CatmullLerp(this Vector3[] points, float t)
    {
        int numSections = points.Length - 1;
        int currentIndex = Mathf.FloorToInt(t * numSections);
        float u = t * numSections - currentIndex;

        Vector3 p0 = points[Mathf.Clamp(currentIndex - 1, 0, numSections)];
        Vector3 p1 = points[currentIndex];
        Vector3 p2 = points[Mathf.Clamp(currentIndex + 1, 0, numSections)];
        Vector3 p3 = points[Mathf.Clamp(currentIndex + 2, 0, numSections)];

        return CatmullRomInterpolation(p0, p1, p2, p3, u);
    }
    
    public static float[] GetCatmullSegmentLength(this Vector3[] points, int numSamples = 15)
    {
        int numSections = points.Length - 1;
        float[] segmentLengths = new float[numSections];

        for (int i = 0; i < numSections; i++)
        {
            Vector3 p0 = points[Mathf.Clamp(i - 1, 0, numSections)];
            Vector3 p1 = points[i];
            Vector3 p2 = points[Mathf.Clamp(i + 1, 0, numSections)];
            Vector3 p3 = points[Mathf.Clamp(i + 2, 0, numSections)];

            float length = 0f;

            for (int j = 0; j < numSamples; j++)
            {
                float t0 = (float)j / numSamples;
                float t1 = (float)(j + 1) / numSamples;
                Vector3 pt0 = CatmullRomInterpolation(p0, p1, p2, p3, t0);
                Vector3 pt1 = CatmullRomInterpolation(p0, p1, p2, p3, t1);
                length += Vector3.Distance(pt0, pt1);
            }

            segmentLengths[i] = length;
        }

        return segmentLengths;
    }
    
    public static float[] GetCatmullSegmentLengthCumulative(this Vector3[] points, int numSamples = 15)
    {
        int numSections = points.Length - 1;
        float[] segmentLengths = new float[numSections];
        float length = 0f;

        for (int i = 0; i < numSections; i++)
        {
            Vector3 p0 = points[Mathf.Clamp(i - 1, 0, numSections)];
            Vector3 p1 = points[i];
            Vector3 p2 = points[Mathf.Clamp(i + 1, 0, numSections)];
            Vector3 p3 = points[Mathf.Clamp(i + 2, 0, numSections)];

            for (int j = 0; j < numSamples; j++)
            {
                float t0 = (float)j / numSamples;
                float t1 = (float)(j + 1) / numSamples;
                Vector3 pt0 = CatmullRomInterpolation(p0, p1, p2, p3, t0);
                Vector3 pt1 = CatmullRomInterpolation(p0, p1, p2, p3, t1);
                length += Vector3.Distance(pt0, pt1);
            }

            segmentLengths[i] = length;
        }

        return segmentLengths;
    }
    
    public static AnimationCurve GetLerpedCurve(AnimationCurve minCurve, AnimationCurve maxCurve, float curveLerp)
    {
        int keyCount = Mathf.Min(minCurve.length, maxCurve.length);
        AnimationCurve lerpedCurve = new AnimationCurve();

        for (int i = 0; i < keyCount; i++)
        {
            Keyframe keyA = minCurve[i];
            Keyframe keyB = maxCurve[i];

            float inTangent = Mathf.Lerp(keyA.inTangent, keyB.inTangent, curveLerp);
            float outTangent = Mathf.Lerp(keyA.outTangent, keyB.outTangent, curveLerp);
            float inWeight = Mathf.Lerp(keyA.inWeight, keyB.inWeight, curveLerp);
            float outWeight = Mathf.Lerp(keyA.outWeight, keyB.outWeight, curveLerp);
            float time = Mathf.Lerp(keyA.time, keyB.time, curveLerp);
            float value = Mathf.Lerp(keyA.value, keyB.value, curveLerp);

            Keyframe lerpedKey = new Keyframe(time, value);
            lerpedKey.inTangent = inTangent;
            lerpedKey.outTangent = outTangent;
            lerpedKey.inWeight = inWeight;
            lerpedKey.outWeight = outWeight;
            
            lerpedCurve.AddKey(lerpedKey);
        }

        return lerpedCurve;
    }

    private static Vector3 CatmullRomInterpolation(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return 0.5f * (
            (2f * p1) +
            (-p0 + p2) * t +
            (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t +
            (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t
        );
    }
}