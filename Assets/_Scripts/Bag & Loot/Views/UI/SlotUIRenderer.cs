using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Installer;
using Models;
using Zenject;

namespace Views
{
    public class SlotUIRenderer : MonoBehaviour
    {
        public bool Rendering { get; private set; }
        
        [SerializeField] private Image background;
        [SerializeField] private Image icon;
        [SerializeField] private Image selectionBorder;
        [SerializeField] private GameObject amountParent;
        [SerializeField] private TextMeshProUGUI amount;
        [Header("Mods")]
        [SerializeField] private GameObject modUIParent;
        [SerializeField] private TextMeshProUGUI modPower;

        [Inject] private ItemIconContainer _itemIconContainer;

        public void RenderItem(ItemType type, int displayAmount)
        {
            Rendering = true;
            icon.sprite = _itemIconContainer.GetItemIcon(type);
            amountParent.SetActive(true);
            amount.text = displayAmount.ToString();
            modUIParent.SetActive(false);
            EnableSlot();
        }

        public void RenderMod(ItemType type, int power)
        {
            icon.sprite = _itemIconContainer.GetItemIcon(type);
            modPower.text = power.ToString();
            modUIParent.SetActive(true);
            EnableSlot();
            amountParent.SetActive(false);
        }

        public void RenderEmpty()
        {
            Rendering = false;
            if(background != null)
                background.enabled = true;
            icon.enabled = false;
            icon.sprite = null;
            modUIParent.SetActive(false);
            amountParent.SetActive(false);
            
        }

        public void RenderSelection(bool isSelected)
        {
            float val = isSelected ? 1 : 0;
            selectionBorder.DOFade(val, .2f);
        }

        public void DisableSlot()
        {
            Rendering = false;
            if(background != null)
                background.enabled = false;
            icon.enabled = false;
            
            modUIParent.SetActive(false);
            amountParent.SetActive(false);
        }
        
        private void EnableSlot()
        {
            if(background != null)
                background.enabled = true;
            icon.enabled = true;
            amountParent.SetActive(true);
        }
    }
}