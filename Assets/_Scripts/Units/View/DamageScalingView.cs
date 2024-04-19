using System;
using DG.Tweening;
using UniRx;
using UnityEngine;
using Unit = Models.Unit;

namespace Views
{
    public class DamageScalingView : MonoBehaviour, IUnitViewInitialize
    {
        [SerializeField] private float maxScale;
        [SerializeField] private float scaleDuration;
        [SerializeField] private AnimationCurve scaleCurve;
        private Unit _unit;

        public void Initialize(Unit unit)
        {
            _unit = unit;

            _unit.Health.Subscribe(_ => UpdateScale()).AddTo(this);
        }

        private void UpdateScale()
        {
            if (_unit.Health.Value <= 0)
                return;

            float scale = Mathf.Lerp(maxScale, 1f, (float)_unit.Health.Value / _unit.MaxHealth);

            transform.DOScale(Vector3.one * scale, scaleDuration)
                .SetEase(scaleCurve);
        }
    }
}