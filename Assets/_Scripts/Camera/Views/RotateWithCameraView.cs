using System;
using UnityEngine;

namespace Views
{
    public class RotateWithCameraView : MonoBehaviour
    {
        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        private void Update()
        {
            if(!gameObject.activeSelf)
                return;
            
            _transform.eulerAngles = new Vector3(_transform.eulerAngles.x,
                GameStateContainer.Player.AnchorYRotation.Value, 0f);
        }
    }
}