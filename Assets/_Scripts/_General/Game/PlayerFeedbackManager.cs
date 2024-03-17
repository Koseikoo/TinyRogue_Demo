using System.Collections.Generic;
using System.Linq;
using Container;
using Factories;
using Factory;
using Models;
using UnityEngine;
using Views;
using Zenject;

namespace Game
{
    public class PlayerFeedbackManager
    {
        [Inject] private ModalFactory _modalFactory;

        [Inject] private PlayerManager _playerManager;
        [Inject] private GameAreaManager _gameAreaManager;
        [Inject] private IntuitiveInputManager _inputManager;
        
        [Inject] private FeedbackFactory _feedbackFactory;

        public Vector3 WorldSwipeVector { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        
        private EnemyInfoModalView _currentEnemyModal;
        private WeaponBoundFeedbackView _weaponBoundFeedbackView;

        private Tile _lastAimTile;

        public void UpdateTileSelection()
        {
            var swipeVector =
                UIHelper.Camera
                    .GetWorldSwipeVector(InputHelper.StartPosition, InputHelper.GetTouchPosition())
                    .ShortenToTileRange(_playerManager.Weapon.Range);
            
            if (GameStateContainer.GameState.Value == GameState.Ship)
            {
                Tile endTile = _gameAreaManager.TileCollection.GetClosestTileFromPosition(_playerManager.Player.Tile.Value.WorldPosition + swipeVector);

                List<Tile> tiles = _gameAreaManager.TileCollection
                    .GetSwipedTiles(_playerManager.Player.Tile.Value, endTile)
                    .GetTilesInWeaponRange(_playerManager.Weapon);
                
                UpdateSwipedTiles(new(tiles));
                return;
            }
            
            if (swipeVector.magnitude < Island.TileDistance || _inputManager.InMoveMode)
            {
                ClearTiles();
                return;
            }
            
            if (InputHelper.IsSwiping() && !InputHelper.StartedOverUI)
            {
                Tile startTile = GameStateContainer.Player.Weapon.HasAttackCharge
                    ? _playerManager.Weapon.Tile.Value
                    : _playerManager.Player.Tile.Value;
                Tile endTile = _gameAreaManager.TileCollection.GetClosestTileFromPosition(startTile.WorldPosition + swipeVector);

                List<Tile> tiles = _gameAreaManager.TileCollection
                    .GetSwipedTiles(startTile, endTile)
                    .GetTilesInWeaponRange(_playerManager.Weapon);
                    
                _playerManager.Weapon.AimedPoint.Value = endTile.WorldPosition;
                UpdateSwipedTiles(new(tiles));
                UpdateSelectedTiles(new List<Tile>(tiles)
                    .Truncate(1)
                    .WithUnitOnTile());
                
                UpdateAimTile();
            }
    
            if (InputHelper.SwipeEnded())
            {
                ClearTiles();
            }
        }

        private void ClearTiles()
        {
            UpdateSwipedTiles(new());
            UpdateSelectedTiles(new());
            _lastAimTile?.RemoveSelector(_playerManager.Player, TileSelectionType.Aim);
                
            _playerManager.Player.SwipedTiles.Clear();
            _playerManager.Player.SelectedTiles.Clear();
        }

        private void UpdateSwipedTiles(List<Tile> tiles)
        {
            for (int i = _playerManager.Player.SwipedTiles.Count - 1; i >= 0; i--)
            {
                
                if (!tiles.Contains(_playerManager.Player.SwipedTiles[i]))
                    _playerManager.Player.SwipedTiles.RemoveAt(i);
            }
    
            for (int i = 0; i < tiles.Count; i++)
            {
                if (!_playerManager.Player.SwipedTiles.Contains(tiles[i]) && tiles[i].CurrentUnit.Value != _playerManager.Player)
                    _playerManager.Player.SwipedTiles.Add(tiles[i]);
            }
            
            _playerManager.Player.SwipedTiles = new(_playerManager.Player.SwipedTiles.OrderBy(tile =>
                Vector3.Distance(tile.WorldPosition, _playerManager.Weapon.Tile.Value.WorldPosition)));
        }

        private void UpdateAimTile()
        {
            List<Tile> tiles = new List<Tile>(_playerManager.Player.SwipedTiles);
            bool showAimedTile = _playerManager.Player.SelectedTiles.Count > 0 && !tiles[^1].HasUnit;
            _lastAimTile?.RemoveSelector(_playerManager.Player, TileSelectionType.Aim);
            if (showAimedTile)
            {
                _lastAimTile = tiles[^1];
                _lastAimTile.AddSelector(new(_playerManager.Player, TileSelectionType.Aim));
            }
        }

        private void UpdateSelectedTiles(List<Tile> tiles)
        {
            for (int i = _playerManager.Player.SelectedTiles.Count - 1; i >= 0; i--)
            {
                if (!tiles.Contains(_playerManager.Player.SelectedTiles[i]))
                {
                    _playerManager.Player.SelectedTiles[i].RemoveSelector(_playerManager.Player, TileSelectionType.Attack);
                    _playerManager.Player.SelectedTiles.RemoveAt(i);
                }
            }
    
            for (int i = 0; i < tiles.Count; i++)
            {
                if (!_playerManager.Player.SelectedTiles.Contains(tiles[i]) && tiles[i].CurrentUnit.Value != _playerManager.Player)
                {
                    tiles[i].AddSelector(new TileSelection(_playerManager.Player, TileSelectionType.Attack));
                    _playerManager.Player.SelectedTiles.Add(tiles[i]);
                }
            }

            _playerManager.Player.SelectedTiles = new(_playerManager.Player.SelectedTiles.OrderBy(tile =>
                Vector3.Distance(tile.WorldPosition, _playerManager.Weapon.Tile.Value.WorldPosition)));
        }
        
        public void TryDestroyEnemyInfoModal()
        {
            if (_currentEnemyModal != null)
                _currentEnemyModal.DestroyModal();
        }
    
        public void UpdateCurrentUnitModal(Unit unit)
        {
            if (_currentEnemyModal != null)
            {
                bool isSameUnit = unit == _currentEnemyModal.Unit;
                TryDestroyEnemyInfoModal();
                
                if(!isSameUnit)
                    _currentEnemyModal = _modalFactory.CreateUnitInfoModal(unit);
            }
            else
            {
                _currentEnemyModal = _modalFactory.CreateUnitInfoModal(unit);
            }
        }
    }
}