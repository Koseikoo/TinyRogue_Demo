using System;
using Models;
using UnityEngine;
using UniRx;
using Unit = Models.Unit;

namespace Views
{
    public class UnitUIView : MonoBehaviour
    {
        private Transform _followTarget;
        
        public void Initialize(Unit unit, Transform unitVisual)
        {
            _followTarget = unitVisual;
            unit.IsDead.Where(b => b && unit is not Player).Subscribe(_ => Destroy(gameObject)).AddTo(this);
            
            unit.IsDestroyed
                .Where(d => d)
                .Subscribe(_ =>
                {
                    Destroy(gameObject);
                })
                .AddTo(this);
        }

        private void LateUpdate()
        {
            if(UIHelper.Camera == null)
                return;
            
            if (_followTarget == null)
            {
                Destroy(gameObject);
                return;
            }
            transform.position = UIHelper.Camera.WorldToScreenPoint(_followTarget.position);
        }
    }
}