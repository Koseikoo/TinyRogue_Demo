using System;
using Models;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    [RequireComponent(typeof(UITransformPositioner))]
    public class MerchantSlotUIView : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;

        private UITransformPositioner _transformPositioner; 
        private Merchant _merchant;

        public Vector3 WorldPosition => _transformPositioner.WorldPosition;

        private void Awake()
        {
            _transformPositioner = GetComponent<UITransformPositioner>();
        }

        public void Initialize(Merchant merchant)
        {
            _merchant = merchant;
        }

        public void SellSelected()
        {
            _merchant.AddSelectedItemToTrade(transform.GetSiblingIndex());
        }

        public void Show()
        {
            itemIcon.enabled = true;
        }

        public void Hide()
        {
            itemIcon.enabled = false;
        }
    }
}