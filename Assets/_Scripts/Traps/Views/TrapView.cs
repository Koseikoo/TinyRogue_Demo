using Models;
using UniRx;
using UnityEngine;
using Unit = Models.Unit;

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

        private void OnTrapTrigger(Unit unit)
        {
            unit.Attack(_trap.ModSlots.GetMods(), Vector3.up);
            Debug.Log($"Damage {unit.Type}");
        }

        private void OnIslandDestroyed()
        {
            Destroy(gameObject);
        }
    }
}