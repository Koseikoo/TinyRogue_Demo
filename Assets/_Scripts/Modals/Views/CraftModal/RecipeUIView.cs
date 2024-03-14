using System;
using Installer;
using Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Views
{
    public class RecipeUIView : MonoBehaviour
    {
        [SerializeField] private ResourceCostRenderer[] _recipeCostRenderer;
        [SerializeField] private Image _recipeIcon;
        [SerializeField] private RecipeProgressView _recipeProgressView;
        [SerializeField] private GameObject _preview;
        [SerializeField] private GameObject _costParent;

        [Inject] private ItemIconContainer _itemIconContainer;
        [Inject] private ItemContainer _itemContainer;

        private Recipe _recipe;
        private Bag _playerBag;
        public void Initialize(Recipe recipe = null)
        {
            _recipe = recipe;
            _playerBag = GameStateContainer.Player.Bag;
            if (recipe == null)
            {
                DisableSlot();
                return;
            }

            _playerBag.OnLootAdded.Subscribe(_ => Render(_recipe)).AddTo(this);
            _recipe.CraftProgress01.Where(p => p >= 1).Subscribe(_ => CraftItem()).AddTo(this);
            
            Render(recipe);
            _recipeProgressView.Initialize(recipe);
        }

        

        public void RenderPreview()
        {
            gameObject.SetActive(true);
            
            _preview.SetActive(true);
            _recipeIcon.gameObject.SetActive(false);
            _costParent.SetActive(false);
            Debug.Log("RenderPreview Here");
        }

        private void CraftItem()
        {
            for (int i = 0; i < _recipe.Cost.Length; i++)
            {
                var cost = _recipe.Cost[i];
                _playerBag.RemoveItem(cost.Type, cost.Amount);
            }

            Loot loot = new(0);
            
            if (_recipe.Type == RecipeType.Equipment)
            {
                loot.Equipment = new() { _itemContainer.GetEquipment(_recipe.Output) };
            }
            else if (_recipe.Type == RecipeType.Mod)
            {
                loot.Mods = new() { _recipe.Output.GetModInstance(1) };
            }

            _playerBag.AddLoot(loot);
        }

        private void Render(Recipe recipe)
        {
            for (int i = 0; i < recipe.Cost.Length; i++)
            {
                _recipeCostRenderer[i].Render(recipe.Cost[i]);
            }

            _recipeIcon.sprite = _itemIconContainer.GetItemIcon(recipe.Output);
            gameObject.SetActive(true);
            
            _preview.SetActive(false);
            _recipeIcon.gameObject.SetActive(true);
            _costParent.SetActive(true);
            
        }

        private void DisableSlot()
        {
            gameObject.SetActive(false);
        }
    }
}