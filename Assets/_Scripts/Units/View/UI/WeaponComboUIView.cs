using System;
using Models;
using TMPro;
using UniRx;
using UnityEngine;

namespace Views
{
    public class WeaponComboUIView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _comboText;
        [SerializeField] private Animator _animator; 
        [SerializeField] private float _minComboScale; 
        [SerializeField] private float _maxComboScale; 
        
        private Player _player;
        
        private static readonly int Popup = Animator.StringToHash("Popup");

        public void Initialize(Player player)
        {
            _player = player;

            _player.Weapon.ActiveCombo.ObserveAdd().Subscribe(_ => RenderNewCombo(_player.Weapon.ActiveCombo.Count)).AddTo(this);
            _player.Weapon.ActiveCombo.ObserveReset().Subscribe(_ => HideCombo()).AddTo(this);
        }

        private void RenderNewCombo(int combo)
        {
            _animator.SetTrigger(Popup);
            _comboText.text = $"x{combo}";
            transform.localScale =
                Vector3.one * Mathf.Lerp(_minComboScale, _maxComboScale, (float)(combo - 1) / Weapon.MaxCombo);
            _comboText.enabled = true;
        }

        private void HideCombo()
        {
            _comboText.enabled = false;
        }

        private void LateUpdate()
        {
            if (_player.Weapon.ActiveCombo.Count == 0)
                return;
            var followTarget = _player.Weapon.ActiveCombo[^1];
            transform.position = UIHelper.Camera.WorldToScreenPoint(followTarget.Tile.Value.WorldPosition);
        }
    }
}