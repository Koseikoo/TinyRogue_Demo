using System.Collections.Generic;
using Container;
using Factories;
using Factory;
using Models;
using TinyRogue;
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
        public Archipel Archipel;

        //public List<Tile> TileCollection => Island == null ? Ship.Tiles : Island.Tiles;
        public List<Tile> TileCollection => _playerManager.Player.Tile.Value.Island.Tiles;
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
                
            StartGame();
        }

        public void SpawnEnemyTestIsland()
        {
            GameStateContainer.LockCameraRotation = false;
            DestroyIsland();
            DestroyShip();
            
            _island = _islandFactory.CreateEnemyTestIsland();
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
            _island.StartTile.MoveUnit(_playerManager.Player);
            _playerManager.Player.EnterIsland.Execute();
                
            StartGame();
        }

        public void SpawnNewArchipel()
        {
            GameStateContainer.LockCameraRotation = false;
            DestroyArchipel();
            
            GameStateContainer.LockCameraRotation = false;
            Archipel = _islandFactory.CreateArchipel();
            Archipel.StartTile.MoveUnit(_playerManager.Player);
            Archipel.EndTile.AddMoveToLogic(unit =>
            {
                if (unit is Player && Archipel.EndIsland.EnemiesOnIsland == 0)
                {
                    SpawnNewArchipel();
                }
            });
            //_playerManager.Player.EnterIsland.Execute();
            
            StartGame();
        }
        
        public void DestroyIsland()
        {
            if(_island == null)
            {
                return;
            }

            _island.DestroyIslandContent();
            _island.IsDestroyed.Value = true;
            _island = null;
        }

        public void DestroyArchipel()
        {
            if (Archipel != null)
            {
                foreach (Island island in Archipel.Islands)
                {
                    island.DestroyIslandContent();
                    island.IsDestroyed.Value = true;
                }

                Archipel = null;
            }
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
            
            _playerManager.Player.EnterShip.Execute(() =>
            {
                StartGame();
            });
            
        }
        
        public void DestroyShip()
        {
            if(_ship == null)
            {
                return;
            }

            _ship.IsDestroyed.Value = true;
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
                if (unit is Player)
                {
                    _ship.BlackSmith.StartTrade();
                }
            });
    
            InteractableDefinition definition = _unitContainer.GetInteractableDefinition(UnitType.HelmInteractable);
            ship.Units.Add(_unitFactory.CreateInteractable(definition, ship.HelmTile));
        }
    
        private void StartGame()
        {
            GameStateContainer.TurnState.Value = TurnState.PlayerTurnStart;
        }
    }
}