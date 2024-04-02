using System;
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
        
        private Unit _unit;
        private bool _pulseActive;
        
        public void Initialize(Unit unit)
        {
            _unit = unit;

            Vector3 offset = Vector3.zero;
            
            if(unit.Tile.Value.GrassType != GrassType.None)
                offset += Vector3.up * Tile.GrassOffset;
            
            if (unit.Tile.Value.BoardType != BoardType.None &&
                unit.Tile.Value.BoardType != BoardType.Metal)
            {
                offset += Vector3.up * Tile.BoardOffset;
            }
            
            transform.position = unit.Tile.Value.WorldPosition + offset;

            unit.IsInvincible.Subscribe(isInvincible => invincibilityVisual.SetActive(isInvincible));

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
            if(GameStateContainer.Player == null || _unit == null)
                return;
            
            if (GameStateContainer.Player.SelectedTiles.Contains(_unit.Tile.Value) && !_unit.IsInvincible.Value)
            {
                var sin = (Mathf.Sin(Time.time * selectedPulseSpeed) + 1) * 0.5f;
                float max = 1 + selectedPulseIntensity;
                float min = 1 - selectedPulseIntensity;
                transform.localScale = Mathf.Lerp(min, max, sin) * Vector3.one;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
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

        private void SetAnimationTrigger(AnimationState state)
        {
            if(animator == null)
                return;
            animator.SetTrigger(state.ToString());
        }
    }
}