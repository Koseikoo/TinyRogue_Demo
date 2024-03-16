using System.Collections.Generic;
using Container;
using Factories;
using Factory;
using Models;
using UniRx;
using Zenject;

namespace Game
{
    public class GameAreaManager
    {
        [Inject] private IslandFactory _islandFactory;
        [Inject] private ShipFactory _shipFactory;
        [Inject] private SegmentFactory _segmentFactory;
        [Inject] private UnitFactory _unitFactory;
        
        [Inject] private InteractableDefinitionContainer _interactableDefinitionContainer;

        [Inject] private PlayerManager _playerManager;
        [Inject] private PlayerFeedbackManager _playerFeedbackManager;
        
        private Island _island;
        private Ship _ship;

        public Island Island => _island;
        public Ship Ship => _ship;

        public List<Tile> TileCollection => Island == null ? Ship.Tiles : Island.Tiles;
        private int _currentIslandLevel;
        private void SpawnIslandSections(Island island)
        {
            foreach (var segment in island.Segments)
            {
                _segmentFactory.CreateSegmentView(segment);
            }
        }

        public void SpawnTestIsland(float size)
        {
            _island = _islandFactory.CreateTestIsland(size);
            _island.StartTile.MoveUnit(_playerManager.Player);
            StartGame();
        }
        
        public void SpawnNewIsland()
        {
            GameStateContainer.LockCameraRotation = false;
            DestroyIsland();
            DestroyShip();
            
            _island = _islandFactory.CreateIsland(_currentIslandLevel);
            _currentIslandLevel++;
            SpawnIslandSections(_island);
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
                {
                    _ship.Merchant.StartTrade(player);
                }
            });
            
            ship.ModSmithTile.AddMoveToLogic(unit =>
            {
                if (unit is Player player)
                {
                    _ship.BlackSmith.StartTrade();
                }
            });
    
            var definition = _interactableDefinitionContainer.GetInteractableDefinition(UnitType.HelmInteractable);
            ship.Units.Add(_unitFactory.CreateInteractable(definition, ship.HelmTile, _playerManager.Player));
        }
    
        private void StartGame()
        {
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnStart;
        }
    }
}