using Models;
using UniRx;
using UnityEngine;

namespace Views
{
    public class TrapView : MonoBehaviour
    {
        private Trap _trap;
        private Tile _tile;

        [SerializeField] private Transform visual;
        public void Initialize(Trap trap, Tile tile)
        {
            _trap = trap;
            _tile = tile;
            
            _tile.AddMoveToLogic(OnTrapTrigger);

            Vector3 position = tile.FlatPosition;
            Vector3 offset = tile.BoardType != BoardType.None ? Vector3.up * Tile.BoardOffset : default;

            tile.Island.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => OnIslandDestroyed())
                .AddTo(this);
            
            transform.position = position;
            visual.localPosition = offset;
        }

        private void OnTrapTrigger(GameUnit gameUnit)
        {
            gameUnit.Attack(_trap.ModSlots.GetMods(), Vector3.up);
            Debug.Log($"Damage {gameUnit.Type}");
        }

        private void OnIslandDestroyed()
        {
            Destroy(gameObject);
        }
    }
}