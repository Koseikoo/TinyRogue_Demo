using System.Collections.Generic;
using Container;
using Models;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Views
{
    public class CraftModalUIView : MonoBehaviour
    {
        [SerializeField] private RectTransform modNav;
        [SerializeField] private RectTransform armorNav;
        [SerializeField] private RectTransform upgradeNav;
        
        [SerializeField] private GameObject modParent;
        [SerializeField] private GameObject armorParent;
        [SerializeField] private GameObject upgradeParent;

        [SerializeField] private RecipeUIView[] modRecipeRenderer;
        [SerializeField] private RecipeUIView[] equipmentRecipeRenderer;
        [SerializeField] private UpgradeModSlotUIView[] upgradeSlots;

        private BlackSmith _blackSmith;

        public void Initialize(BlackSmith blackSmith)
        {
            _blackSmith = blackSmith;
            
            InitializeRecipes();
            ShowMods();
            
            foreach (var slot in upgradeSlots)
                slot.Initialize(_blackSmith);
        }

        public void ShowMods()
        {
            modParent.SetActive(true);
            armorParent.SetActive(false);
            upgradeParent.SetActive(false);
            UpdateNavScale(modNav);
        }
        
        public void ShowArmor()
        {
            modParent.SetActive(false);
            armorParent.SetActive(true);
            upgradeParent.SetActive(false);
            UpdateNavScale(armorNav);
        }

        public void ShowModUpgrade()
        {
            modParent.SetActive(false);
            armorParent.SetActive(false);
            upgradeParent.SetActive(true);
            UpdateNavScale(upgradeNav);
        }

        public void DestroyModal()
        {
            GameStateContainer.OpenUIElements.Remove(gameObject);
            Destroy(gameObject);
        }

        private void InitializeRecipes()
        {
            InitializeMods();
            InitializeEquipment();
        }

        private void InitializeMods()
        {
            var modRecipes = GetModRecipes();
            for (int i = 0; i < modRecipeRenderer.Length; i++)
            {
                if(i == modRecipes.Length)
                    modRecipeRenderer[i].RenderPreview();
                else
                    modRecipeRenderer[i].Initialize(i > modRecipes.Length ? null : modRecipes[i]);
            }
        }

        private void InitializeEquipment()
        {
            var equipmentRecipes = GetEquipmentRecipes();
            for (int i = 0; i < equipmentRecipeRenderer.Length; i++)
            {
                if(i == equipmentRecipes.Length)
                    equipmentRecipeRenderer[i].RenderPreview();
                else
                    equipmentRecipeRenderer[i].Initialize(i > equipmentRecipes.Length ? null : equipmentRecipes[i]);
            }
        }

        private void UpdateNavScale(RectTransform nav)
        {
            modNav.sizeDelta = new Vector2(modNav.sizeDelta.x, nav == modNav ? 150 : 100);
            armorNav.sizeDelta = new Vector2(armorNav.sizeDelta.x, nav == armorNav ? 150 : 100);
            upgradeNav.sizeDelta = new Vector2(armorNav.sizeDelta.x, nav == upgradeNav ? 150 : 100);
        }
        
        private Recipe[] GetModRecipes()
        {
            List<Recipe> unlockedRecipes = new();
            
            for (int i = 0; i < PersistentPlayerState.UnlockedModRecipes.Count; i++)
            {
                unlockedRecipes.Add(PersistentPlayerState.UnlockedModRecipes[i].GetRecipeInstance());
            }

            return unlockedRecipes.ToArray();
        }
        
        private Recipe[] GetEquipmentRecipes()
        {
            List<Recipe> unlockedRecipes = new();
            
            for (int i = 0; i < PersistentPlayerState.UnlockedEquipmentRecipes.Count; i++)
            {
                unlockedRecipes.Add(PersistentPlayerState.UnlockedEquipmentRecipes[i].GetRecipeInstance());
            }

            return unlockedRecipes.ToArray();
        }
    }
}