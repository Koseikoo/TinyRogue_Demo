using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Views
{
    public class UITransformPositioner : MonoBehaviour
    {
        [SerializeField] private Transform ReferenceTransform;

        public Vector3 WorldPosition => ReferenceTransform.position;

        public void Initialize(Transform referenceTransform)
        {
            ReferenceTransform = referenceTransform;
        }

        private void LateUpdate()
        {
            if(ReferenceTransform == null)
                return;
            transform.position = UIHelper.Camera.WorldToScreenPoint(ReferenceTransform.position);
        }
    }
}