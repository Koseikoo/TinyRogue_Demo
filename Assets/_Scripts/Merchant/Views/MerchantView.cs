using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using UniRx;
using UnityEngine;
using DG.Tweening;
using Factories;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class MerchantView : MonoBehaviour
    {
        [SerializeField] private float defaultFOV;
        [SerializeField] private float inTradeFOV;
        [SerializeField] private float fovLerpDuration;
        [SerializeField] private float coinDropInterval;
        [SerializeField] private float coinLerpDuration;
        [SerializeField] private float coinArch;
        [Space]
        [SerializeField] private MerchantSlotUIView[] slotViews;
        [SerializeField] private BuyResourceSlotUIView[] sellSlots;
        [SerializeField] private Transform goldCoinDropPoint;
        [SerializeField] private Transform CameraTradePoint;
        [SerializeField] private GameObject moveSelection;
        [SerializeField] private MoveCameraView _moveCameraView;

        [SerializeField] private Button takeMoneyButton;
        [SerializeField] private GameObject buttonParent;

        private Merchant _merchant;
        private LootFactory _lootFactory;
        private List<IDisposable> _buySlotsDisposable = new();

        public void Initialize(Merchant merchant, LootFactory lootFactory)
        {
            _merchant = merchant;
            _lootFactory = lootFactory;
            for (int i = 0; i < slotViews.Length; i++)
                slotViews[i].Initialize(merchant);
            
            foreach (var slot in sellSlots)
                slot.Initialize(_merchant);
            
            _merchant.InTrade.SkipLatestValueOnSubscribe().Subscribe(b =>
            {
                if(b)
                    OnStartTrade();
                else
                    OnEndTrade();
                
            }).AddTo(this);

            _merchant.CurrentTradeGold
                .Pairwise()
                .Where(pair => pair.Previous < pair.Current)
                .Subscribe(pair => SellingAnimation(pair.Current - pair.Previous))
                .AddTo(this);

            takeMoneyButton.onClick.AddListener(() =>
            {
                buttonParent.SetActive(false);
                WorldLootContainer.ClaimMerchantCoins.Execute();
            });
            
            
            buttonParent.SetActive(false);
            HideShop();
            HideMerchantSlots();
        }

        private void SellingAnimation(int coins)
        {
            Sequence sequence = DOTween.Sequence();

            var view = _lootFactory.CreateLootView(LootType.Item, _merchant.Player.Bag.BagPoint.position);
            sequence.Insert(0, view.transform.DOMove(slotViews[_merchant.LastTradedIndex].WorldPosition, .3f))
                .OnComplete(() =>
                {
                    view.ResetView();
                    StartCoroutine(DropGoldCoins(coins));
                });
        }

        private IEnumerator DropGoldCoins(int coins)
        {
            buttonParent.SetActive(false);
            
            for (int i = 0; i < coins; i++)
            {
                yield return new WaitForSeconds(coinDropInterval);
                _lootFactory.CreateGoldCoin(goldCoinDropPoint.position, true);
            }
            
            buttonParent.SetActive(true);
        }

        private void OnStartTrade()
        {
            UIHelper.Camera.DOFieldOfView(inTradeFOV, fovLerpDuration);
            _moveCameraView.SetCameraTransform(CameraTradePoint);
            moveSelection.SetActive(false);
            
            ShowMerchantSlots();
            ShowShop();
        }

        private void OnEndTrade()
        {
            UIHelper.Camera.DOFieldOfView(defaultFOV, fovLerpDuration);
            _moveCameraView.ResetCameraTransform();
            moveSelection.SetActive(true);

            WorldLootContainer.ClaimMerchantCoins.Execute();
            HideShop();
            HideMerchantSlots();
        }
        
        

        private void ShowMerchantSlots()
        {
            foreach (var view in slotViews)
                view.Show();
        }

        private void HideMerchantSlots()
        {
            foreach (var view in slotViews)
                view.Hide();
        }

        private void Destroy(bool destroy)
        {
            if(destroy)
                Destroy(gameObject);
        }
        
        private void ShowShop()
        {
            for (int i = 0; i < sellSlots.Length; i++)
            {
                SubscribeToBuyModSlot(i);
                sellSlots[i].gameObject.SetActive(true);
            }
        }
        
        private void HideShop()
        {
            for (int i = 0; i < sellSlots.Length; i++)
            {
                sellSlots[i].gameObject.SetActive(false);
            }
            
            UnsubscribeFromBuyModSlot();
        }
        
        private void SubscribeToBuyModSlot(int index)
        {
            IDisposable disposable = _merchant.SellSlots[index].Item.Subscribe(item =>
            {
                if(item == null)
                    sellSlots[index].RenderEmpty();
                else
                    sellSlots[index].Render(item);
                    
            }).AddTo(this);
            _buySlotsDisposable.Add(disposable);
        }

        private void UnsubscribeFromBuyModSlot()
        {
            for (int i = 0; i < _buySlotsDisposable.Count; i++)
            {
                _buySlotsDisposable[i].Dispose();
            }
            
            _buySlotsDisposable.Clear();
        }
    }
}