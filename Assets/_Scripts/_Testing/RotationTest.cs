using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Testing
{
    public class RotationTest : MonoBehaviour
    {
        public bool Reset;
        public bool Rotate;
        
        [FormerlySerializedAs("anchor")] [SerializeField] private Transform pivot;
        [SerializeField] private Transform point;
        [SerializeField] private Transform axis;
        [SerializeField] private float angle;

        private Quaternion startPointRotation;
        private Vector3 startPointPosition;

        private void Awake()
        {
            startPointRotation = point.rotation;
            startPointPosition = point.position;
        }

        private void Update()
        {
            if (Reset)
            {
                Reset = false;
                ResetTest();
            }

            if (Rotate)
            {
                Rotate = false;
                var newPoint = RotatePointAroundPoint(point.position, pivot.position, axis.forward, angle);
                point.position = newPoint;
            }
        }

        private void ResetTest()
        {
            point.rotation = startPointRotation;
            point.position = startPointPosition;
        }

        private void RotatePoint()
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, axis.forward);
            point.rotation *= rotation;
        }
        
        Vector3 RotatePointAroundPoint(Vector3 point, Vector3 pivot, Vector3 axis, float angle)
        {
            Vector3 translatedPoint = point - pivot;
            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            Vector3 rotatedTranslatedPoint = rotation * translatedPoint;
            Vector3 rotatedPoint = rotatedTranslatedPoint + pivot;

            return rotatedPoint;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(axis.position + axis.forward * -50, axis.position + axis.forward * 50);
        }
    }
}