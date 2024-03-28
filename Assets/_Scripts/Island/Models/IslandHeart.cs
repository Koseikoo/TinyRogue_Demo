using UniRx;
using UnityEngine;

namespace Models
{
    public class IslandHeart : Unit
    {
        public override void Death()
        {
            base.Death();
            Tile.Value.Island.IsHeartDestroyed.Value = true;
        }
    }
}