using System;
using System.Collections.Generic;
using Container;
using Factories;
using Factory;
using Models;
using UnityEngine;
using Zenject;

namespace Game
{
    public class IntuitiveInputManager
    {
        public const float MoveModeTimer = .2f;
        public bool InWeaponMode { get; private set; }
        public bool InMoveMode { get; private set; }
        
        [Inject] private PlayerManager _playerManager;
        [Inject] private PlayerFeedbackManager _playerFeedbackManager;
        [Inject] private GameAreaManager _gameAreaManager;

        public float MoveModeTracker { get; private set; }
        private float _moveModeDelay;

        public void ProcessInput()
        {
            InputHelper.ProcessSwipeInput();
            
            if (InputHelper.StartedOverUI || GameStateContainer.GameState.Value == GameState.Dead)
                return;
            
            var swipeVector = UIHelper.Camera
                .GetWorldSwipeVector(InputHelper.StartPosition, InputHelper.GetTouchPosition())
                .ShortenToTileRange(_playerManager.Weapon.Range);

            if (InputHelper.IsSwiping())
            {
                TryEnableMoveMode(swipeVector);
                if(InMoveMode && GameStateContainer.TurnState.Value == TurnState.PlayerTurnStart)
                    HandleMoveMode(swipeVector);
            }
            else if (InputHelper.SwipeEnded())
            {
                if(GameStateContainer.TurnState.Value == TurnState.PlayerTurnStart)
                    HandleInput(swipeVector);
                InMoveMode = false;
                MoveModeTracker = 0;
            }
            else if (InputHelper.TouchEnded() && GameStateContainer.TurnState.Value == TurnState.PlayerTurnStart)
            {
                if (InWeaponMode)
                {
                    _playerManager.Weapon.ReturnToHolster();
                    InWeaponMode = false;
                    return;
                }
                
                GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
                _playerManager.Player.SkippedTurns.Value++;
                _playerManager.Player.Weapon.RecoverAttackCharge();
            }

            bool returnWeapon = InWeaponMode && !_playerManager.Weapon.HasAttackCharge;
            if (GameStateContainer.TurnState.Value == TurnState.IslandTurn && returnWeapon)
            {
                //_playerManager.Weapon.ReturnToHolster();
                InWeaponMode = false;
            }
        }

        private void HandleInput(Vector3 swipeVector)
        {
            if (GameStateContainer.GameState.Value == GameState.Island)
            {
                if (InWeaponMode && _playerManager.Weapon.HasAttackCharge)
                    HandleWeaponAction(swipeVector);
                else if (InMoveMode)
                    HandleMoveMode(swipeVector);
                else
                    HandleDefaultMode(swipeVector);
            }
            else if (GameStateContainer.GameState.Value == GameState.Ship)
            {
                HandleMoveMode(swipeVector);
            }
        }

        private void HandleWeaponAction(Vector3 swipeVector)
        {
            if(_playerManager.Weapon.HasAttackCharge)
            {
                var tilesFromWeapon = GetSwipedTiles(_playerManager.Weapon.Tile.Value,
                    _gameAreaManager.TileCollection.GetClosestTileFromPosition(_playerManager.Weapon.Tile.Value.WorldPosition + swipeVector));

                var matching =
                    tilesFromWeapon.GetMatchingTiles(tile => tile.CurrentUnit.Value != GameStateContainer.Player);

                if (matching.HasAnyUnit())
                {
                    Debug.Log("Attack");
                    AttackTiles(swipeVector);
                }
                else
                {
                    Debug.Log("Nothing");
                }
            }
            else
            {
                _playerManager.Weapon.ReturnToHolster();
                InWeaponMode = false;
            }
        }

        private void HandleMoveMode(Vector3 swipeVector)
        {
            var tilesFromPlayer = GetSwipedTiles(_playerManager.Player.Tile.Value,
                _playerManager.Player.Tile.Value.TileCollection.GetClosestTileFromPosition(_playerManager.Player.Tile.Value.WorldPosition + swipeVector));

            if(tilesFromPlayer.Count < 2)
                return;

            tilesFromPlayer.RemoveAt(0);
            if (tilesFromPlayer[0].HasUnit)
            {
                InMoveMode = false;
            }
            else
            {
                MovePlayer(swipeVector);

                if (InWeaponMode)
                {
                    _playerManager.Weapon.ReturnToHolster();
                    InWeaponMode = false;
                }
            }
        }

        private void HandleDefaultMode(Vector3 swipeVector)
        {
            var tilesFromPlayer = GetSwipedTiles(_playerManager.Player.Tile.Value,
                _playerManager.Player.Tile.Value.TileCollection.GetClosestTileFromPosition(_playerManager.Player.Tile.Value.WorldPosition + swipeVector));

            if (tilesFromPlayer.GetMatchingTiles(tile => tile.CurrentUnit.Value != GameStateContainer.Player).HasAnyUnit())
            {
                AttackTiles(swipeVector);
            }
            else
            {
                MovePlayer(swipeVector);
                InWeaponMode = false;
            }
        }

        private void AttackTiles(Vector3 swipeVector)
        {
            CloseOpenUIElements();
            if (GameStateContainer.GameState.Value == GameState.Ship)
                return;

            Tile startTile = _playerManager.Weapon.Tile.Value;
            Tile endTile = _gameAreaManager.TileCollection.GetClosestTileFromPosition(startTile.WorldPosition + swipeVector);
            List<Tile> tiles = GetSwipedTiles(startTile, endTile);
            List<Tile> attackTiles = new();

            int attackDamage = _playerManager.Weapon.GetAttackDamage();

            bool bounceBack = false;

            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i].CurrentUnit.Value == _playerManager.Player)
                    continue;
                
                attackTiles.Add(tiles[i]);
                Unit unit = tiles[i].CurrentUnit.Value;
                if (unit != null && (unit.Health.Value > attackDamage || unit.IsInvincible.Value))
                {
                    endTile = tiles[i];
                    bounceBack = true;
                    break;
                }
            }

            InWeaponMode = !bounceBack;

            if (!bounceBack && endTile.HasUnit)
            {
                Debug.Log("End Tile Occupied Feedback");
                return;
            }
            
            _playerManager.Weapon.AttackDirection.Value = swipeVector.normalized;
            _playerManager.Weapon.AttackTiles(attackTiles, startTile);
            _playerManager.Weapon.UseAttackCharge();
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
        }
        

        private void MovePlayer(Vector3 swipeVector)
        {
            Tile tile = _gameAreaManager.TileCollection.GetClosestTileFromPosition(
                _playerManager.Player.Tile.Value.WorldPosition + swipeVector);
            if(tile == _playerManager.Player.Tile.Value)
                return;
            
            CloseOpenUIElements();
            EndActiveTrade();
            bool movedPlayer = _playerManager.Player.TryMoveInDirection(InputHelper.SwipeVector);
            if (movedPlayer)
            {
                GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
                _playerManager.Weapon.ReturnToHolster();
                    
            }
        }

        private void TryEnableMoveMode(Vector3 swipeVector)
        {
            if(InMoveMode)
                return;

            var worldPosition = _playerManager.Player.Tile.Value.WorldPosition + swipeVector;
            Tile worldTile = _gameAreaManager.TileCollection.GetTileClosestToPosition(worldPosition);
            
            if(worldTile == null)
                return;

            bool canUpdateMoveMode = InputHelper.IsSwipeStationary() && !worldTile.HasUnit &&
                                     _playerManager.Player.Tile.Value.Neighbours.Contains(worldTile);
            if (canUpdateMoveMode)
            {
                if (_playerManager.Player.SelectedTiles.Count == 0 && InputHelper.IsSwipe)
                {
                    if (_moveModeDelay < MoveModeTimer)
                        _moveModeDelay += Time.deltaTime;
                    else
                        MoveModeTracker += Time.deltaTime;
                    
                }
                else
                {
                    MoveModeTracker = 0;
                    _moveModeDelay = 0;
                }
                
                if (MoveModeTracker >= MoveModeTimer)
                {
                    InMoveMode = true;
                    MoveModeTracker = 0;
                    _moveModeDelay = 0;
                }
            }
            else
            {
                MoveModeTracker = 0;
                _moveModeDelay = 0;
            }
        }

        private List<Tile> GetSwipedTiles(Tile startTile, Tile endTile)
        {
            List<Tile> tiles = startTile.TileCollection
                .GetSwipedTiles(startTile, endTile);
            
            tiles = tiles.GetTilesInWeaponRange(_playerManager.Weapon);
                
            return tiles;

        }
        
        private void CloseOpenUIElements()
        {
            if (GameStateContainer.OpenUI && !InputHelper.StartedOverUI)
                GameStateContainer.CloseOpenUIElements.Execute();
        }
        
        private void EndActiveTrade()
        {
            if (_gameAreaManager.Ship != null)
            {
                if(_gameAreaManager.Ship.Merchant.InTrade.Value)
                    _gameAreaManager.Ship.Merchant.EndTrade();
                if(_gameAreaManager.Ship.BlackSmith.InTrade.Value)
                    _gameAreaManager.Ship.BlackSmith.EndTrade();
            }
        }
    }
}