using System;
using System.Collections.Generic;
using Models;
using TMPro;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class UnitHealthUIView : MonoBehaviour
    {
        [SerializeField] private GameObject visual;

        [SerializeField] private HealthRenderer renderer;

        private GameUnit _gameUnit;

        private Dictionary<StatusEffect, IDisposable> _effectSubscriptions = new();
        
        public void Initialize(GameUnit gameUnit)
        {
            _gameUnit = gameUnit;
            gameUnit.Health.Subscribe(UpdateHealthBar).AddTo(this);

            gameUnit.ActiveStatusEffects.ObserveAdd().Subscribe(next => AddEffectSubscription(next.Value)).AddTo(this);
            gameUnit.ActiveStatusEffects.ObserveRemove().Pairwise().Subscribe(next => RemoveEffectSubscription(next.Previous.Value)).AddTo(this);

            if(gameUnit is not Player)
            {
                gameUnit.IsDamaged.Subscribe(ShowHealthBar).AddTo(this);
            }
        }

        private void UpdateHealthBar(int newHealth)
        {
            renderer.Render(_gameUnit);
        }

        private void ShowHealthBar(bool isDamaged)
        {
            if(visual == null)
            {
                return;
            }
            visual.SetActive(isDamaged);
        }

        private void AddEffectSubscription(StatusEffect effect)
        {
            if (effect is PoisonEffect poison)
            {
                _effectSubscriptions[poison] = poison.Duration.Subscribe(_ => UpdateHealthBar(_gameUnit.Health.Value)).AddTo(this);
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