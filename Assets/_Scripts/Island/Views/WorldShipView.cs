using Models;
using UniRx;
using UnityEngine;

namespace Views
{
    public class WorldShipView : MonoBehaviour
    {
        public void Initialize(Island island)
        {
            island.IsDestroyed.Where(b => b).Subscribe(_ => Destroy(gameObject)).AddTo(this);
        }
    }
}