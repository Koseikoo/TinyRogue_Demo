using System;
using DG.Tweening;
using Game;
using Models;
using UniRx;
using UnityEngine;
using Zenject;

namespace Views
{
    public class WorldShipView : MonoBehaviour
    {
        [SerializeField] private Transform ship;
        [SerializeField] private ParticleSystem enterExitFX;
        [SerializeField] private float maxScale;
        [SerializeField] private float scaleDuration;
        [SerializeField] private AnimationCurve scaleCurve;

        [Inject] private PlayerManager _playerManager;

        private Sequence _currentSequence;

        private void Update()
        {
            if(ship == null)
                _currentSequence?.Kill();
        }

        public void Initialize(Segment startSegment)
        {
            var averageDirection = startSegment.CenterTile.GetAverageDirection();
            transform.position = startSegment.CenterTile.WorldPosition;
            transform.right = averageDirection;
            
            startSegment.CenterTile.Island.IsDestroyed.Where(b => b).Subscribe(_ => Destroy(gameObject)).AddTo(this);
            startSegment.CenterTile.Island.IslandShipPosition = ship.position;

            _playerManager.Player.EnterIsland.Subscribe(_ => EnterIslandAnimation()).AddTo(this);
            _playerManager.Player.ExitIsland.Subscribe(_ => ExitIslandAnimation()).AddTo(this);
        }

        private void EnterIslandAnimation()
        {
            _currentSequence?.Kill();
            _currentSequence = DOTween.Sequence();

            _currentSequence.Insert(PlayerView.EnterDelay, DOTween.To(() => 0f, t =>
            {
                if(ship == null)
                    return;
                ship.localScale = Vector3.Lerp(Vector3.one, Vector3.one * maxScale, scaleCurve.Evaluate(t));
            }, 1f, scaleDuration));
        }

        private void ExitIslandAnimation()
        {
            _currentSequence?.Kill();
            _currentSequence = DOTween.Sequence();
            
            _currentSequence.Insert(PlayerView.EnterDuration, DOTween.To(() => 0f, t =>
            {
                if(ship == null)
                    return;
                ship.localScale = Vector3.Lerp(Vector3.one, Vector3.one * maxScale, scaleCurve.Evaluate(t));
            }, 1f, scaleDuration));
        }
    }
}