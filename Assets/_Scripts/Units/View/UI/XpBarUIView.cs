using System;
using DG.Tweening;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class XpBarUIView : MonoBehaviour
    {
        [SerializeField] private Image _barFill;
        [SerializeField] private Image _bar;
        [SerializeField] private Image _barBackground;
        
        [SerializeField] private float _fadeDuration;
        private Sequence _sequence;
        
        public void Initialize(Player player)
        {
            player.Weapon.Xp.SkipLatestValueOnSubscribe().Subscribe(xp =>
            {
                UpdateXpBar(player.Weapon.Progress());
                ShowXpBar();
            });

            FadeBar(0);
        }

        private void UpdateXpBar(float fill)
        {
            _barFill.fillAmount = fill;
        }

        private void ShowXpBar()
        {
            KillSequence();
            _sequence = FadeBar(1f);
            _sequence.AppendInterval(1);
            _sequence.OnComplete(HideXpBar);
        }

        private void HideXpBar()
        {
            var sequence = FadeBar(0f);
        }

        private Sequence FadeBar(float fadeTarget)
        {
            var sequence = DOTween.Sequence();
            sequence.Insert(0f, _barFill.DOFade(fadeTarget, _fadeDuration));
            sequence.Insert(0f, _bar.DOFade(fadeTarget, _fadeDuration));
            sequence.Insert(0f, _barBackground.DOFade(fadeTarget, _fadeDuration));
            return sequence;
        }

        private void KillSequence()
        {
            if (_sequence != null)
            {
                _sequence.Kill();
                _sequence = null;
            }
        }
    }
}