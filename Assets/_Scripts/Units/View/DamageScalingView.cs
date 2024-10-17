using System;
using DG.Tweening;
using Models;
using UniRx;
using UnityEngine;

namespace Views
{
    public class DamageScalingView : MonoBehaviour, IUnitViewInitialize
    {
        [SerializeField] private float maxScale;
        [SerializeField] private float scaleDuration;
        [SerializeField] private AnimationCurve scaleCurve;
        private GameUnit _gameUnit;

        public void Initialize(GameUnit gameUnit)
        {
            _gameUnit = gameUnit;

            _gameUnit.Health.Subscribe(_ => UpdateScale()).AddTo(this);
        }

        private void UpdateScale()
        {
            if (_gameUnit.Health.Value <= 0)
            {
                return;
            }

            float scale = Mathf.Lerp(maxScale, 1f, (float)_gameUnit.Health.Value / _gameUnit.MaxHealth);

            transform.DOScale(Vector3.one * scale, scaleDuration)
                .SetEase(scaleCurve);
        }
    }
}