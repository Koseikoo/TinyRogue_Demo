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
using Unit = Models.Unit;

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

        private Tween _knockBackTween;
        
        private Unit _unit;
        private bool _pulseActive;
        
        public void Initialize(Unit unit)
        {
            _unit = unit;
            InitializeAttachedUnitViews(unit);

            Vector3 offset = Vector3.zero;
            
            if(unit.Tile.Value.GrassType != GrassType.None)
            {
                offset += Vector3.up * Tile.GrassOffset;
            }

            if (unit.Tile.Value.BoardType != BoardType.None &&
                unit.Tile.Value.BoardType != BoardType.Metal)
            {
                offset += Vector3.up * Tile.BoardOffset;
            }
            
            transform.position = unit.Tile.Value.WorldPosition + offset;

            unit.OnKnockback
                .Subscribe(OnKnockback)
                .AddTo(this);

            unit.IsInvincible
                .Subscribe(isInvincible => invincibilityVisual.SetActive(isInvincible))
                .AddTo(this);

            unit.IsDead
                .Where(b => b)
                .Subscribe(_ => DeathEvent())
                .AddTo(this);

            unit.IsDestroyed
                .Where(destroy => destroy)
                .Subscribe(_ =>
                {
                    Destroy(gameObject);
                })
                .AddTo(this);
            
            
        }

        private void Update()
        {
            if(GameStateContainer.Player == null || _unit == null || visual == null)
            {
                return;
            }

            if (GameStateContainer.Player.SelectedTiles.Contains(_unit.Tile.Value) && !_unit.IsInvincible.Value)
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

        private void InitializeAttachedUnitViews(Unit unit)
        {
            Component[] components = gameObject.GetComponents<Component>();

            foreach (IUnitViewInitialize i in components.OfType<IUnitViewInitialize>())
                i.Initialize(unit);

        }

        private void DeathEvent()
        {
            Sequence sequence = DOTween.Sequence();
            _unit.Death();
            deathFX.Play();
            hitFX.Play();
            
            Destroy(visual.gameObject);
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