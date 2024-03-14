using UnityEngine;

namespace Models
{
    public class TileVisualProperties
    {
        // Grass
        public bool HasGrass;
        public float GrassRotation;
        public GameObject GrassPrefab;
        
        // Board
        public bool HasBoard;
        public GameObject BoardPrefab;
        
        //Bridge
        public bool IsBridge;
        public GameObject BridgePrefab;

        public void SetGrass(GameObject prefab, float rotation)
        {
            HasGrass = true;
            GrassRotation = rotation;
            GrassPrefab = prefab;
        }

        public void SetBoard(GameObject prefab)
        {
            HasBoard = true;
            BoardPrefab = prefab;
        }

        public void SetBridge(GameObject prefab)
        {
            IsBridge = true;
            BridgePrefab = prefab;
        }

        public void Reset()
        {
            HasGrass = false;
            GrassRotation = default;
            GrassPrefab = null;

            HasBoard = false;
            BoardPrefab = null;

            IsBridge = false;
            BridgePrefab = null;
        }
    }
}