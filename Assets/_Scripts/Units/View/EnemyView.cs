using System;
using Models;
using UniRx;
using UnityEngine;
using DG.Tweening;
using AnimationState = Models.AnimationState;

namespace Views
{
    [RequireComponent(typeof(MovementView))]
    public class EnemyView : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Transform visual;
        [SerializeField] private Transform body;
        [SerializeField] private float hitImpact;
        [SerializeField] private float impactDuration;
        [SerializeField] private AnimationCurve hitCurve;
        [SerializeField] private ParticleSystem aimFX;
        
        private Enemy _enemy;
        private MovementView _move;
        private EnragedView _enrage;

        public void Initialize(Enemy enemy)
        {
            _enemy = enemy;
            _move = GetComponent<MovementView>();
            if(TryGetComponent(out _enrage))
            {
                _enrage.Initialize(enemy);
            }


            _enemy.AimAtTarget
                .SkipLatestValueOnSubscribe()
                .Subscribe(b =>
                {
                    if (b)
                    {
                        aimFX.Play();
                    }
                    else
                    {
                        aimFX.Stop();
                    }
                })
                .AddTo(this);

            _enemy.Tile.SkipLatestValueOnSubscribe()
                .Where(tile => tile != null)
                .Subscribe(tile =>
                {
                    _move.ToTile(tile);
                    
                    if(_enemy.AimAtTarget.Value)
                    {
                        AimEvent();
                    }
                }).AddTo(this);

            _enemy.AttackDirection
                .SkipLatestValueOnSubscribe()
                .Where(_ => !_enemy.IsDead.Value)
                .Subscribe(DamageEvent)
                .AddTo(this);

            _enemy.Health
                .Pairwise()
                .Where(pair => pair.Previous > pair.Current && pair.Current > 0 && _enemy.CurrentTurnDelay.Value != 0)
                .Subscribe(_ => SetAnimationTrigger(AnimationState.GetDamaged))
                .AddTo(this);
            _enemy.Tile.SkipLatestValueOnSubscribe().Subscribe(_ => SetAnimationTrigger(AnimationState.MoveToTile)).AddTo(this);
            _enemy.AnimationCommand.Subscribe(SetAnimationTrigger).AddTo(this);

            _enemy.AimAtTarget
                .SkipLatestValueOnSubscribe()
                .Where(_ => _enemy.Tile.Value != null)
                .Subscribe(b =>
                {
                    if(b)
                    {
                        AimEvent();
                    }
                    else
                    {
                        IdleEvent();
                    }
                })
                .AddTo(this);

            _enemy.IsDead
                .Where(b => b)
                .Subscribe(_ => aimFX.gameObject.SetActive(false));
            
            transform.position = _enemy.Tile.Value.FlatPosition;
        }

        private void DamageEvent(Vector3 attackDirection)
        {
            transform.DOLookAt(transform.position - attackDirection, .2f);
            Debug.Log("damage event");
            Vector3 knockbackPosition = visual.position + (attackDirection * hitImpact);
            DOTween.To(() => 0f, t =>
            {
                body.position = Vector3.Lerp(visual.position, knockbackPosition, hitCurve.Evaluate(t));
            }, 1f, impactDuration);
            
            SetAnimationTrigger(AnimationState.GetDamaged);
        }

        private void IdleEvent()
        {
            SetAnimationTrigger(AnimationState.Idle);
        }

        private void AimEvent()
        {
            Vector3 attackDirection = GameStateContainer.Player.Tile.Value.FlatPosition -
                                  _enemy.Tile.Value.FlatPosition;
            attackDirection.Normalize();
            
            transform.DOLookAt(transform.position + attackDirection, .2f);
            SetAnimationTrigger(AnimationState.Aim);
        }

        private void SetAnimationTrigger(AnimationState state)
        {
            if(animator == null)
            {
                return;
            }

            animator.SetTrigger(state.ToString());
            LookAtPlayer(state);
        }

        private void LookAtPlayer(AnimationState state)
        {
            if(state.ToString().ToLower().Contains("attack"))
            {
                transform.DOLookAt(_enemy.AttackTarget.Tile.Value.FlatPosition, .2f);
            }
        }
    }
}