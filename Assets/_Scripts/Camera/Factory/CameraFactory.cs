using Models;
using UnityEngine;
using Views;
using Zenject;

namespace Factories
{
    public class CameraFactory
    {
        [Inject] private DiContainer _container;
        [Inject] private CameraModel _cameraModel;
        [Inject] private CameraView _cameraPrefab;
        [Inject] private CameraRotationUIView _cameraRotationUIPrefab;

        private readonly Transform _playerUICanvas;

        public CameraFactory()
        {
            _playerUICanvas = GameObject.Find("PlayerUICanvas").transform;
        }

        public void CreateCamera(Player player)
        {
            var view = CreateView(player);
            CreateRotationUI(player, view.transform);
        }

        private CameraView CreateView(Player player)
        {
            CameraView view = _container.InstantiatePrefab(_cameraPrefab).GetComponent<CameraView>();
            view.Initialize(player);
            return view;
        }

        private void CreateRotationUI(Player player, Transform cameraAnchor)
        {
            CameraRotationUIView view = _container.InstantiatePrefab(_cameraRotationUIPrefab, _playerUICanvas)
                .GetComponent<CameraRotationUIView>();
            view.Initialize(player, cameraAnchor);
        }
    }
}