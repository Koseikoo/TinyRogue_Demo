using System;
using DG.Tweening;
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
        [SerializeField] float ComboDropSpeed = .06f;
        
        private Player _player;
        
        private static readonly int Popup = Animator.StringToHash("ComboUp");

        public void Initialize(Player player)
        {
            _player = player;

            _player.Weapon.ActiveCombo.ObserveAdd().Subscribe(_ => RenderNewCombo(_player.Weapon.ActiveCombo.Count)).AddTo(this);
            _player.Weapon.ActiveCombo.ObserveRemove().Subscribe(_ => RenderNewCombo(_player.Weapon.ActiveCombo.Count)).AddTo(this);
            _player.Weapon.ActiveCombo.ObserveReset().Subscribe(_ => HideCombo()).AddTo(this);

            _player.Weapon.DropComboLootCommand
                .Where(_ => _player.Weapon.ActiveCombo.Count > 0)
                .Subscribe(_ => DropComboLootAnimation())
                .AddTo(this);
            
            HideCombo();
        }

        private void RenderNewCombo(int combo)
        {
            _animator.SetTrigger(Popup);
            _comboText.text = $"x{combo}";
            transform.localScale =
                Vector3.one * Mathf.Lerp(_minComboScale, _maxComboScale, (float)(combo - 1) / Weapon.MaxCombo);
            _comboText.enabled = true;
            
            if(_player.Weapon.ActiveCombo.Count == 0)
                HideCombo();
        }

        private void HideCombo()
        {
            _comboText.enabled = false;
        }

        private void DropComboLootAnimation()
        {
            Sequence sequence = DOTween.Sequence();
            float delay = 0;

            RectTransform rect = GetComponent<RectTransform>();
            Vector3 dropPosition = rect.ScreenToWorldPoint();

            for (int i = _player.Weapon.ActiveCombo.Count - 1; i >= 0; i--)
            {
                int index = i;
                sequence.InsertCallback(delay, () =>
                {
                    _player.Weapon.DropComboLoot(dropPosition);
                });

                delay += ComboDropSpeed;
            }
        }

        private void LateUpdate()
        {
            //if (_player.Weapon.ActiveCombo.Count == 0)
            //    return;
            //var followTarget = _player.Weapon.ActiveCombo[^1];
            //transform.position = UIHelper.Camera.WorldToScreenPoint(followTarget.Tile.Value.WorldPosition);
        }
    }
}