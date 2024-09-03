using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Scripts.Island
{
    public class ShowTileDetails : MonoBehaviour
    {
        private System.Random _random = new();
        [SerializeField] private Transform grassTransform;
        [SerializeField] private Vector2Int detailRange;
        [SerializeField] private List<GameObject> potentialDetails;

        private void Awake()
        {
            int detailAmount = _random.Next(detailRange.x, detailRange.y + 1);

            while (potentialDetails.Count > detailAmount)
            {
                GameObject detailToDestroy = potentialDetails.Random();
                potentialDetails.Remove(detailToDestroy);
                Destroy(detailToDestroy);
            }
            
            grassTransform.Rotate(_random.Next(0, 361) * Vector3.up);
        }
    }
}