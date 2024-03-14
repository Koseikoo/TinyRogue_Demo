using UnityEngine;
using Models;
using UniRx;

namespace Views
{
    public class IslandView : MonoBehaviour
    {
        public Transform TileParent;
        private Island _island;

        public void Initialize(Island island)
        {
            _island = island;

            _island.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(this);
        }
    }
}