using System.Collections.Generic;
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

        public void UpdateTileSelection()
        {
            if(GameStateContainer.GameState.Value == GameState.Ship)
                return;
            
            if (InputHelper.IsSwiping() && !InputHelper.StartedOverUI)
            {
                Tile startTile = GameStateContainer.Player.Weapon.HasAttackCharge
                    ? _playerManager.Weapon.Tile.Value
                    : _playerManager.Player.Tile.Value;

                var swipeVector =
                    UIHelper.Camera
                        .GetWorldSwipeVector(InputHelper.StartPosition, InputHelper.GetTouchPosition())
                        .ShortenToTileRange(_playerManager.Weapon.Range);
                
                Tile endTile = _gameAreaManager.TileCollection.GetClosestTileFromPosition(startTile.WorldPosition + swipeVector);
    
                if (swipeVector.magnitude < Island.TileDistance || _inputManager.InMoveMode)
                {
                    _playerManager.Player.SelectedTiles.Clear();
                    return;
                }

                List<Tile> tiles = _gameAreaManager.Island.Tiles
                    .GetSwipedTiles(startTile, endTile)
                    .GetTilesInWeaponRange(_playerManager.Weapon)
                    .WithUnitOnTile();
                tiles.Remove(endTile);
                
                _playerManager.Weapon.AimedPoint.Value = endTile.WorldPosition;
                for (int i = _playerManager.Player.SelectedTiles.Count - 1; i >= 0; i--)
                {
                    if (!tiles.Contains(_playerManager.Player.SelectedTiles[i]))
                    {
                        _playerManager.Player.SelectedTiles[i].RemoveSelector(_playerManager.Player);
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
            }
    
            if (InputHelper.SwipeEnded())
            {
                for (int i = 0; i < _playerManager.Player.SelectedTiles.Count; i++)
                    _playerManager.Player.SelectedTiles[i].RemoveSelector(_playerManager.Player);
                
                _playerManager.Player.SelectedTiles.Clear();
            }
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