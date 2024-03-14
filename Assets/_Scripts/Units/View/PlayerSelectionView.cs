using System.Collections.Generic;
using Models;
using UnityEngine;

namespace Views
{
    public class PlayerSelectionView : MonoBehaviour
    {
        private List<Tile> _selectedTiles = new();

        private Player _player;

        public void Initialize(Player player)
        {
            _player = player;
        }
        
        public void SelectTiles(List<Tile> tiles)
        {
            ReleaseSelection();
            
            foreach (Tile tile in tiles)
            {
                TileSelection selection = new TileSelection(_player,
                    tile.HasUnit ? TileSelectionType.Attack : TileSelectionType.Move);
                tile.AddSelector(selection);
                
                if (!_selectedTiles.Contains(tile))
                {
                    _selectedTiles.Add(tile);
                }
            }
        }

        public void ReleaseSelection()
        {
            foreach (Tile tile in _selectedTiles)
            {
                tile.RemoveSelector(_player);
            }
            _selectedTiles.Clear();
        }
    }
}