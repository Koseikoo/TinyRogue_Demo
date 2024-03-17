using System;
using DG.Tweening;
using Factories;
using Models;
using UniRx;
using UnityEngine;
using Zenject;

namespace Views
{
    public class SegmentView : MonoBehaviour
    {
        public SegmentType Type;
        public int Size;
        public Transform PointParent;
        
        public float Radius => Size * Island.TileDistance;
        
        public SegmentUnitDefinition[] SegmentUnitDefinitions;
        
        [SerializeField] private bool DestroySegmentOnCompletion;
        [SerializeField] private Transform segmentParent;
        [SerializeField] private Transform segmentCompletedParent;

        [SerializeField] private float segmentScaleDuration;
        [SerializeField] private AnimationCurve segmentScaleCurve;

        [Inject] private TextFactory _textFactory;

        private Segment _segment;
        public void Initialize(Segment segment)
        {
            _segment = segment;
            transform.position = _segment.Tile.WorldPosition;

            segment.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(this);

            segment.Tiles[0].Island.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(this);
            
            segment.IsCompleted
                .Where(b => b)
                .Subscribe(_ => CompleteSegmentAnimation())
                .AddTo(this);
        }

        private void OnDrawGizmos()
        {
            if(Application.isEditor)
                Gizmos.DrawWireSphere(transform.position + (Vector3.up * 2), Size * Island.TileDistance);
        }

        private void CompleteSegmentAnimation()
        {
            _segment.SegmentCompleteAction(segmentCompletedParent);
            return;
            
            segmentParent.localScale = Vector3.one;
            segmentCompletedParent.localScale = Vector3.zero;

            foreach (var tile in _segment.Tiles)
            {
                tile.Selections.Clear();
            }
            
            Sequence sequence = DOTween.Sequence();

            sequence.Append(segmentParent.DOScale(Vector3.zero, segmentScaleDuration)
                .SetEase(segmentScaleCurve)
                .OnComplete(() =>
                {
                    _segment.SegmentCompleteAction(segmentCompletedParent);
                    //_textFactory.Create(transform.position + (Vector3.up * 2), "Complete!");
                }));
            sequence.Append(segmentCompletedParent.DOScale(Vector3.one, segmentScaleDuration))
                .SetEase(segmentScaleCurve);
        }
    }
}