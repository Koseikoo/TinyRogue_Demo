using System;
using Models;
using UnityEngine;
using DG.Tweening;
using UniRx;

namespace Views
{
    public class PlayerUIView : MonoBehaviour
    {
        [SerializeField] private UnitHealthUIView _healthUIView;
        [SerializeField] private WeaponChargesUIView _attackChargesUIView;
        [SerializeField] private BagUIView _bagUIView;
        [SerializeField] private PlayerCompassUIView _compassUIView;
        [SerializeField] private WeaponUIView _weaponUIView;
        [SerializeField] private WeaponComboUIView _weaponComboUIView;
        [SerializeField] private LevelUpUIView _levelUpUIView;
        [SerializeField] private LootHistoryView _lootHistoryView;
        
        [SerializeField] private RectTransform navBar;

        private float _yOffset;

        private void Awake()
        {
            //Vector2 anchoredPosition = navBar.anchoredPosition;
            //_yOffset = navBar.sizeDelta.y;
        }

        public void Initialize(Player player)
        {
            _healthUIView?.Initialize(player);
            _attackChargesUIView?.Initialize(player.Weapon);
            _bagUIView?.Initialize(player.Bag);
            _weaponUIView?.Initialize(player.Weapon);
            _compassUIView?.Initialize(player);
            _weaponComboUIView?.Initialize(player);
            _levelUpUIView?.Initialize(player);
            //_lootHistoryView.Initialize(player);

            player.IsDestroyed
                .Where(b => b)
                .Subscribe(_ => Destroy(gameObject))
                .AddTo(this);

            GameStateContainer.CloseOpenUIElements.Subscribe(_ => HideNavButton()).AddTo(this);
        }

        public void OnNavButtonPressed()
        {
            //float y = navBar.anchoredPosition.y > 0 ? 0 : _yOffset;
            //navBar.DOAnchorPos(new Vector2(0, y), .2f);
        }

        private void HideNavButton()
        {
            //navBar.DOAnchorPos(new Vector2(0, 0), .2f);
        }
    }
}