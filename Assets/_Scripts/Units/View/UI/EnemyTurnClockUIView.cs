using DG.Tweening;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace Views
{
    public class EnemyTurnClockUIView : MonoBehaviour
    {
        private Enemy _enemy;

        [SerializeField] private GameObject turnClock;
        [SerializeField] private Transform clockHandle;
        [SerializeField] private Image clockProgress;
        [SerializeField] private Transform[] clockPoints;

        [SerializeField] private AnimationCurve clockProgressCurve;
        [SerializeField] private AnimationCurve clockProgressEndCurve;
        [SerializeField] private float clockProgressDuration;
        
        [SerializeField] private AnimationCurve clockFadeInCurve;
        [SerializeField] private AnimationCurve clockFadeOutCurve;
        [SerializeField] private float clockFadeDuration;

        private float _radius;
        private bool _isActive;

        private Tween _clockProgressTween;
        private Tween _clockFadeInTween;
        private Tween _clockFadeOutTween;

        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;
            _radius = GetComponent<RectTransform>().sizeDelta.x * .5f;
            
            AssignClockPoints();

            _enemy.IsDead.Where(b => b).Subscribe(_ =>
            {
                _clockProgressTween?.Kill();
                _clockFadeInTween?.Kill();
                _clockFadeOutTween?.Kill();
            }).AddTo(this);

            _enemy.CurrentTurnDelay
                .Where(_ => _enemy.TurnDelay > 0 && !_enemy.IsDead.Value)
                .Pairwise()
                .Subscribe(pair => UpdateTurnClock(pair.Previous, pair.Current))
                .AddTo(this);
        }
        
        private void UpdateTurnClock(int lastDelay, int currentDelay)
        {
            HandleClockFade();
            AssignClockPoints();
            RenderClockState(lastDelay, currentDelay);
        }

        private void AssignClockPoints()
        {
            var anchoredVector = Vector2.up;
            float rotationStep = 360f / _enemy.TurnDelay;

            for (int i = 0; i < clockPoints.Length; i++)
                clockPoints[i].gameObject.SetActive(i < _enemy.TurnDelay);
            
            for (int i = 0; i < _enemy.TurnDelay; i++)
            {
                clockPoints[i].GetComponent<RectTransform>().anchoredPosition = anchoredVector.RotateVector(rotationStep * i) * _radius;
            }
        }

        private void HandleClockFade()
        {
            if (_enemy.SkipTurn && !_isActive)
            {
                // Fade In
                _clockFadeInTween = turnClock.transform.DOScale(Vector3.one, clockFadeDuration).SetEase(clockFadeInCurve);
                _isActive = true;
            }
            else if(!_enemy.SkipTurn && _isActive)
            {
                // FadeOut
                _clockFadeOutTween = turnClock.transform.DOScale(Vector3.zero, clockFadeDuration).SetEase(clockFadeOutCurve);
                _isActive = false;
            }
        }

        private void RenderClockState(int lastDelay, int currentDelay)
        {
            lastDelay = Mathf.Min(lastDelay, _enemy.TurnDelay);
            
            float state = (float)currentDelay / _enemy.TurnDelay;
            float lastState = (float)lastDelay / _enemy.TurnDelay;

            if (currentDelay == _enemy.TurnDelay)
            {
                clockProgress.fillAmount = 0f;
                clockHandle.localRotation = default;
                return;
            }

            AnimationCurve curve = currentDelay == 0 ? clockProgressEndCurve : clockProgressCurve;

            _clockProgressTween = DOTween.To(() => 0f, t =>
            {
                var progress = Mathf.LerpUnclamped(lastState, state, curve.Evaluate(t));
                
                clockProgress.fillAmount = 1-progress;
                clockHandle.localRotation = Quaternion.Euler(new Vector3(0f, 0f, progress * 360));
            }, 1f, clockProgressDuration);
        }
    }
}