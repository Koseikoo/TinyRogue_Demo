using System.Collections.Generic;
using Game;
using Models;
using UnityEngine;
using Zenject;

namespace TinyRogue
{
    public class SkillCraftingModalView : MonoBehaviour
    {
        [SerializeField] private RectTransform recipeParent;
        [SerializeField] private SkillRecipeView recipePrefab;
        
        [Inject] private PlayerManager _playerManager;
        
        public void Initialize(List<PlayerSkill> skills)
        {
            foreach (PlayerSkill skill in skills)
            {
                SkillRecipeView recipeView = Instantiate(recipePrefab, recipeParent);
                recipeView.Initialize(skill, TryBuySkill);
            }
        }

        public void DestroyModal()
        {
            Destroy(gameObject);
        }

        private void TryBuySkill(PlayerSkill skill)
        {
            _playerManager.Player.AddSkill(skill);
            DestroyModal();
        }
    }
}