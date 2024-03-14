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
                .Subscribe(tile => HolsterAttackAnimation(tile.WorldPosition))
                .AddTo(this);
            
            weapon.Tile
                .SkipLatestValueOnSubscribe()
                .Where(tile => tile == _weapon.Owner.Tile.Value && !_weapon.FixToHolster)
                .Subscribe(tile => HolsterReturnAnimation())
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
                swordGrip.transform.SetPositionAndRotation(_weapon.Owner.HolsterTransform.position, _weapon.Owner.HolsterTransform.rotation);
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
            s.Insert(0f, swordGrip.DOMove(position + Vector3.up - (dummy.up * .5f), Weapon.AttackAnimationDuration).SetEase(Ease.InCubic));
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

                    swordGrip.position = Vector3.Lerp(startPosition, _weapon.Owner.HolsterTransform.position, tEval);
                    swordGrip.rotation = Quaternion.Lerp(startRotation, _weapon.Owner.HolsterTransform.rotation, tEval);
                    //swordGrip.localScale = Vector3.Lerp(startScale, new Vector3(.63f, .72f, .63f), tEval);

                }, 1f, Weapon.AttackAnimationDuration * 1.5f)
                .OnComplete(() =>
                {
                    trailRenderer.enabled = false;
                    _weapon.FixToHolster = true;
                });
        }
    }
}