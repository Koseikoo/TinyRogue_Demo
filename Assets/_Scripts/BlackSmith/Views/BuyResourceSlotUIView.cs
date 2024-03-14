using Installer;
using Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class BuyResourceSlotUIView : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private GameObject priceParent;
        [SerializeField] private TextMeshProUGUI priceText;

        private UITransformPositioner _transformPositioner; 
        private Merchant _merchant;
        private int _slotIndex;

        public Vector3 WorldPosition => _transformPositioner.WorldPosition;
        
        [Inject] private ItemIconContainer _itemIconContainer;

        private void Awake()
        {
            _transformPositioner = GetComponent<UITransformPositioner>();
            _slotIndex = transform.GetSiblingIndex();
        }

        public void Initialize(Merchant merchant)
        {
            _merchant = merchant;
        }

        public void BuyResource()
        {
            Slot slot = _merchant.SellSlots[_slotIndex];

            Bag playerBag = _merchant.Player.Bag;
            int itemValue = slot.Item.Value.Type.GetValue();
            
            if (playerBag.HasGold(itemValue))
            {

                if (slot.Item.Value is Resource resource)
                {
                    playerBag.RemoveGold(itemValue);
                    _merchant.Player.Bag.AddLoot(new(0, null, null, new()
                    {
                        resource
                    }));
                }
                else
                {
                    playerBag.RemoveGold(itemValue);
                    _merchant.Player.Bag.AddLoot(new(0, null, new()
                    {
                        slot.Item.Value
                    }));
                }
                
            }
            else
            {
                Debug.Log("Insufficient Funds");
            }
        }

        public void Render(Item item)
        {
            itemIcon.enabled = true;
            itemIcon.sprite = _itemIconContainer.GetItemIcon(item.Type);

            priceText.text = item.Type.GetValue().ToString();
            priceParent.SetActive(true);
        }

        public void RenderEmpty()
        {
            itemIcon.enabled = false;
            priceParent.SetActive(false);
        }
    }
}