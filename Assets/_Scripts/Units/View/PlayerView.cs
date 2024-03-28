using System.Collections.Generic;
using UnityEngine;
using Models;
using UniRx;
using DG.Tweening;
using Factory;
using Zenject;
using AnimationState = Models.AnimationState;

namespace Views
{
    [RequireComponent(typeof(MovementView), typeof(BagView))]
    public class PlayerView : MonoBehaviour
    {
        private const int LinePoints = 5;
        
        [SerializeField] private Animator animator;
        [SerializeField] private Transform holsterTransform;
        
        [Header("Aim Feedback")]
        [SerializeField] private AnimationCurve minCurve;
        [SerializeField] private AnimationCurve maxCurve;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineHeight;
        [SerializeField] private float lineWidth;
        
        [Inject] private ModalFactory _modalFactory;
        
        private MovementView _move;
        private PlayerSelectionView _selectionView;
        private BagView _bagView;
        
        private Player _player;

        private Tile _lastSelectedTile;

        private List<Tile> _lastBlockedTiles = new();
        public void Initialize(Player player)
        {
            _player = player;
            _player.HolsterTransform = holsterTransform;
            _move = GetComponent<MovementView>();
            _selectionView = GetComponent<PlayerSelectionView>();
            _bagView = GetComponent<BagView>();

            _selectionView.Initialize(player);
            _bagView.Initialize(_player.Bag);

            _player.Tile
                .Where(tile => tile != null)
                .Subscribe(_move.ToTile)
                .AddTo(this);
            
            _player.Tile.SkipLatestValueOnSubscribe()
                .Subscribe(tile =>
                {
                    UpdateBlockedTiles(tile);
                    TriggerPlayerAnimation(AnimationState.MoveToTile);
                }).AddTo(this);

            _player.SkippedTurns
                .SkipLatestValueOnSubscribe()
                .Subscribe(_ => _move.SkipTurn())
                .AddTo(this);

            _player.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(this);
                
            _player.Weapon.AttackCharges.SkipLatestValueOnSubscribe()
                .Pairwise()
                .Where(pair => pair.Current < pair.Previous)
                .Subscribe(_ => TriggerPlayerAnimation(AnimationState.Attack1)).AddTo(this);
            _player.Health.SkipLatestValueOnSubscribe()
                .Where(health => health > 0)
                .Subscribe(_ => TriggerPlayerAnimation(AnimationState.GetDamaged)).AddTo(this);
            _player.Health.SkipLatestValueOnSubscribe()
                .Where(health => health <= 0)
                .Subscribe(_ =>
            {
                TriggerPlayerAnimation(AnimationState.Death);
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(1f)
                    .OnComplete(() => GameStateContainer.GameState.Value = GameState.Dead);
            }).AddTo(this);
        }

        private void Update()
        {
            ShowAimPath();
        }

        private void ShowAimPath()
        {
            
            if (InputHelper.IsSwiping() && !InputHelper.StartedOverUI)
            {
                var playerPosition = transform.position;
                playerPosition.y = 0;
                var weaponPosition = _player.Weapon.Tile.Value.FlatPosition;

                Vector3 startPosition = playerPosition;

                if (_player.Weapon.HasAttackCharge && !_player.Weapon.FixToHolster)
                    startPosition = weaponPosition;
                
                var swipeVector =
                    UIHelper.Camera
                        .GetWorldSwipeVector(InputHelper.StartPosition, InputHelper.GetTouchPosition())
                        .ShortenToTileRange(_player.Weapon.Range);


                if (_player.SwipedTiles.Count >= _player.Weapon.Range)
                {
                    float maxLength = Vector3.Distance(startPosition, _player.SwipedTiles[^1].FlatPosition);
                    swipeVector = Vector3.ClampMagnitude(swipeVector, maxLength);
                }

                lineRenderer.enabled = true;
                lineRenderer.positionCount = LinePoints;
                lineRenderer.SetPositions(GetLinePoints(startPosition, startPosition + swipeVector));
                float maxRangeProgress = swipeVector.magnitude / (_player.Weapon.Range * Island.TileDistance);
                lineRenderer.widthCurve = GetLerpedCurve(maxRangeProgress);
                lineRenderer.widthMultiplier = lineWidth;
            }
            else
            {
                lineRenderer.enabled = false;
            }
        }

        private Vector3[] GetLinePoints(Vector3 startPosition, Vector3 endPosition)
        {
            Vector3[] points = new Vector3[LinePoints];
            for (int i = 0; i < LinePoints; i++)
            {
                Vector3 position = Vector3.Lerp(startPosition, endPosition, (float)i / (LinePoints - 1));
                position.y = lineHeight;
                points[i] = position;
            }

            return points;
        }

        private void ClearSelection()
        {
            if (_lastSelectedTile != null)
            {
                _lastSelectedTile.RemoveSelector(_player);
                _lastSelectedTile = null;
            }
        }

        private void UpdateBlockedTiles(Tile currentTile)
        {
            for (int i = 0; i < _lastBlockedTiles.Count; i++)
            {
                if(!currentTile.Neighbours.Contains(_lastBlockedTiles[i]))
                    _lastBlockedTiles[i].RemoveSelector(_player);
            }
        }

        private void TriggerPlayerAnimation(AnimationState animationState)
        {
            animator.SetTrigger(animationState.ToString());
        }
        
        private AnimationCurve GetLerpedCurve(float curveLerp)
        {
            int keyCount = Mathf.Min(minCurve.length, maxCurve.length);
            AnimationCurve lerpedCurve = new AnimationCurve();

            for (int i = 0; i < keyCount; i++)
            {
                Keyframe keyA = minCurve[i];
                Keyframe keyB = maxCurve[i];

                float inTangent = Mathf.Lerp(keyA.inTangent, keyB.inTangent, curveLerp);
                float outTangent = Mathf.Lerp(keyA.outTangent, keyB.outTangent, curveLerp);
                float inWeight = Mathf.Lerp(keyA.inWeight, keyB.inWeight, curveLerp);
                float outWeight = Mathf.Lerp(keyA.outWeight, keyB.outWeight, curveLerp);
                float time = Mathf.Lerp(keyA.time, keyB.time, curveLerp);
                float value = Mathf.Lerp(keyA.value, keyB.value, curveLerp);

                Keyframe lerpedKey = new Keyframe(time, value);
                lerpedKey.inTangent = inTangent;
                lerpedKey.outTangent = outTangent;
                lerpedKey.inWeight = inWeight;
                lerpedKey.outWeight = outWeight;
            
                lerpedCurve.AddKey(lerpedKey);
            }

            return lerpedCurve;
        }
        
        
    }
}