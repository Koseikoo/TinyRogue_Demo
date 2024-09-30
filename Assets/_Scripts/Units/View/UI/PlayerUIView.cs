using System;
using Models;
using UnityEngine;
using DG.Tweening;
using Factory;
using UniRx;
using Zenject;

namespace Views
{
    public class PlayerUIView : MonoBehaviour
    {
        [Inject]
        private ModalFactory _modalFactory;
        
        [SerializeField] private UnitHealthUIView _healthUIView;
        [SerializeField] private BagUIView _bagUIView;
        [SerializeField] private PlayerCompassUIView _compassUIView;
        [SerializeField] private LootHistoryView _lootHistoryView;
        [SerializeField] private SkillUIView _skillUIView;
        
        [SerializeField] private RectTransform navBar;

        private Player _player;

        private float _yOffset;

        private void Awake()
        {
            //Vector2 anchoredPosition = navBar.anchoredPosition;
            //_yOffset = navBar.sizeDelta.y;
        }

        public void Initialize(Player player)
        {
            _player = player;
            _healthUIView?.Initialize(player);
            _bagUIView?.Initialize(player.Bag);
            _compassUIView?.Initialize(player);
            _skillUIView?.Initialize(player);
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