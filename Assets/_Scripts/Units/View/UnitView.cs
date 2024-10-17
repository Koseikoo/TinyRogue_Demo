using System;
using System.Linq;
using DG.Tweening;
using Factories;
using Models;
using UnityEngine;
using UniRx;
using Unity.VisualScripting;
using AnimationState = Models.AnimationState;
using Sequence = DG.Tweening.Sequence;

namespace Views
{
    
    public class UnitView : MonoBehaviour
    {
        [SerializeField] private float deathDuration = .3f;
        [SerializeField] private Transform visual;
        [SerializeField] private GameObject invincibilityVisual;
        [SerializeField] private Animator animator;
        [SerializeField] private float selectedPulseSpeed;
        [SerializeField] private float selectedPulseIntensity;
        [SerializeField] private ParticleSystem deathFX;
        [SerializeField] private ParticleSystem hitFX;

        [Header("Knockback")]
        [SerializeField] private float knockbackIntensity;
        [SerializeField] private float knockbackDuration;
        [SerializeField] private AnimationCurve knockbackCurve;
        
        [Header("KnockOff")]
        [SerializeField] private float knockOffDistance;
        [SerializeField] private float fallDistance;

        private Tween _knockBackTween;
        
        private GameUnit _gameUnit;
        private bool _pulseActive;
        
        public void Initialize(GameUnit gameUnit)
        {
            _gameUnit = gameUnit;
            InitializeAttachedUnitViews(gameUnit);

            Vector3 offset = Vector3.zero;
            
            if(gameUnit.Tile.Value.GrassType != GrassType.None)
            {
                offset += Vector3.up * Tile.GrassOffset;
            }

            if (gameUnit.Tile.Value.BoardType != BoardType.None &&
                gameUnit.Tile.Value.BoardType != BoardType.Metal)
            {
                offset += Vector3.up * Tile.BoardOffset;
            }
            
            transform.position = gameUnit.Tile.Value.WorldPosition + offset;

            gameUnit.OnKnockback
                .Subscribe(OnKnockback)
                .AddTo(this);
            
            gameUnit.OnKnockDown
                .Subscribe(OnKnockDown)
                .AddTo(this);

            gameUnit.IsInvincible
                .Subscribe(isInvincible => invincibilityVisual.SetActive(isInvincible))
                .AddTo(this);

            gameUnit.IsDead
                .Where(b => b)
                .Subscribe(_ => DeathEvent())
                .AddTo(this);

            gameUnit.IsDestroyed
                .Where(destroy => destroy)
                .Subscribe(_ =>
                {
                    Destroy(gameObject);
                })
                .AddTo(this);
            
            
        }

        private void Update()
        {
            if(GameStateContainer.Player == null || _gameUnit == null || visual == null)
            {
                return;
            }

            if (GameStateContainer.Player.SelectedTiles.Contains(_gameUnit.Tile.Value) && !_gameUnit.IsInvincible.Value)
            {
                float sin = (Mathf.Sin(Time.time * selectedPulseSpeed) + 1) * 0.5f;
                float max = 1 + selectedPulseIntensity;
                float min = 1 - selectedPulseIntensity;
                visual.localScale = Mathf.Lerp(min, max, sin) * Vector3.one;
            }
            else
            {
                visual.localScale = Vector3.one;
            }
        }

        private void InitializeAttachedUnitViews(GameUnit gameUnit)
        {
            Component[] components = gameObject.GetComponents<Component>();

            foreach (IUnitViewInitialize i in components.OfType<IUnitViewInitialize>())
                i.Initialize(gameUnit);

        }

        private void DeathEvent()
        {
            Sequence sequence = DOTween.Sequence();
            float delay = 0;

            if (_knockBackTween != null)
            {
                delay = knockbackDuration;
            }

            sequence.InsertCallback(delay, () =>
            {
                _gameUnit.Death();
                deathFX.Play();
                hitFX.Play();
                Destroy(visual.gameObject);
            });
            
            sequence.AppendInterval(1f);
            sequence.OnComplete(() =>
            {
                Destroy(gameObject);
            });
        }

        private void OnKnockback(Vector3 direction)
        {
            Vector3 visualStartPosition = visual.position;
            Vector3 maxPosition = visualStartPosition + (knockbackIntensity * direction);

            if (_knockBackTween != null)
            {
                _knockBackTween.Kill();
            }

            _knockBackTween = DOTween.To(() => 0f, t =>
            {
                visual.position = Vector3.Lerp(visualStartPosition, maxPosition, knockbackCurve.Evaluate(t));
            }, 1, knockbackDuration);
        }

        private void OnKnockDown(Vector3 direction)
        {
            Vector3 visualStartPosition = visual.position;
            Vector3 midPosition = visualStartPosition + (direction * knockOffDistance);
            Vector3 endPosition = midPosition + (fallDistance * Vector3.down);

            if (_knockBackTween != null)
            {
                _knockBackTween.Kill();
            }

            _knockBackTween = DOTween.To(() => 0f, t =>
            {
                Vector3 lerpPoint = MathHelper.BezierLerp(visualStartPosition, midPosition, endPosition, t);
                transform.position = lerpPoint;
            }, 1, knockbackDuration);
        }

        private void SetAnimationTrigger(AnimationState state)
        {
            if(animator == null)
            {
                return;
            }
            animator.SetTrigger(state.ToString());
        }
    }
}