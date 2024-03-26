using System.Collections.Generic;
using Container;
using Factories;
using Factory;
using Models;
using UniRx;
using Views;
using Zenject;

namespace Game
{
    public class GameAreaManager
    {
        [Inject] private IslandFactory _islandFactory;
        [Inject] private ShipFactory _shipFactory;
        [Inject] private SegmentFactory _segmentFactory;
        [Inject] private UnitFactory _unitFactory;
        
        [Inject] private UnitContainer _unitContainer;

        [Inject] private PlayerManager _playerManager;
        [Inject] private PlayerFeedbackManager _playerFeedbackManager;
        
        private Island _island;
        private Ship _ship;

        public Island Island => _island;
        public Ship Ship => _ship;

        public List<Tile> TileCollection => Island == null ? Ship.Tiles : Island.Tiles;
        private int _currentIslandLevel;

        public void SpawnSegmentTestIsland(SegmentView segmentToTest)
        {
            _island = _islandFactory.CreateSegmentTestIsland(segmentToTest);
            _island.StartTile.MoveUnit(_playerManager.Player);
            StartGame();
        }

        public void SpawnNewSegmentTestIsland(SegmentView segmentToTest)
        {
            GameStateContainer.LockCameraRotation = false;
            DestroyIsland();
            DestroyShip();
            
            _island = _islandFactory.CreateSegmentTestIsland(segmentToTest);
            _island.StartTile.MoveUnit(_playerManager.Player);
            _playerManager.Player.Weapon.Tile.Value = _island.StartTile;
                
            StartGame();
        }
        
        public void SpawnNewIsland()
        {
            GameStateContainer.LockCameraRotation = false;
            DestroyIsland();
            DestroyShip();
            
            _island = _islandFactory.CreateIsland(_currentIslandLevel);
            _currentIslandLevel++;
            _island.StartTile.MoveUnit(_playerManager.Player);
            _playerManager.Player.Weapon.Tile.Value = _island.StartTile;
                
            StartGame();
        }
        
        public void DestroyIsland()
        {
            if(_island == null)
                return;
    
            _island.DestroyIslandContent();
            _island.IsDestroyed.Value = true;
            _playerFeedbackManager.TryDestroyEnemyInfoModal();
            _island = null;
        }
        
        // Ship
        
        public void SpawnNewShip()
        {
            GameStateContainer.LockCameraRotation = true;
            _currentIslandLevel = 0;
            DestroyIsland();
            DestroyShip();
            
            _ship = _shipFactory.CreateShip();
            LinkShipTileActions(_ship);
            _ship.StartTile.MoveUnit(_playerManager.Player);
            _playerManager.Player.Weapon.Tile.Value = _ship.StartTile;
            
            StartGame();
        }
        
        public void DestroyShip()
        {
            if(_ship == null)
                return;
    
            _ship.IsDestroyed.Value = true;
            _playerFeedbackManager.TryDestroyEnemyInfoModal();
            _ship = null;
        }
        
        private void LinkShipTileActions(Ship ship)
        {
            ship.MerchantTile.AddMoveToLogic(unit =>
            {
                if (unit is Player player)
                    _ship.Merchant.StartTrade(player);
            });
            
            ship.ModSmithTile.AddMoveToLogic(unit =>
            {
                if (unit is Player)
                    _ship.BlackSmith.StartTrade();
            });
    
            var definition = _unitContainer.GetInteractableDefinition(UnitType.HelmInteractable);
            ship.Units.Add(_unitFactory.CreateInteractable(definition, ship.HelmTile));
        }
    
        private void StartGame()
        {
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnStart;
        }
    }
}