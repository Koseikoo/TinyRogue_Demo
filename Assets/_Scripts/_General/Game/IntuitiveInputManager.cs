using System.Collections.Generic;
using Models;
using UnityEngine;
using Zenject;

namespace Game
{
    public class IntuitiveInputManager
    {
        public const float MoveModeTimer = .2f;
        public bool InMoveMode { get; private set; }
        
        [Inject] private PlayerManager _playerManager;
        [Inject] private GameAreaManager _gameAreaManager;
        [Inject] private MoveInputTracker _moveInputTracker;
        
        [Inject] private CameraModel _cameraModel;

        public float MoveModeTracker { get; private set; }
        private float _moveModeDelay;

        public void ProcessInput()
        {
            InputHelper.ProcessSwipeInput();
            
            if (InputHelper.StartedOverUI || GameStateContainer.GameState.Value == GameState.Dead)
            {
                return;
            }

            Vector3 swipeVector = UIHelper.Camera
                .GetWorldSwipeVector();

            if (InputHelper.IsSwiping())
            {
                TryEnableMoveMode(swipeVector);
                if(InMoveMode && GameStateContainer.TurnState.Value == TurnState.PlayerTurnStart)
                {
                    HandleMoveMode(swipeVector);
                }
            }
            else if (InputHelper.SwipeEnded())
            {
                if(GameStateContainer.TurnState.Value == TurnState.PlayerTurnStart)
                {
                    HandleInput(swipeVector);
                }

                InMoveMode = false;
                MoveModeTracker = 0;
            }
            else if (InputHelper.TouchEnded() && GameStateContainer.TurnState.Value == TurnState.PlayerTurnStart)
            {
                GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
                bool hadOpenElements = CloseOpenUIElements();

                if (!hadOpenElements)
                {
                    _playerManager.Player.SkippedTurns.Value++;
                }
            }
        }

        private void HandleInput(Vector3 swipeVector)
        {
            if (GameStateContainer.GameState.Value == GameState.Island)
            {
                if (InMoveMode)
                {
                    HandleMoveMode(swipeVector);
                }
                else
                {
                    HandleDefaultMode(swipeVector);
                }
            }
            else if (GameStateContainer.GameState.Value == GameState.Ship)
            {
                HandleMoveMode(swipeVector);
            }
        }

        private void HandleMoveMode(Vector3 swipeVector)
        {
            List<Tile> tilesFromPlayer = GetSwipedTiles(_playerManager.Player.Tile.Value,
                _playerManager.Player.Tile.Value.TileCollection.GetClosestTileFromPosition(_playerManager.Player.Tile.Value.FlatPosition + swipeVector));

            if(tilesFromPlayer.Count < 2)
            {
                return;
            }

            tilesFromPlayer.RemoveAt(0);
            if (tilesFromPlayer[0].HasUnit)
            {
                InMoveMode = false;
            }
            else
            {
                MovePlayer(swipeVector);
            }
        }

        private void HandleDefaultMode(Vector3 swipeVector)
        {
            List<Tile> tilesFromPlayer = GetSwipedTiles(_playerManager.Player.Tile.Value,
                _playerManager.Player.Tile.Value.TileCollection.GetClosestTileFromPosition(_playerManager.Player.Tile.Value.FlatPosition + swipeVector));

            if (_playerManager.Player.SelectedTiles.Count > 0)
            {
                AttackTiles(swipeVector);
            }
            else
            {
                MovePlayer(swipeVector);
            }
        }

        private void AttackTiles(Vector3 swipeVector)
        {
            bool hadOpenElements = CloseOpenUIElements();
            if (GameStateContainer.GameState.Value == GameState.Ship || hadOpenElements)
            {
                return;
            }

            Tile startTile = _playerManager.Player.Tile.Value;
            Tile endTile = _gameAreaManager.TileCollection.GetClosestTileFromPosition(startTile.FlatPosition + swipeVector);
            List<Tile> tiles = GetSwipedTiles(startTile, endTile);
            List<Tile> attackTiles = new();

            int attackDamage = _playerManager.Player.Weapon.Value.Damage;

            bool bounceBack = false;

            for (int i = 0; i < tiles.Count; i++)
            {
                attackTiles.Add(tiles[i]);
                Unit unit = tiles[i].Unit.Value;
                
                if(unit is Player)
                {
                    continue;
                }

                if (unit != null && (unit.Health.Value > attackDamage || unit.IsInvincible.Value))
                {
                    endTile = tiles[i];
                    bounceBack = true;
                    break;
                }
            }
            
            if (endTile.HasUnit && !bounceBack)
            {
                Debug.Log("End Tile Occupied Feedback");
                _cameraModel.SideShakeCommand.Execute();
                return;
            }

            _playerManager.Player.AttackTiles(attackTiles.ToArray());
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
        }
        

        private void MovePlayer(Vector3 swipeVector)
        {
            Tile tile = _gameAreaManager.TileCollection.GetClosestTileFromPosition(
                _playerManager.Player.Tile.Value.FlatPosition + swipeVector);
            if(tile == _playerManager.Player.Tile.Value)
            {
                return;
            }

            bool hadOpenElements = CloseOpenUIElements();
            EndActiveTrade();
            
            if(hadOpenElements)
            {
                return;
            }

            bool movedPlayer = _playerManager.Player.TryMoveInDirection(InputHelper.SwipeVector);
            if (movedPlayer)
            {
                GameStateContainer.TurnState.Value = TurnState.PlayerTurnEnd;
                //_moveInputTracker.AddMovement(swipeVector.normalized);
            }
        }

        private void TryEnableMoveMode(Vector3 swipeVector)
        {
            if(InMoveMode)
            {
                return;
            }

            Vector3 worldPosition = _playerManager.Player.Tile.Value.FlatPosition + swipeVector;
            Tile worldTile = _gameAreaManager.TileCollection.GetTileClosestToPosition(worldPosition);
            
            if(worldTile == null)
            {
                return;
            }

            bool canUpdateMoveMode = InputHelper.IsSwipeStationary() && !worldTile.HasUnit &&
                                     swipeVector.magnitude <= Island.TileDistance;
            if (canUpdateMoveMode)
            {
                if (_playerManager.Player.SelectedTiles.Count == 0 && InputHelper.IsSwipe)
                {
                    if (_moveModeDelay < MoveModeTimer)
                    {
                        _moveModeDelay += Time.deltaTime;
                    }
                    else
                    {
                        MoveModeTracker += Time.deltaTime;
                    }
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
            
            return tiles;

        }
        
        private bool CloseOpenUIElements()
        {
            bool hasOpenUI = GameStateContainer.OpenUI;
            if (hasOpenUI && !InputHelper.StartedOverUI)
            {
                GameStateContainer.CloseOpenUIElements.Execute();
            }

            return hasOpenUI;
        }
        
        private void EndActiveTrade()
        {
            if (_gameAreaManager.Ship != null)
            {
                if(_gameAreaManager.Ship.Merchant.InTrade.Value)
                {
                    _gameAreaManager.Ship.Merchant.EndTrade();
                }

                if(_gameAreaManager.Ship.BlackSmith.InTrade.Value)
                {
                    _gameAreaManager.Ship.BlackSmith.EndTrade();
                }
            }
        }
    }
}