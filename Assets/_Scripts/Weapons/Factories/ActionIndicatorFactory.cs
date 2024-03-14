using System.Collections.Generic;
using System.Linq;
using Models;
using UnityEngine;
using Views;
using Zenject;
using UniRx;

namespace Factories
{
    public class ActionIndicatorFactory
    {
        [Inject] private DiContainer _container;
        [Inject] private ActionIndicatorView _actionIndicatorPrefab;

        [Inject] private CameraModel _cameraModel;
        
        private Transform _actionUICanvas;
        private List<ActionIndicatorView> _viewPool = new();

        public ActionIndicatorFactory()
        {
            _actionUICanvas = GameObject.Find("ActionUICanvas").transform;
        }
        
        public ActionIndicatorView CreateActionIndicator(Tile tile)
        {
            CleanupViewPool();
            ActionIndicatorView view = _viewPool.FirstOrDefault(view => !view.gameObject.activeSelf);
            if (view == null)
            {
                view = _container.InstantiatePrefab(_actionIndicatorPrefab, _actionUICanvas)
                    .GetComponent<ActionIndicatorView>();
                _viewPool.Add(view);
                view.Initialize();
            }
            return view;
        }

        private void CleanupViewPool()
        {
            for (int i = 0; i < _viewPool.Count; i++)
            {
                if (_viewPool[i] == null)
                {
                    _viewPool.Clear();
                    return;
                }
            }
        }
    }
}