using System;
using Models;
using UnityEngine;
using UniRx;

namespace Views
{
    public class UnitUIView : MonoBehaviour
    {
        private Transform _followTarget;
        
        public void Initialize(GameUnit gameUnit, Transform unitVisual)
        {
            _followTarget = unitVisual;
            gameUnit.IsDead.Where(b => b && gameUnit is not Player).Subscribe(_ => Destroy(gameObject)).AddTo(this);
            
            gameUnit.IsDestroyed
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
            {
                return;
            }

            if (_followTarget == null)
            {
                Destroy(gameObject);
                return;
            }
            transform.position = UIHelper.Camera.WorldToScreenPoint(_followTarget.position);
        }
    }
}