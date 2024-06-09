using System;
using System.Collections.Generic;
using DG.Tweening;
using Factory;
using Models;
using UnityEngine;
using UniRx;
using UnityEngine.Serialization;

namespace Views
{
    public class BlackSmithView : MonoBehaviour
    {
        [SerializeField] private GameObject anvilUI;
        [SerializeField] private Transform CameraTradePoint;
        [SerializeField] private GameObject moveSelection;
        [SerializeField] private MoveCameraView _moveCameraView;

        private BlackSmith _blackSmith;
        
        private ModalFactory _modalFactory;
        private CraftModalUIView _currentCraftModal;

        public void Initialize(BlackSmith blackSmith, ModalFactory modalFactory)
        {
            _blackSmith = blackSmith;
            _modalFactory = modalFactory;
            
            //foreach (var slot in upgradeSlots)
            //    slot.Initialize(_blackSmith);
            
            GameStateContainer.CloseOpenUIElements.Subscribe(_ => CloseCraftingUI()).AddTo(this);
            
            _blackSmith.InTrade.SkipLatestValueOnSubscribe().Subscribe(b =>
            {
                if(b)
                {
                    OnTradeStart();
                }
                else
                {
                    OnTradeEnd();
                }
            }).AddTo(this);
        }
        
        private void OnTradeStart()
        {
            _moveCameraView.SetCameraTransform(CameraTradePoint);
            UIHelper.Camera.DOFieldOfView(30, .4f);
            moveSelection.SetActive(false);
            
            ShowAnvilUI();
            //ShowWeaponMods();
        }

        private void OnTradeEnd()
        {
            _moveCameraView.ResetCameraTransform();
            UIHelper.Camera.DOFieldOfView(60, .4f);
            moveSelection.SetActive(true);
            
            HideAnvilUI();
            //HideWeaponMods();
        }

        private void ShowAnvilUI()
        {
            anvilUI.SetActive(true);
        }
        
        private void HideAnvilUI()
        {
            anvilUI.SetActive(false);
        }

        public void OpenCraftingUI()
        {
            if(_currentCraftModal != null)
            {
                return;
            }

            _currentCraftModal = _modalFactory.CreateCraftModal(_blackSmith);
            GameStateContainer.OpenUIElements.Add(_currentCraftModal.gameObject);
        }

        public void CloseCraftingUI()
        {
            if(_currentCraftModal == null)
            {
                return;
            }

            GameStateContainer.OpenUIElements.Remove(_currentCraftModal.gameObject);
            _currentCraftModal.DestroyModal();
        }
    }
}