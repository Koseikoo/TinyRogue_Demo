using System;
using System.Collections.Generic;
using System.Linq;
using Testing;
using UniRx;
using UnityEngine;
using DG.Tweening;
using Models;
using Random = UnityEngine.Random;

namespace Views
{
    public class SwordView : MonoBehaviour
    {
        public Transform SwordTransform => swordGrip;
        public float holsterAttackAngle;

        [SerializeField] private float attackDuration;
        [Space]
        [SerializeField] private Transform dummy;
        [SerializeField] private Transform swordGrip;
        [SerializeField] private Transform swordAnchor;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private AnimationCurve returnCurve;
        
        [Header("Attack")]
        [SerializeField] private float attackRotateDuration;
        [SerializeField] private float toIdleDuration;

        [Header("Return")]
        [SerializeField] private ParticleSystem disappearFX;
        [SerializeField] private ParticleSystem appearFX;
        [SerializeField] private float scaleDuration;
        [SerializeField] private float travelDuration;
        
        [Header("Bounce")]
        [SerializeField] private float bounceHeight;
        [SerializeField] private float bounceDuration;
        [SerializeField] private AnimationCurve bounceCurve;
        [SerializeField] private ParticleSystem bounceFX;

        private Quaternion first;
        private Quaternion second;
        private Quaternion third;
        private Quaternion fourth;

        private const float initialRotation = 180;
        private const float swingBackRotation = 130;
        private const float swingRotation = -270;

        private Weapon _weapon;
        private Tile _lastWeaponTile;
        private Vector3 _lastPosition;

        public void Initialize(Weapon weapon)
        {
            _weapon = weapon;

            weapon.AttackPath
                .SkipLatestValueOnSubscribe()
                .Where(t => t.Count >= 2)
                .Subscribe(t => AttackAnimation(t))
                .AddTo(this);
            
            weapon.Tile
                .SkipLatestValueOnSubscribe()
                .Where(tile => tile == _weapon.Owner.Tile.Value && !_weapon.FixToHolster)
                .Subscribe(tile =>
                {
                    if (_weapon.BounceBack)
                        BounceBackAnimation();
                    else
                        ReturnAnimation();
                })
                .AddTo(this);

            weapon.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(this);
        }

        private void Update()
        {
            if (_weapon.Owner != null && _weapon.Owner.HolsterTransform != null)
                transform.localScale = _weapon.Owner.HolsterTransform.localScale;
            
            if (_weapon.FixToHolster)
            {
                swordAnchor.position = _weapon.Owner.HolsterTransform.position;
                swordGrip.rotation = _weapon.Owner.HolsterTransform.rotation;
                swordGrip.transform.localScale = new Vector3(.63f, .72f, .63f);
            }
        }

        private void AttackAnimation(List<Tile> attackPath)
        {
            _weapon.InAttack = true;
            _weapon.FixToHolster = false;
            
            var p = attackPath.Select(t => t.FlatPosition).ToArray();
            var segmentLengths = p.GetCatmullSegmentLengthCumulative();
            var duration = segmentLengths.Sum();
            swordGrip.up = _weapon.AttackDirection.Value.normalized;

            Vector3 lastPosition = p[0] + Vector3.up;
            Sequence sequence = DOTween.Sequence();

            for (int i = 0; i < attackPath.Count; i++)
            {
                int index = i;
                float lastStop = index == 0 ? 0 : segmentLengths[index - 1] / duration;
                float stop = index == segmentLengths.Length ? 1 : segmentLengths[index] / duration;

                sequence.Append(DOTween.To(() => 0, t =>
                    {
                        float segmentT = Mathf.Lerp(lastStop, stop, t);
                        Vector3 position = p.CatmullLerp(segmentT) + Vector3.up;
                        swordAnchor.position = position;
                        swordGrip.up = (position - lastPosition).normalized;
                        lastPosition = position;
                    }, 1f, (stop - lastStop) * attackDuration)
                        .OnComplete(() =>
                        {
                            _weapon.AttackTile(attackPath[index], _weapon.AttackDirection.Value);
                        })
                        .SetEase(Ease.Linear));
            }

            sequence.OnComplete(() =>
            {
                _weapon.InAttack = false;
                _weapon.BounceBack = attackPath[^1].HasAliveUnit;
                if (_weapon.BounceBack)
                    _weapon.ReturnToHolster();
            });

        }

        private void BounceBackAnimation()
        {
            Vector3 startPosition = swordAnchor.position;
            Vector3 endPosition = _weapon.Owner.HolsterTransform.position;
            Vector3 p0 = startPosition + (Vector3.up * bounceHeight);
            Vector3 p1 = endPosition + (Vector3.up * bounceHeight);

            Quaternion endRotation = _weapon.Owner.HolsterTransform.rotation;

            bounceFX.gameObject.transform.position = startPosition;
            bounceFX.Play();

            Sequence sequence = DOTween.Sequence();

            sequence.Insert(0f, DOTween.To(() => 0f, t =>
            {
                swordAnchor.position = MathHelper.CubicBezierLerp(startPosition, p0, p1, endPosition, t);
            }, 1f, bounceDuration));

            sequence.Insert(0f, swordGrip.DORotateQuaternion(endRotation, bounceDuration));
            sequence.SetEase(bounceCurve);
            sequence.OnComplete(() => _weapon.FixToHolster = true);
        }

        private void ReturnAnimation()
        {
            Sequence sequence = DOTween.Sequence();
            trailRenderer.enabled = false;
            
            Vector3 disappearPosition = swordAnchor.position;

            sequence.Insert(0f, swordAnchor.DOScale(Vector3.zero, scaleDuration)
                .OnComplete(() =>
                {
                    disappearFX.gameObject.transform.position = disappearPosition;
                    disappearFX.Play();
                    
                    swordAnchor.position = _weapon.Owner.HolsterTransform.position;
                    swordGrip.rotation = _weapon.Owner.HolsterTransform.rotation;
                }));
            
            sequence.Insert(scaleDuration + travelDuration, swordAnchor.DOScale(Vector3.one, scaleDuration));
            sequence.Insert(scaleDuration + travelDuration, swordGrip.DORotateQuaternion(_weapon.Owner.HolsterTransform.rotation, scaleDuration));
            sequence.InsertCallback(scaleDuration + travelDuration, () =>
            {
                appearFX.gameObject.transform.position = swordAnchor.position;
                appearFX.Play();
                _weapon.FixToHolster = true;
                trailRenderer.enabled = true;
            });
        }
    }
}