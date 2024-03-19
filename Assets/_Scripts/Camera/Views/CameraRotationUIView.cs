using System;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Views
{
    public class CameraRotationUIView : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        [SerializeField] private float rotationSpeed;

        private Transform _cameraAnchor;
        private float lastYPosition;
        private Player _player;
        private Quaternion _startRotation;

        public void Initialize(Player player, Transform cameraAnchor)
        {
            _cameraAnchor = cameraAnchor;
            _startRotation = cameraAnchor.rotation;
            _player = player;
            
            GameStateContainer.GameState
                .Where(state => state == GameState.Island && _cameraAnchor != null)
                .Subscribe(_ =>
                {
                    gameObject.SetActive(true);
                })
                .AddTo(this);
            
            GameStateContainer.GameState
                .Where(state => state == GameState.Ship && _cameraAnchor != null)
                .Subscribe(_ =>
                {
                    ResetRotation();
                    gameObject.SetActive(false);
                })
                .AddTo(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(GameStateContainer.LockCameraRotation || GameStateContainer.OpenUI)
                return;
            
            float delta = lastYPosition - eventData.position.y;
            _cameraAnchor.Rotate(delta * rotationSpeed * Vector3.up);
            lastYPosition = eventData.position.y;
            _player.AnchorYRotation.Value = _cameraAnchor.eulerAngles.y;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            lastYPosition = eventData.position.y;
        }

        private void ResetRotation()
        {
            _cameraAnchor.rotation = _startRotation;
            _player.AnchorYRotation.Value = _cameraAnchor.eulerAngles.y;
        }
    }
}