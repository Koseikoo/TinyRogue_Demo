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
        [Inject] private FeedbackFactory _feedbackFactory;

        [Inject] private PlayerManager _playerManager;
        [Inject] private GameAreaManager _gameAreaManager;

        public Vector3 WorldSwipeVector { get; private set; }
        public Vector3 WorldPosition { get; private set; }
        
        private EnemyInfoModalView _currentEnemyModal;
        private WeaponBoundFeedbackView _weaponBoundFeedbackView;

        private Tile _lastAimTile;

        private List<Tile> lastHighlightedTiles = new();

        public void HighlightTiles()
        {
            HideHighlights();
            foreach (Tile tile in _playerManager.Player.AimedTiles)
            {
                if (tile.HasUnit)
                {
                    // attack highlight
                    tile.AddSelector(new(_playerManager.Player, TileSelectionType.Attack));
                    lastHighlightedTiles.Add(tile);
                }
                else if (tile.DistanceTo(_playerManager.Player.Tile.Value) == 1)
                {
                    // move highlight
                    tile.AddSelector(new(_playerManager.Player, TileSelectionType.Move));
                    lastHighlightedTiles.Add(tile);
                }
            }
        }

        public void HideHighlights()
        {
            foreach (Tile tile in lastHighlightedTiles)
            {
                tile.RemoveSelector(_playerManager.Player);
            }
            lastHighlightedTiles.Clear();
        }
    }
}