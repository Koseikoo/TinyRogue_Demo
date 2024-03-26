using System;
using Models;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Views
{
    public class MovementView : MonoBehaviour
    {
        [SerializeField] private Transform visual;
        [SerializeField] private float animationDuration = .2f;
        [SerializeField] private float scaleAnimationDuration = .3f;
        [SerializeField] private float maxJumpHeight = 1f;
        [SerializeField] private float maxYScale = .5f;
        [SerializeField] private AnimationCurve movementCurve;
        [SerializeField] private AnimationCurve jumpCurve;
        [SerializeField] private AnimationCurve scaleCurve;
        [SerializeField] private ParticleSystem jumpImpactFX;

        private bool skippedLeft;
        private Sequence _sequence;

        private void Update()
        {
            if (visual == null)
                _sequence?.Kill();
        }

        public void ToTile(Tile tile)
        {
            Vector3 startPosition = transform.position;
            Vector3 endPosition = tile.WorldPosition;
            Vector3 lookDirection = endPosition - startPosition;
            lookDirection.Normalize();
            
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            _sequence
                .Insert(0f, DOTween.To(() => 0f, t =>
                {
                    float adjustedT = movementCurve.Evaluate(t);
                    Vector3 position = Vector3.Lerp(startPosition, endPosition, adjustedT);
                    float yOffset = GetYOffset(startPosition, tile, adjustedT);
                    transform.position = position;
                    visual.localPosition = new Vector3(0f, yOffset, 0f);
                    visual.localScale = GetScale(adjustedT);
                }, 1f, animationDuration)
                    .OnComplete(() => jumpImpactFX.Play()))
                .Insert(0f, DOTween.To(() => 0f, t =>
                {
                    visual.localScale = GetScale(movementCurve.Evaluate(t));
                }, 1f, scaleAnimationDuration))
                .Insert(0f, transform.DOLookAt(tile.WorldPosition, animationDuration));

        }

        public void SkipTurn()
        {
            Vector3 startPosition = transform.position;
            
            _sequence?.Kill();
            _sequence = DOTween.Sequence();

            float rotation = skippedLeft ? 360 : -360;
            rotation += transform.eulerAngles.y;

            _sequence
                .Insert(0f, transform.DORotate(Vector3.up * rotation, animationDuration, RotateMode.FastBeyond360))
                .Insert(0f, transform.DOMoveY(startPosition.y + maxJumpHeight, animationDuration)
                    .SetEase(jumpCurve))
                .Insert(0f, DOTween.To(() => 0f, t =>
                {
                    visual.localScale = GetScale(movementCurve.Evaluate(t));
                }, 1f, scaleAnimationDuration));

            skippedLeft = !skippedLeft;
        }

        private Vector3 GetScale(float t)
        {
            float adjustedT = scaleCurve.Evaluate(t);
            float y = 1 + (adjustedT * maxYScale);
            float xz = 1 / y;
            return new Vector3(xz, y, xz);
        }

        private float GetYOffset(Vector3 startPosition, Tile targetTile, float t)
        {
            float startY = startPosition.y;
            float endY = targetTile.WorldPosition.y + GetBoardOffset(targetTile);
            float yJump = Mathf.Lerp(0f, maxJumpHeight, jumpCurve.Evaluate(t));
            yJump += Mathf.Lerp(startY, endY, t);
            return yJump;
        }
        
        private float GetBoardOffset(Tile tile)
        {
            if (tile.BoardType == BoardType.Chiseled || tile.BoardType == BoardType.Stone)
                return Tile.BoardOffset;
            return 0;
        }
    }
}