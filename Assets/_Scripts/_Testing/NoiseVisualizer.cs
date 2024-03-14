using System;
using DG.Tweening;
using UnityEngine;

namespace _Testing
{
    public class NoiseVisualizer : MonoBehaviour
    {
        [SerializeField] private LODGroup[] tiles;

        [SerializeField] private bool updateNoise;
        [SerializeField] private float scale;
        [SerializeField] private float scale2;
        [SerializeField] private float step;
        [SerializeField] private Vector2 offset;
        [SerializeField] private float noiseOffset;
        [SerializeField] private Color grassColor;
        [SerializeField] private Color groundColor;

        private void Update()
        {
            UpdateNoise();
        }

        private void UpdateNoise()
        {
            for (int i = 0; i < tiles.Length; i++)
            {
                var pos = tiles[i].transform.position;
                var noiseVal = Mathf.PerlinNoise((pos.x * scale) + offset.x, (pos.z * scale) + offset.y);
                var noiseVal2 = Mathf.PerlinNoise((pos.x * scale2) + offset.x, (pos.z * scale2) + offset.y);
                noiseVal = (noiseVal + noiseVal2) / 2;
                //noiseVal = noiseVal > step ? 1 : 0;
                //var color = noiseVal > .5f ? groundColor : grassColor;
                var lods = tiles[i].GetLODs();
                foreach (var lod in lods)
                {
                    lod.renderers[0].material.DOColor(Color.Lerp(groundColor, grassColor, noiseVal + noiseOffset), .1f);
                }
            }
        }
    }
}