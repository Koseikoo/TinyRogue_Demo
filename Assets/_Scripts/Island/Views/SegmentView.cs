using System;
using System.Linq;
using DG.Tweening;
using Factories;
using Models;
using UniRx;
using UnityEngine;
using Zenject;

using Unit = Models.Unit;

namespace Views
{
    public class SegmentView : MonoBehaviour
    {
        public SegmentType Type;
        public int Size;
        public Transform PointParent;
        
        [Inject] private CameraModel _cameraModel;
        
        public float Radius => Size * Island.TileDistance;
        
        public SegmentUnitDefinition[] SegmentUnitDefinitions;
        
        [SerializeField] private Transform segmentCompletedParent;
        [SerializeField] private float destroyDelay;

        [Inject] private TextFactory _textFactory;

        private Segment _segment;
        public void Initialize(Segment segment)
        {
            _segment = segment;
            transform.position = _segment.CenterTile.WorldPosition;

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
            Sequence sequence = DOTween.Sequence();
            float delay = 0;

            sequence.AppendInterval(.2f);

            foreach (Unit unit in _segment.Units.Where(unit => unit.Health.Value > 0))
            {
                sequence.InsertCallback(delay, () =>
                {
                    unit.IncreaseComboWithDeath = false;
                    unit.Damage(unit.Health.Value, GameStateContainer.Player);
            
                    _cameraModel.RotationShakeCommand.Execute();
                });

                delay += destroyDelay;
            }

            sequence.OnComplete(() =>
            {
                _segment.SegmentCompleteAction(transform);
            });
        }
    }
}