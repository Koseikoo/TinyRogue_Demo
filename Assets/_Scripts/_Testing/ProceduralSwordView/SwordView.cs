using System;
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
        
        [SerializeField] private Transform dummy;
        [SerializeField] private Transform swordGrip;
        [SerializeField] private Transform swordAnchor;
        [SerializeField] private float sideMoveAmount;
        [SerializeField] private Quaternion startRotation;
        [SerializeField] private TrailRenderer trailRenderer;
        [SerializeField] private AnimationCurve attackCurve;
        [SerializeField] private AnimationCurve returnCurve;
        
        [Header("new")]
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

            weapon.Tile
                .SkipLatestValueOnSubscribe()
                .Where(tile => tile != _weapon.Owner.Tile.Value)
                .Subscribe(tile => AttackAnimation(tile.WorldPosition))
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
            if (_weapon.FixToHolster)
            {
                swordAnchor.position = _weapon.Owner.HolsterTransform.position;
                swordGrip.rotation = _weapon.Owner.HolsterTransform.rotation;
                swordGrip.transform.localScale = new Vector3(.63f, .72f, .63f);
            }
        }

        private void HolsterAttackAnimation(Vector3 position)
        {
            _weapon.FixToHolster = false;
            trailRenderer.enabled = true;

            var forward = (position - _lastPosition).normalized;
            dummy.forward = forward;
            dummy.rotation = Quaternion.AngleAxis(holsterAttackAngle, forward) * dummy.rotation;
            
            Sequence s = DOTween.Sequence();
            s.Insert(0f, swordAnchor.DOMove(position + Vector3.up - (dummy.up * .5f), Weapon.AttackAnimationDuration).SetEase(Ease.InCubic));
            s.Insert(0f, swordGrip.DORotateQuaternion(dummy.rotation, Weapon.AttackAnimationDuration));
            s.AppendInterval(trailRenderer.time)
                .OnComplete(() => trailRenderer.enabled = false);
            _lastPosition = position;
        }
        
        private void HolsterReturnAnimation()
        {
            trailRenderer.enabled = true;
            Vector3 startPosition = swordGrip.position;
            Quaternion startRotation = swordGrip.rotation;
            Vector3 startScale = swordGrip.localScale;

            DOTween.To(() => 0f, t =>
                {
                    float tEval = returnCurve.Evaluate(t);

                    swordAnchor.position = Vector3.Lerp(startPosition, _weapon.Owner.HolsterTransform.position, tEval);
                    swordGrip.rotation = Quaternion.Lerp(startRotation, _weapon.Owner.HolsterTransform.rotation, tEval);
                    //swordGrip.localScale = Vector3.Lerp(startScale, new Vector3(.63f, .72f, .63f), tEval);

                }, 1f, Weapon.AttackAnimationDuration * 1.5f)
                .OnComplete(() =>
                {
                    trailRenderer.enabled = false;
                    _weapon.FixToHolster = true;
                });
        }

        private void AttackAnimation(Vector3 position)
        {
            _weapon.FixToHolster = false;
            
            Debug.Log("attack");
            
            var attackDirection = _weapon.AttackDirection.Value;
            Vector3 start = swordGrip.up;
            Vector3 end = attackDirection.normalized;
            float distance = (position - _weapon.Owner.Tile.Value.WorldPosition).magnitude;
            float relativeDuration = Weapon.AttackAnimationDuration / (_weapon.Range * Island.TileDistance);
            //_lastPosition = position + Vector3.up;

            float moveDuration = distance * relativeDuration;

            Sequence sequence = DOTween.Sequence();

            sequence.Insert(0f, swordAnchor.DOMove(position + Vector3.up, moveDuration)
                .SetEase(Ease.Linear));
            sequence.Insert(0f, DOTween.To(() => 0f, t =>
            {
                swordGrip.up = Vector3.Lerp(start, end, t).normalized;
            }, 1f, attackRotateDuration));

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
            });
            
        }

        private void IdleAnimation()
        {
            
        }

        private void AimAnimation()
        {
            
        }
    }
}