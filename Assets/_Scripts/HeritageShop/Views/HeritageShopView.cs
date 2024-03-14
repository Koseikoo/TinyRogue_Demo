using Models;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class HeritageShopView : MonoBehaviour
    {
        [SerializeField] private Button shopButton;
        [SerializeField] private GameObject shopParent;
        
        private HeritageShop _shop;
        
        public void Initialize(HeritageShop shop)
        {
            _shop = shop;
        }

        public void OpenShop()
        {
            shopParent.SetActive(true);
            GameStateContainer.OpenUIElements.Add(gameObject);
        }

        public void CloseShop()
        {
            shopParent.SetActive(false);
            GameStateContainer.OpenUIElements.Remove(gameObject);
        }
    }
}