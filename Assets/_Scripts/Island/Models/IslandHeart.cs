using UniRx;
using UnityEngine;

namespace Models
{
    public class IslandHeart : GameUnit
    {
        public override void Death()
        {
            base.Death();
            Tile.Value.Island.IsHeartDestroyed.Value = true;
        }
    }
}