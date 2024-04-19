using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using UniRx;
using DG.Tweening;
using Factory;
using Game;
using Zenject;
using AnimationState = Models.AnimationState;

namespace Views
{
    [RequireComponent(typeof(MovementView), typeof(BagView))]
    public class PlayerView : MonoBehaviour
    {
        private const int LinePoints = 5;

        public static float EnterDuration;
        public static float EnterDelay;
        
        [SerializeField] private Animator animator;
        [SerializeField] private Transform holsterTransform;
        
        [Header("Aim Feedback")]
        [SerializeField] private AnimationCurve minCurve;
        [SerializeField] private AnimationCurve maxCurve;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineHeight;
        [SerializeField] private float lineWidth;
        
        [Header("Enter/Exit Animation")]
        [SerializeField] private AnimationCurve enterCurve;
        [SerializeField] private float enterDuration;
        [SerializeField] private float jumpHeight;
        [SerializeField] private float delay;
        
        [Inject] private GameAreaManager _gameAreaManager;
        
        private MovementView _move;
        private PlayerSelectionView _selectionView;
        private BagView _bagView;
        
        private Player _player;

        private Tile _lastSelectedTile;

        private List<Tile> _lastBlockedTiles = new();
        public void Initialize(Player player)
        {
            EnterDelay = delay;
            EnterDuration = enterDuration;
            
            _player = player;
            _player.HolsterTransform = holsterTransform;
            _move = GetComponent<MovementView>();
            _selectionView = GetComponent<PlayerSelectionView>();
            _bagView = GetComponent<BagView>();

            _selectionView.Initialize(player);
            _bagView.Initialize(_player.Bag);

            _player.Tile
                .Pairwise()
                .Where(pair => pair.Current != null && pair.Current.Island == pair.Previous?.Island)
                .Subscribe(pair =>
                {
                    _move.ToTile(pair.Current);
                    UpdateBlockedTiles(pair.Current);
                    TriggerPlayerAnimation(AnimationState.MoveToTile);
                })
                .AddTo(this);

            _player.EnterIsland.Subscribe(_ =>
            {
                EnterIslandAnimation();
            }).AddTo(this);
            
            _player.ExitIsland
                .Subscribe(ExitIslandAnimation)
                .AddTo(this);

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
                lineRenderer.widthCurve = MathHelper.GetLerpedCurve(minCurve, maxCurve, maxRangeProgress);
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

        private void EnterIslandAnimation()
        {
            GameStateContainer.TurnState.Value = TurnState.Disabled;
            Transform player = transform; 
            
            var island = _gameAreaManager.Island;
            Vector3 startPosition = island.IslandShipPosition;
            Vector3 endPosition = island.StartTile.WorldPosition;
            Vector3 anchorPosition = Vector3.Lerp(startPosition, endPosition, .5f) + (Vector3.up * jumpHeight);

            player.position = startPosition;
            player.localScale = Vector3.zero;
            var forward = (island.StartTile.FlatPosition - startPosition);
            forward.y = 0;
            forward.Normalize();
            player.forward = forward;

            Sequence sequence = DOTween.Sequence();

            sequence.Insert(delay, DOTween.To(() => 0f, t =>
            {
                var evalT = enterCurve.Evaluate(t);
                var position = MathHelper.BezierLerp(startPosition, anchorPosition, endPosition, evalT);
                player.position = position;
                var scale = Mathf.Lerp(0f, 1f, evalT) * Vector3.one;
                player.localScale = scale;
                holsterTransform.localScale = scale;

            }, 1f, enterDuration));

            sequence.OnComplete(() =>
            {
                GameStateContainer.TurnState.Value = TurnState.PlayerTurnStart;
            });
        }

        private void ExitIslandAnimation(Action action)
        {
            var turnState = GameStateContainer.TurnState.Value;
            GameStateContainer.TurnState.Value = TurnState.Disabled;
            
            var island = _gameAreaManager.Island;
            Vector3 startPosition = island.StartTile.WorldPosition;
            Vector3 endPosition = island.IslandShipPosition;
            Vector3 anchorPosition = Vector3.Lerp(startPosition, endPosition, .5f) + Vector3.up;
            
            Sequence sequence = DOTween.Sequence();

            sequence.Insert(0f, DOTween.To(() => 0f, t =>
            {
                var evalT = enterCurve.Evaluate(t);
                var position = MathHelper.BezierLerp(startPosition, anchorPosition, endPosition, evalT);
                transform.position = position;
                var scale = Mathf.Lerp(1f, 0f, evalT) * Vector3.one;
                transform.localScale = scale;
                holsterTransform.localScale = scale;
                
            }, 1f, enterDuration));
            
            sequence.InsertCallback(enterDuration + .4f, () =>
            {
                action?.Invoke();
                GameStateContainer.TurnState.Value = turnState;
            });
        }
    }
}