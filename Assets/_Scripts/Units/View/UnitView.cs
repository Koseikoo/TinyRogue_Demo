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
        [SerializeField] private Renderer meshRenderer;
        [SerializeField] private Animator animator;
        [SerializeField] private float selectedPulseSpeed;
        [SerializeField] private float selectedPulseIntensity;
        [SerializeField] private AnimationCurve deathScaleCurve;
        [SerializeField] private ParticleSystem deathFX;
        private Unit _unit;
        private bool _pulseActive;
        private Color _deathColor;
        
        public void Initialize(Unit unit)
        {
            _deathColor = new Color(.43f, .43f, .43f, 1f);
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
            SetAnimationTrigger(AnimationState.GetDamaged);
            
            Sequence sequence = DOTween.Sequence();
            if (meshRenderer != null)
                sequence.Insert(0f, meshRenderer.material.DOColor(_deathColor, deathDuration));
                
            sequence.Insert(0f, visual.DOScale(Vector3.zero, deathDuration)
                .SetEase(deathScaleCurve)
                .OnComplete(() =>
                {
                    _unit.Death();
                    if (meshRenderer != null)
                        deathFX.transform.position = meshRenderer.transform.position;
                    deathFX.Play();
                }));
            
            sequence.AppendInterval(.5f)
                .OnComplete(() => Destroy(gameObject));
        }

        private void SetAnimationTrigger(AnimationState state)
        {
            if(animator == null)
                return;
            animator.SetTrigger(state.ToString());
        }
    }
}