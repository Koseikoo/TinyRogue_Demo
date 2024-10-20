using Models;
using UniRx;
using UnityEngine;

namespace Views
{
    public class PlayerCompassUIView : MonoBehaviour
    {
        private const float zRotationOffset = 45;
        
        [SerializeField] private RectTransform compassNeedle;

        private Player _player;

        public void Initialize(Player player)
        {
            _player = player;
            _player.AnchorYRotation
                .Where(_ => CanUpdateNeedle)
                .Subscribe(UpdateNeedle).AddTo(this);
            
            _player.Tile
                .Where(_ => CanUpdateNeedle)
                .Subscribe(_ => UpdateNeedle(_player.AnchorYRotation.Value)).AddTo(this);
        }

        private void UpdateNeedle(float yRotation)
        {
            return;
            Vector3 needleDirection = _player.Tile.Value.Island.HeartTile.FlatPosition -
                                      _player.Tile.Value.FlatPosition;
            needleDirection.Normalize();
            Vector3 lookDirection = MathHelper.RotateVector(Vector3.forward, Vector3.up, yRotation);

            Quaternion rot = Quaternion.FromToRotation(needleDirection, lookDirection);
            Vector3 euler = rot.eulerAngles;

            compassNeedle.rotation = Quaternion.AngleAxis(euler.y, Vector3.forward);
            compassNeedle.Rotate(Vector3.forward, zRotationOffset);
        }

        private bool CanUpdateNeedle =>
            _player.Tile.Value != null && GameStateContainer.GameState.Value == GameState.Island;
    }
}