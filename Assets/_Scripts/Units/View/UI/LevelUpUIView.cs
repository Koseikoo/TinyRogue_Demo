using Models;
using TMPro;
using UniRx;
using UnityEngine;

namespace Views
{
    public class LevelUpUIView : MonoBehaviour
    {
        private static readonly int Popup = Animator.StringToHash("LevelUp");
        
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private Animator _animator;

        private Player _player;
        public void Initialize(Player player)
        {
            _player = player;
            player.Weapon.Level
                .SkipLatestValueOnSubscribe()
                .Subscribe(_ => RenderLevelUp())
                .AddTo(this);
        }
        
        private void RenderLevelUp()
        {
            _animator.SetTrigger(Popup);
            _comboText.text = $"Level Up!";
            transform.localScale = Vector3.one;
            _comboText.enabled = true;
        }

        private void HideCombo()
        {
            _comboText.enabled = false;
        }

        private void LateUpdate()
        {
            if (_player == null)
                return;
            transform.position = UIHelper.Camera.WorldToScreenPoint(_player.Tile.Value.FlatPosition);
        }
    }
}