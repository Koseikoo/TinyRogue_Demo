using System;
using System.Collections.Generic;
using Models;
using UniRx;
using UnityEngine;
using DG.Tweening;

namespace Views
{
    public class ModSmithView : MonoBehaviour
    {
        [SerializeField] private BuyResourceSlotUIView[] sellSlots;
        [SerializeField] private UpgradeModSlotUIView[] upgradeSlots;

        [SerializeField] private Transform CameraTradePoint;
        
        private ModSmith _modSmith;

        private Vector3 _startPosition;
        private Quaternion _startRotation;

        private List<IDisposable> _weaponSlotsDisposable = new();
        private List<IDisposable> _buySlotsDisposable = new();

        private void Start()
        {
            var camTransform = UIHelper.Camera.transform;
            _startPosition = camTransform.localPosition;
            _startRotation = camTransform.rotation;
        }

        public void Initialize(ModSmith modSmith)
        {
            _modSmith = modSmith;

            _modSmith.InTrade.SkipLatestValueOnSubscribe().Subscribe(b =>
            {
                if(b)
                    OnTradeStart();
                else
                    OnTradeEnd();
                
            }).AddTo(this);
            
            HideModsToSell();
            HideWeaponMods();
        }

        private void OnTradeStart()
        {
            MoveCamera(CameraTradePoint.position, CameraTradePoint.rotation);
            UIHelper.Camera.DOFieldOfView(30, .4f);
            
            ShowModsToSell();
            ShowWeaponMods();
        }

        private void OnTradeEnd()
        {
            MoveCameraLocal(_startPosition, _startRotation);
            UIHelper.Camera.DOFieldOfView(60, .4f);
            
            HideModsToSell();
            HideWeaponMods();
        }

        private void MoveCamera(Vector3 position, Quaternion rotation)
        {
            var camTransform = UIHelper.Camera.transform;

            Sequence moveSequence = DOTween.Sequence();

            moveSequence.Insert(0, camTransform.DOMove(position, .4f))
                .Insert(0f, camTransform.DORotateQuaternion(rotation, .4f));
        }

        private void MoveCameraLocal(Vector3 localPosition, Quaternion rotation)
        {
            var camTransform = UIHelper.Camera.transform;

            Sequence moveSequence = DOTween.Sequence();

            moveSequence.Insert(0, camTransform.DOLocalMove(localPosition, .4f))
                .Insert(0f, camTransform.DORotateQuaternion(rotation, .4f));
        }

        private void ShowModsToSell()
        {
            for (int i = 0; i < sellSlots.Length; i++)
            {
                SubscribeToBuyModSlot(i);
                sellSlots[i].gameObject.SetActive(true);
            }
        }

        private void HideModsToSell()
        {
            for (int i = 0; i < sellSlots.Length; i++)
            {
                sellSlots[i].gameObject.SetActive(false);
            }
            
            UnsubscribeFromBuyModSlot();
        }

        private void ShowWeaponMods()
        {
            
            for (int i = 0; i < upgradeSlots.Length; i++)
            {
                upgradeSlots[i].gameObject.SetActive(true);
            }
        }
        
        private void HideWeaponMods()
        {
            for (int i = 0; i < upgradeSlots.Length; i++)
            {
                upgradeSlots[i].gameObject.SetActive(false);
            }
        }

        private void SubscribeToBuyModSlot(int index)
        {
            IDisposable disposable = _modSmith.SellSlots[index].Item.Subscribe(item =>
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