using System;
using Installer;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;
using UnityEngine.Serialization;

namespace Views
{
    public class SlotUIView : MonoBehaviour
    {
        [SerializeField] private SlotUIRenderer slotRenderer;
        [SerializeField] private Image lockedVisual;
        
        private Slot _slot;

        private IDisposable _selectionSubscription;
        private IDisposable _stackSubscription;
        private IDisposable _powerSubscription;

        public void Initialize(Slot slot)
        {
            _slot = slot;
            _slot.IsLocked.Subscribe(isLocked => lockedVisual.enabled = isLocked).AddTo(this);
            _slot.Item.Subscribe(_ => UpdateVisuals(_slot)).AddTo(this);
        }

        public void TapSlot()
        {
            _slot.TapSlot();
        }

        private void UpdateVisuals(Slot slot)
        {
            Render(slot.Item.Value);
            UpdateSubscriptions(slot);
        }

        private void Render(Item item)
        {
            if (item == null)
            {
                slotRenderer.RenderEmpty();
                return;
            }
            
            if (item is Mod mod)
                slotRenderer.RenderMod(mod.Type, mod.Power.Value);
            else
                slotRenderer.RenderItem(item.Type, item.Stack.Value);
        }
        
        private void UpdateSubscriptions(Slot slot)
        {
            _selectionSubscription?.Dispose();
            _stackSubscription?.Dispose();
            _powerSubscription?.Dispose();

            Item item = slot.Item.Value;
            
            if (item != null)
            {
                _selectionSubscription = slot.IsSelected.Subscribe(slotRenderer.RenderSelection).AddTo(this);

                if (item is Mod mod)
                {
                    _powerSubscription = mod.Power.Subscribe(_ => Render(mod)).AddTo(this);
                }
                else
                {
                    _stackSubscription = item.Stack.Subscribe(_ => Render(item));
                }
            }
        }
    }
}