using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Models;
using UniRx;
using Factories;
using TMPro;
using Zenject;

namespace Views
{
    public class TileView : MonoBehaviour
    {
        [SerializeField] private Transform visual;

        [SerializeField] private GameObject _attackSelection;
        [SerializeField] private GameObject _aimSelection;
        [SerializeField] private GameObject _moveSelection;
        [SerializeField] private GameObject _weaponSelection;
        [SerializeField] private GameObject _blockedSelection;

        [SerializeField] private ActionIndicatorView _actionIndicator;
        [SerializeField] private GameObject _actionLocked;
        [SerializeField] private GameObject _actionTile;
        
        [Header("Destruction")]
        [SerializeField] private float deathDuration;
        [SerializeField] private float fallDistance;
        [SerializeField] private AnimationCurve deathCurve;
        
        private List<GameObject> _visualObjects = new();
        private Tile _tile;

        [Inject] private ActionIndicatorFactory _actionIndicatorFactory;

        public Transform Visual => visual;

        public void Initialize(Tile tile)
        {
            _tile = tile;
            this.enabled = true;

            transform.position = tile.WorldPosition;
            float scale = _tile.HeightLevel < 0 ? .7f : 1f;
            visual.localScale = new Vector3(scale, 1, scale);

            _tile.Destroyed
                .Where(destroyed => destroyed)
                .Subscribe(_ => DestroyTile())
                .AddTo(this);
            
            if (tile.Island != null && tile.IsStartTile)
            {
                _actionTile.SetActive(true);
                tile.Island.IsHeartDestroyed.Subscribe(b => _actionLocked.SetActive(!b)).AddTo(this);
            }

            _tile.Selections.ObserveAdd().Subscribe(_ => UpdateSelection()).AddTo(this);
            _tile.Selections.ObserveRemove().Subscribe(_ => UpdateSelection()).AddTo(this);
            _tile.Selections.ObserveReset().Subscribe(_ => ResetSelection()).AddTo(this);

            _tile.WeaponOnTile.Where(_ => GameStateContainer.Player.Tile.Value != _tile).Subscribe(UpdateWeaponOnTileVisual).AddTo(this);

            // DEBUG
            _tile.DebugElevate.Where(b => b).Subscribe(_ => DebugElevate()).AddTo(this);
        }

        private void Update()
        {
            if (GameStateContainer.Player.SelectedTiles.Contains(_tile))
            {
                if (_actionIndicator == null)
                    _actionIndicator = _actionIndicatorFactory.CreateActionIndicator(_tile);
                _actionIndicator.Render(_tile);
            }
            else if(_actionIndicator != null)
            {
                _actionIndicator.Hide();
            }
        }

        public void RemoveVisuals()
        {
            foreach (var v in _visualObjects)
                Destroy(v);

            _visualObjects.Clear();
            ResetSelection();
        }

        public void AddVisual(GameObject visual)
        {
            _visualObjects.Add(visual);
        }

        private void DestroyTile()
        {
            var startPosition = transform.position;
            var endPosition = startPosition + (Vector3.down * fallDistance);

            Sequence sequence = DOTween.Sequence();
            
            sequence.Append(transform.DOMove(endPosition, deathDuration)
                .SetEase(deathCurve));
            sequence.OnComplete(() => Destroy(gameObject));
        }

        private void UpdateWeaponOnTileVisual(bool isOnTile)
        {
            _weaponSelection.SetActive(isOnTile);
        }

        private void UpdateSelection()
        {
            TileSelection prioritySelection = _tile.Selections.FirstOrDefault(selector => selector.Type == TileSelectionType.Attack);
            bool skipPrioritySelection = prioritySelection == null;
            bool isSelected = _tile.Selections.Count > 0;
            TileSelectionType type = prioritySelection?.Type ?? TileSelectionType.None;

            if (isSelected && skipPrioritySelection)
            {
                type = _tile.Selections[^1].Type;
            }
            
            _attackSelection.SetActive(type == TileSelectionType.Attack);
            _aimSelection.SetActive(type == TileSelectionType.Aim);
            _moveSelection.SetActive(type == TileSelectionType.Move);
            _blockedSelection.SetActive(type == TileSelectionType.Blocked);
        }

        private void ResetSelection()
        {
            _attackSelection.SetActive(false);
            _moveSelection.SetActive(false);
            _weaponSelection.SetActive(false);
            _blockedSelection.SetActive(false);
            _actionTile.SetActive(false);
        }

        private void DebugElevate()
        {
            transform.position = new Vector3(transform.position.x, 1, transform.position.z);
        }
    }
}