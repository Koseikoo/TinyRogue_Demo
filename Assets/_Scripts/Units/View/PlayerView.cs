using System;
using System.Collections.Generic;
using UnityEngine;
using Models;
using UniRx;
using DG.Tweening;
using Factories;
using Factory;
using Game;
using TinyRogue;
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
        [SerializeField]
        private PlayerAnimationView animationView;
        
        [Header("Aim Feedback")]
        [SerializeField] private AnimationCurve minCurve;
        [SerializeField] private AnimationCurve maxCurve;
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineHeight;
        [SerializeField] private float lineWidth;
        
        [Header("Island Enter/Exit Animation")]
        [SerializeField] private AnimationCurve enterCurve;
        [SerializeField] private float enterDuration;
        [SerializeField] private float jumpHeight;
        [SerializeField] private float delay;
        
        [Header("Ship Enter/Exit Animation")]
        [SerializeField] private AnimationCurve shipEnterCurve;
        [SerializeField] private float shipEnterDuration;
        [SerializeField] private float shipEnterDelay;
        [SerializeField] private float dropHeight;
        
        [Inject] private GameAreaManager _gameAreaManager;
        [Inject] private LootFactory _lootFactory;
        
        private MovementView _move;
        private PlayerSelectionView _selectionView;
        private BagView _bagView;
        
        private Player _player;

        private Tile _lastSelectedTile;

        private Tween _lookTween;

        private List<Tile> _lastBlockedTiles = new();
        public void Initialize(Player player)
        {
            EnterDelay = delay;
            EnterDuration = enterDuration;
            animationView.Initialize(player);
            
            _player = player;
            _player.HolsterTransform = holsterTransform;
            _move = GetComponent<MovementView>();
            _selectionView = GetComponent<PlayerSelectionView>();
            _bagView = GetComponent<BagView>();

            _selectionView.Initialize(player);
            _bagView.Initialize(_player.Bag);

            _player.Tile
                .Pairwise()
                .Where(pair => pair.Current != null)
                .Subscribe(pair =>
                {
                    _move.ToTile(pair.Current);
                    UpdateBlockedTiles(pair.Current);
                    TriggerPlayerAnimation(AnimationState.MoveToTile);
                })
                .AddTo(this);

            _player.StartAttackCommand
                .Subscribe(_ => TriggerPlayerAnimation(AnimationState.Attack1))
                .AddTo(this);

            _player.EnterIsland.Subscribe(_ =>
            {
                EnterIslandAnimation();
            }).AddTo(this);

            _player.LookDirection
                .Where(_ => _player.Tile.Value != null && _player.Tile.Value.Neighbours.Count > 0)
                .Subscribe(LookTowardsClosestTile)
                .AddTo(this);
            
            _player.ExitIsland
                .Subscribe(ExitIslandAnimation)
                .AddTo(this);

            _player.EnterShip
                .Subscribe(EnterShipAnimation)
                .AddTo(this);

            _player.SkippedTurns
                .SkipLatestValueOnSubscribe()
                .Subscribe(_ => _move.SkipTurn())
                .AddTo(this);

            _player.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(this);
            
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

        private void UpdateBlockedTiles(Tile currentTile)
        {
            for (int i = 0; i < _lastBlockedTiles.Count; i++)
            {
                if(!currentTile.Neighbours.Contains(_lastBlockedTiles[i]))
                {
                    _lastBlockedTiles[i].RemoveSelector(_player);
                }
            }
        }

        private void LookTowardsClosestTile(Vector3 direction)
        {
            Vector3 currentDirection = transform.forward;
            Tile currentlyFacedTile = IslandHelper.GetTileInDirection(_player.Tile.Value, currentDirection);
            Tile newDirectionTile = IslandHelper.GetTileInDirection(_player.Tile.Value, direction);
            
            Vector3 snappedDirection = (newDirectionTile.FlatPosition - _player.Tile.Value.FlatPosition).normalized;
            float dot = Vector3.Dot(currentDirection, snappedDirection);

            if (dot < .9f)
            {
                transform.forward = snappedDirection;
                
                _lookTween?.Kill();
                _lookTween = DOTween.To(() => 0f, t =>
                {
                    transform.forward = Vector3.Lerp(currentDirection, snappedDirection, t).normalized;
                }, 1f, .2f);
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
            
            Island island = _gameAreaManager.Island;
            Vector3 startPosition = island.IslandShipPosition;
            Vector3 endPosition = island.StartTile.WorldPosition;
            Vector3 anchorPosition = Vector3.Lerp(startPosition, endPosition, .5f) + (Vector3.up * jumpHeight);

            player.position = startPosition;
            player.localScale = Vector3.zero;
            Vector3 forward = (island.StartTile.FlatPosition - startPosition);
            forward.y = 0;
            forward.Normalize();
            player.forward = forward;

            Sequence sequence = DOTween.Sequence();

            sequence.Insert(delay, DOTween.To(() => 0f, t =>
            {
                float evalT = enterCurve.Evaluate(t);
                Vector3 position = MathHelper.BezierLerp(startPosition, anchorPosition, endPosition, evalT);
                player.position = position;
                Vector3 scale = Mathf.Lerp(0f, 1f, evalT) * Vector3.one;
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
            TurnState turnState = GameStateContainer.TurnState.Value;
            GameStateContainer.TurnState.Value = TurnState.Disabled;
            
            Island island = _gameAreaManager.Island;
            Vector3 startPosition = island.StartTile.WorldPosition;
            Vector3 endPosition = island.IslandShipPosition;
            Vector3 anchorPosition = Vector3.Lerp(startPosition, endPosition, .5f) + Vector3.up;
            
            Sequence sequence = DOTween.Sequence();

            sequence.Insert(0f, DOTween.To(() => 0f, t =>
            {
                float evalT = enterCurve.Evaluate(t);
                Vector3 position = MathHelper.BezierLerp(startPosition, anchorPosition, endPosition, evalT);
                transform.position = position;
                Vector3 scale = Mathf.Lerp(1f, 0f, evalT) * Vector3.one;
                transform.localScale = scale;
                holsterTransform.localScale = scale;
                
            }, 1f, enterDuration));
            
            sequence.InsertCallback(enterDuration + .4f, () =>
            {
                action?.Invoke();
                GameStateContainer.TurnState.Value = turnState;
            });
        }

        private void EnterShipAnimation(Action action)
        {
            Tile startTile = _gameAreaManager.Ship.StartTile;

            Vector3 endPosition = startTile.WorldPosition;
            Vector3 startPosition = endPosition + (Vector3.up * dropHeight);
            
            transform.position = startPosition;
            transform.localScale = Vector3.one;
            
            Sequence sequence = DOTween.Sequence();

            sequence.Insert(shipEnterDelay, transform.DOMove(endPosition, shipEnterDuration));
            sequence.SetEase(shipEnterCurve);
            sequence.OnComplete(() =>
            {
                action?.Invoke();
            });
        }
    }
}