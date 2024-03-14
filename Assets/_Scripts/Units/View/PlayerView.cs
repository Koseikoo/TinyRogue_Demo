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
        
        [SerializeField] private Animator animator;
        [SerializeField] private Transform holsterTransform;
        [SerializeField] private LineRenderer lineRenderer;
        
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
                var weaponPosition = _player.Weapon.Tile.Value.WorldPosition;

                Vector3 startPosition = playerPosition;

                if (_player.Weapon.HasAttackCharge && !_player.Weapon.FixToHolster)
                    startPosition = weaponPosition;
                
                var swipeVector =
                    UIHelper.Camera
                        .GetWorldSwipeVector(InputHelper.StartPosition, InputHelper.GetTouchPosition())
                        .ShortenToTileRange(_player.Weapon.Range);
            
                Tile tile = _player.Tile.Value.TileCollection.GetClosestTileFromPosition(startPosition + swipeVector);
                
                ClearSelection();
                if (_player.SelectedTiles.Count > 0 && !tile.HasUnit)
                {
                    tile.AddSelector(new TileSelection(_player, TileSelectionType.Aim));
                    _lastSelectedTile = tile;
                }

                lineRenderer.enabled = true;
                lineRenderer.positionCount = LinePoints;
                lineRenderer.SetPositions(GetLinePoints(startPosition + Vector3.up, startPosition + swipeVector + Vector3.up));
            }
            else
            {
                lineRenderer.enabled = false;
                ClearSelection();
            }
        }

        private Vector3[] GetLinePoints(Vector3 startPosition, Vector3 endPosition)
        {
            Vector3[] points = new Vector3[LinePoints];
            for (int i = 0; i < LinePoints; i++)
            {
                points[i] = Vector3.Lerp(startPosition, endPosition, (float)i / (LinePoints - 1));
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
        
        
    }
}