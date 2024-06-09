using UnityEngine;

namespace TinyRogue
{
    public class AnimationEventHandler : MonoBehaviour
    {
        // Triggered by Animation Event
        public void AttackEvent()
        {
            Debug.Log("Attack");
            GameStateContainer.Player.Attack();
        }
    }
}