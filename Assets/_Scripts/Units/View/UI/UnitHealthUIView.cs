using System;
using System.Collections.Generic;
using Models;
using TMPro;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Unit = Models.Unit;

namespace Views
{
    public class UnitHealthUIView : MonoBehaviour
    {
        [SerializeField] private GameObject visual;

        [SerializeField] private HealthRenderer renderer;

        private Unit _unit;

        private Dictionary<StatusEffect, IDisposable> _effectSubscriptions = new();
        
        public void Initialize(Unit unit)
        {
            _unit = unit;
            unit.Health.Subscribe(UpdateHealthBar).AddTo(this);

            unit.ActiveStatusEffects.ObserveAdd().Subscribe(next => AddEffectSubscription(next.Value)).AddTo(this);
            unit.ActiveStatusEffects.ObserveRemove().Pairwise().Subscribe(next => RemoveEffectSubscription(next.Previous.Value)).AddTo(this);

            if(unit is not Player)
                unit.IsDamaged.Subscribe(ShowHealthBar).AddTo(this);
        }

        private void UpdateHealthBar(int newHealth)
        {
            renderer.Render(_unit);
        }

        private void ShowHealthBar(bool isDamaged)
        {
            if(visual == null)
                return;
            visual.SetActive(isDamaged);
        }

        private void AddEffectSubscription(StatusEffect effect)
        {
            if (effect is PoisonEffect poison)
            {
                _effectSubscriptions[poison] = poison.Duration.Subscribe(_ => UpdateHealthBar(_unit.Health.Value)).AddTo(this);
            }
        }

        private void RemoveEffectSubscription(StatusEffect effect)
        {
            if (effect is PoisonEffect poison)
            {
                _effectSubscriptions[poison].Dispose();
                _effectSubscriptions.Remove(effect);
            }
        }
    }
}