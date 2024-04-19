using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Models;
using UniRx;
using Random = UnityEngine.Random;

namespace Views
{
    public class IslandView : MonoBehaviour
    {
        public Transform TileParent;
        private Island _island;

        [SerializeField] private float dissolveInterval;

        private Coroutine _dissolveRoutine;
        private List<Vector3> dissolvedPoints = new();

        public void Initialize(Island island)
        {
            _island = island;

            _island.IsDestroyed
                .Where(b => b)
                .Subscribe(_ =>
                {
                    if(_dissolveRoutine != null)
                        StopCoroutine(_dissolveRoutine);
                    Destroy(gameObject);
                })
                .AddTo(this);

            _island.DissolveIslandCommand.Subscribe(_ =>
            {
                if (_dissolveRoutine == null)
                    _dissolveRoutine = StartCoroutine(DissolveRoutine());
            }).AddTo(this);
        }

        private IEnumerator DissolveRoutine()
        {
            System.Random rand = new System.Random();
            List<Tile> dissolvePool = new()
            {
                _island.HeartTile
            };

            List<Tile> usedPool = new(dissolvePool);
            List<Tile> nextDissolvePool = new();

            while (dissolvePool.Count > 0)
            {
                foreach (Tile tile in dissolvePool)
                {
                    int max = tile.Neighbours.Count;
                    int min = Mathf.Max(1, max - 3);
                    var next = tile.Neighbours
                        .Where(t => !usedPool.Contains(t) && t != _island.StartTile)
                        .OrderBy(t => rand.Next())
                        .Take(Random.Range(min, max))
                        .ToList();
                    
                    next.ForEach(t => usedPool.Add(t));
                    
                    nextDissolvePool.AddRange(next);
                    dissolvedPoints.Add(tile.WorldPosition);
                    _island.DestroyTile(tile);
                    yield return new WaitForSeconds(dissolveInterval);
                }
                
                dissolvePool = new(nextDissolvePool);
                nextDissolvePool.Clear();
            }
        }

        private void OnDrawGizmos()
        {
            if(dissolvedPoints == null)
                return;

            Gizmos.color = new(1, 0, 0, .2f);
            foreach (Vector3 point in dissolvedPoints)
            {
                Gizmos.DrawSphere(point, .5f);
            }
        }
    }
}