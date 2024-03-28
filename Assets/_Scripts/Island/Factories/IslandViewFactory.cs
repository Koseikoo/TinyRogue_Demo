using System.Collections.Generic;
using System.Linq;
using Installer;
using UnityEngine;
using Views;
using Models;
using Zenject;

namespace Factories
{
    public class IslandViewFactory
    {
        public const float GrassOffset = 1.385f;
        public const float NegativeTerrainOffset = 1.24f;
        
        [Inject] private TileView _tilePrefab;
        [Inject] private IslandView _islandPrefab;
        
        [Inject] private GrassContainer _grass;
        [Inject] private BoardContainer _board;
        [Inject] private BridgeContainer _bridge;
        [Inject] private TerrainContainer _terrain;

        [Inject] private DiContainer _container;
        public List<TileView> TilePool { get; private set; }

        private TileView _currentTileView;
        private Tile _currentTile;

        public IslandViewFactory()
        {
            TilePool = new();
        }
        
        public IslandView CreateIslandView(Island island)
        {
            IslandView view = _container.InstantiatePrefab(_islandPrefab).GetComponent<IslandView>();
            
            ResetPooledTiles();
            foreach (Tile tile in island.Tiles)
            {
                CreateTileView(tile, view.TileParent)
                    .WithTerrain()
                    .WithGrass()
                    .WithBoard();
            }
            view.Initialize(island);
            return view;
        }

        public IslandViewFactory CreateTileView(Tile tile, Transform parent)
        {
            TileView view = GetPooledTile();
            view.Initialize(tile);
            view.transform.SetParent(parent);
            view.gameObject.SetActive(true);
            _currentTileView = view;
            _currentTile = tile;
            return this;
        }

        public IslandViewFactory WithGrass()
        {
            if (_currentTile.GrassType == GrassType.None)
                return this;
            
            var pre = _grass.GetGrass(_currentTile.GrassType);
            var grass = Object.Instantiate(pre,
                _currentTileView.Visual);
            _currentTileView.AddVisual(grass);
            grass.transform.Rotate(Vector3.up, _currentTile.GrassRotation);
            grass.transform.localPosition = Vector3.up * GrassOffset;
            return this;
        }

        public IslandViewFactory WithTerrain()
        {
            if (_currentTile.TerrainType == TerrainType.None)
                return this;
            GameObject terrain = Object.Instantiate(_terrain.GetTerrain(_currentTile.TerrainType),
                _currentTileView.Visual);
            _currentTileView.AddVisual(terrain);
            
            float widthScale = terrain.transform.localScale.x;
            
            if (_currentTile.TerrainType == TerrainType.Top)
            {
                int randScale = Random.Range(4, 8);
                
                terrain.transform.localScale = new(widthScale, randScale, widthScale);
                terrain.transform.localPosition -= Vector3.up * (NegativeTerrainOffset * (randScale - 1));
            }
            else if (_currentTile.TerrainType == TerrainType.Weak)
            {
                int weakScale = Random.Range(2, 4);
                terrain.transform.localScale = new(widthScale, weakScale, widthScale);
                terrain.transform.localPosition = new(0f, 1.3f, 0f);
            }
            
            return this;
        }

        public IslandViewFactory WithTerrainTest()
        {
            GameObject terrain = Object.Instantiate(_terrain.GetTerrain(TerrainType.Top),
                _currentTileView.Visual);
            _currentTileView.AddVisual(terrain);
            
            int randScale = Random.Range(4, 8);
            terrain.transform.localScale = new(1, randScale, 1);
            terrain.transform.localPosition -= Vector3.up * (NegativeTerrainOffset * (randScale - 1));

            return this;
        }

        public IslandViewFactory WithBoard()
        {
            if (_currentTile.BoardType == BoardType.None)
                return this;

            var board = Object.Instantiate(_board.GetBoard(_currentTile.BoardType),
                _currentTileView.Visual);
            _currentTileView.AddVisual(board);
            board.transform.localPosition = Vector3.up * GrassOffset;
            return this;
        }

        public IslandViewFactory AsBridge()
        {
            if (_currentTile.BridgeType == BridgeType.None)
                return this;
            
            return this;
        }
        
        void ResetPooledTiles()
        {
            for (int i = TilePool.Count - 1; i >= 0; i--)
            {
                if(TilePool[i] == null)
                    TilePool.RemoveAt(i);
            }
            
            foreach (TileView tile in TilePool)
            {
                tile.RemoveVisuals();
                tile.gameObject.SetActive(false);
            }
        }
        
        TileView GetPooledTile()
        {
            var tile = TilePool.FirstOrDefault(t => !t.gameObject.activeSelf);
            if (tile == null)
            {
                tile = _container.InstantiatePrefab(_tilePrefab).GetComponent<TileView>();
                TilePool.Add(tile);
            }
            else
            {
                tile.RemoveVisuals();
            }
            
            return tile;
        }
    }
}