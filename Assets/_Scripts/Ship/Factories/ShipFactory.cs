using Models;
using UnityEngine;
using Views;
using Zenject;

namespace Factories
{
    public class ShipFactory
    {
        [Inject] private ShipView _shipPrefab;
        [Inject] private DiContainer _container;
        
        public Ship CreateShip()
        {
            ShipView view = CreateShipView();
            Ship ship = view.ShipDefinition.GetShipInstance();
            ship.LinkTiles();
            
            ship.Merchant = new();
            ship.BlackSmith = new();
            
            view.Initialize(ship);
            
            foreach (Tile tile in ship.Tiles)
            {
                var tiles = ship.Tiles.GetTilesWithinDistance(tile, 1);
                tiles.Remove(tile);
                tile.Neighbours.AddRange(tiles);
            }
            return ship;
        }

        private ShipView CreateShipView()
        {
            var view = _container.InstantiatePrefab(_shipPrefab).GetComponent<ShipView>();
            return view;
        }
    }
}