using System;
using DG.Tweening;
using Models;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using UniRx;
using UnityEngine.UI;

namespace Views
{
    public class XpUIView : MonoBehaviour
    {
        [SerializeField]
        private ProceduralImage fillBar;
        [SerializeField]
        private float showDuration;
        [SerializeField]
        private float fadeDuration;

        [SerializeField]
        private Image[] fadeElements;

        private Player _player;
        private Sequence _currentSequence;
        public void Initialize(Player player)
        {
            _player = player;
            player.Xp
                .Subscribe(UpdateXpBar)
                .AddTo(this);

            player.Level
                .Subscribe(OnLevelUp)
                .AddTo(this);
        }

        private void UpdateXpBar(int newXp)
        {
            int level = WeaponHelper.GetLevel(_player.Xp.Value);
            int levelUpXp = WeaponHelper.GetLevelXp(level);
            float fillAmount = (float)newXp / levelUpXp;
            fillBar.fillAmount = fillAmount;
            ShowXpBar();
        }

        private void FadeOut(float delay = 0)
        {
            _currentSequence?.Kill();
            _currentSequence = DOTween.Sequence();
            _currentSequence.Insert(delay, DOTween.To(() => 0, t =>
            {
                foreach (Image image in fadeElements)
                {
                    Color color = image.color;
                    color.a = Mathf.Lerp(1, 0, t);
                    image.color = color;
                }
                
            }, 1f, fadeDuration));
        }

        private void ShowXpBar()
        {
            _currentSequence?.Kill();
            foreach (Image image in fadeElements)
            {
                Color color = image.color;
                color.a = 1;
                image.color = color;
            }
            
            FadeOut(showDuration);
        }

        public void OnLevelUp(int level)
        {
            Debug.Log($"Level Up to {level}");
        }
    }
}