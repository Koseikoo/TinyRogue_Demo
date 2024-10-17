using Models;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;

namespace Views
{
    public class BagUIView : MonoBehaviour
    {
        [SerializeField] private GameObject bagUI;
        [SerializeField] private GameObject modSlotParent;
        [SerializeField] private GameObject equipmentSlotParent;
        [SerializeField] private GameObject itemSlotParent;
        [SerializeField] private GameObject resourceSlotParent;
        
        [Header("20 Slots Each")]
        [SerializeField] private SlotUIView[] itemSlotViews;
        [SerializeField] private SlotUIView[] equipmentSlotViews;
        [SerializeField] private SlotUIView[] modSlotViews;
        [SerializeField] private SlotUIView[] resourceSlotViews;
        [SerializeField] private TextMeshProUGUI goldText;
        
        private Bag _bag;
        
        public void Initialize(Bag bag)
        {
            _bag = bag;

            for (int i = 0; i < itemSlotViews.Length; i++)
            {
                itemSlotViews[i].Initialize(_bag.Items[i]);
            }
            
            for (int i = 0; i < equipmentSlotViews.Length; i++)
            {
                equipmentSlotViews[i].Initialize(_bag.Equipment[i]);
            }
            
            for (int i = 0; i < modSlotViews.Length; i++)
            {
                modSlotViews[i].Initialize(_bag.Mods[i]);
            }
            
            for (int i = 0; i < resourceSlotViews.Length; i++)
            {
                resourceSlotViews[i].Initialize(_bag.Resources[i]);
            }
            
            _bag.ShowUI.Subscribe(b =>
            {
                if(b)
                {
                    ShowBagUI();
                }
                else
                {
                    HideBagUI();
                }
            }).AddTo(this);

            _bag.OnBagItemsChanged.Subscribe(_ => UpdateGold()).AddTo(this);
            GameStateContainer.CloseOpenUIElements.Subscribe(_ => HideBagUI()).AddTo(this);
        }

        public void ShowMods()
        {
            ResetSelectedSlot();
            modSlotParent.SetActive(true);
            equipmentSlotParent.SetActive(false);
            itemSlotParent.SetActive(false);
            resourceSlotParent.SetActive(false);
        }
        
        public void ShowEquipment()
        {
            ResetSelectedSlot();
            modSlotParent.SetActive(false);
            equipmentSlotParent.SetActive(true);
            itemSlotParent.SetActive(false);
            resourceSlotParent.SetActive(false);
        }

        public void ShowItems()
        {
            ResetSelectedSlot();
            modSlotParent.SetActive(false);
            equipmentSlotParent.SetActive(false);
            itemSlotParent.SetActive(true);
            resourceSlotParent.SetActive(false);
        }
        
        public void ShowResources()
        {
            ResetSelectedSlot();
            modSlotParent.SetActive(false);
            equipmentSlotParent.SetActive(false);
            itemSlotParent.SetActive(false);
            resourceSlotParent.SetActive(true);
        }

        public void ShowBagUI()
        {
            if (bagUI.activeSelf)
            {
                HideBagUI();
                return;
            }
            
            UpdateGold();
            bagUI.SetActive(true);
            GameStateContainer.OpenUIElements.Add(gameObject);
            ShowItems();
        }

        public void HideBagUI()
        {
            bagUI.SetActive(false);
            GameStateContainer.OpenUIElements.Remove(gameObject);

            if (GameStateContainer.SelectedSlot != null)
            {
                GameStateContainer.SelectedSlot.IsSelected.Value = false;
                GameStateContainer.SelectedSlot = null;
            }
        }

        private void UpdateGold()
        {
            goldText.text = _bag.GetSummedItemValue().ToString("N0");
        }

        private void ResetSelectedSlot()
        {
            if(GameStateContainer.SelectedSlot == null)
            {
                return;
            }

            GameStateContainer.SelectedSlot.IsSelected.Value = false;
            GameStateContainer.SelectedSlot = null;
        }
    }
}