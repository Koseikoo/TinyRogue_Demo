using Models;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class RecipeProgressView : MonoBehaviour
    {
        private const float ProgressAddAmount = .4f;
        private const float ResetSpeedMult = .2f;
        [SerializeField] private Image progressImage;

        private Recipe _recipe;
        
        private void Update()
        {
            if(_recipe == null || _recipe.CraftProgress01.Value <= 0)
                return;

            if (_recipe.CraftProgress01.Value >= 1)
            {
                _recipe.CraftProgress01.Value = 0;
            }

            _recipe.CraftProgress01.Value -= Time.deltaTime * ResetSpeedMult;
        }

        public void Initialize(Recipe recipe)
        {
            _recipe = recipe;

            recipe.CraftProgress01.Subscribe(UpdateView).AddTo(this);
            
        }
        
        public void OnIconTap()
        {
            if(CanCraftRecipe())
                _recipe.CraftProgress01.Value += ProgressAddAmount;
        }

        private void UpdateView(float amount)
        {
            progressImage.fillAmount = amount;
        }

        private bool CanCraftRecipe()
        {
            for (int i = 0; i < _recipe.Cost.Length; i++)
            {
                ResourceCost cost = _recipe.Cost[i];
                if (!GameStateContainer.Player.Bag.HasResources(cost.Type, cost.Amount))
                    return false;
            }

            return true;
        }
    }
}