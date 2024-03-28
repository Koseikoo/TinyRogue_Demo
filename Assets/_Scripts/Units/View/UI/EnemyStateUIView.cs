using Container;
using Models;
using UnityEngine;
using UniRx;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;
using Unity.VisualScripting;
using Sequence = DG.Tweening.Sequence;

namespace Views
{
    public class EnemyStateUIView : MonoBehaviour
    {
        [SerializeField] private Image stateImage;
        [SerializeField] private RectTransform imageRect;
        [SerializeField] private RectTransform centerRect;
        [SerializeField] private float showDuration;
        [SerializeField] private float fadeDuration;
        
        [Inject] private EnemyStateIconContainer _enemyStateIconContainer;

        private bool _visible;
        private Enemy _enemy;

        private Sequence _sequence;

        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;
            _enemy.State
                .SkipLatestValueOnSubscribe()
                .Subscribe(ShowStatePopup)
                .AddTo(this);
        }

        private void Update()
        {
            if(_enemy.IsDead.Value)
                _sequence?.Kill();
            
            if(!_visible)
                return;
            
            if(!centerRect.IsInsideScreen())
                imageRect.MoveIntoScreen(_enemy.Tile.Value.FlatPosition);
            else
                imageRect.localPosition = default;
        }

        private void ShowStatePopup(EnemyState state)
        {
            if(_enemy.IsDead.Value)
                return;
            ShowStateSprite(state);
            FadeOutSprite(showDuration);
        }

        private void ShowStateSprite(EnemyState state)
        {
            Sprite sprite = _enemyStateIconContainer.GetStateSprite(state);

            stateImage.sprite = sprite;
            stateImage.transform.localScale = Vector3.one;
            stateImage.color = Color.white;
            _visible = true;
        }

        private void FadeOutSprite(float delay)
        {
            _sequence = DOTween.Sequence();
            _sequence
                .AppendInterval(delay)
                .Insert(delay, stateImage.DOFade(0, fadeDuration))
                .Insert(delay, stateImage.transform.DOScale(Vector3.zero, fadeDuration));
            _sequence.OnComplete(() => _visible = false);
        }
    }
}