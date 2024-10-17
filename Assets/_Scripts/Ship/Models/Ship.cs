using System.Collections.Generic;
using UniRx;

namespace Models
{
    public class Ship
    {
        public List<Tile> Tiles = new();
        public Tile StartTile;
        public Tile HelmTile;
        public Tile MerchantTile;
        public Tile ModSmithTile;

        public List<GameUnit> Units = new();

        public Merchant Merchant;
        public BlackSmith BlackSmith;
        
        public BoolReactiveProperty IsDestroyed = new();
        public void LinkTiles()
        {
            foreach (Tile tile in Tiles)
                tile.SetShip(this);
        }
    }
}