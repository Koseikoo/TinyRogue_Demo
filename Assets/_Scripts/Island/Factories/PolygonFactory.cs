using System.Collections.Generic;
using UnityEngine;

namespace Factories
{
    public class PolygonFactory
    {
        public Vector3[] Create(PolygonConfig config)
        {
            float lastOffsetValue = 0;
            Vector3[] polygon = new Vector3[config.Points];
            for (int i = 0; i < config.Points; i++)
            {
                float angle = 360 * ((float)i / config.Points);
                var baseVector = Vector2.right;

                lastOffsetValue = Mathf.Lerp(lastOffsetValue, Random.value, 1-config.LastPointWeight);
                var distortValue = lastOffsetValue * config.Distortion;
                baseVector += Vector2.right * distortValue;
            
                baseVector = RotateVector(baseVector, angle);
                polygon[i] = new Vector3(baseVector.x, 0f, baseVector.y);
                polygon[i] *= config.Size;
            }

            return polygon;
        }

        public Vector3[] Create(float size)
        {
            Vector3[] polygon = new Vector3[8];
            var baseVector = Vector2.right * size;

            for (int i = 0; i < polygon.Length; i++)
            {
                float angle = 360 * ((float)i / polygon.Length);
                var rotatedVector = RotateVector(baseVector, angle);
                polygon[i] = new Vector3(rotatedVector.x, 0f, rotatedVector.y);
            }

            return polygon;
        }
        
        
        
        Vector2 RotateVector(Vector2 vector, float angleDegrees)
        {
            float radians = angleDegrees * Mathf.Deg2Rad;
            
            float sinTheta = Mathf.Sin(radians);
            float cosTheta = Mathf.Cos(radians);

            float newX = vector.x * cosTheta - vector.y * sinTheta;
            float newY = vector.x * sinTheta + vector.y * cosTheta;

            return new Vector2(newX, newY);
        }
    }
}