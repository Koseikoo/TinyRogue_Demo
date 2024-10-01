using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TinyRogue
{
    public class SkillRecipeView : MonoBehaviour
    {
        [SerializeField] private Image Icon;
        [SerializeField] private TextMeshProUGUI DescriptionText;
        [SerializeField] private TextMeshProUGUI CostText;
        
        private PlayerSkill _skill;
        private Action<PlayerSkill> _onClick;
        
        public void Initialize(PlayerSkill skill, Action<PlayerSkill> onClick)
        {
            _skill = skill;
            _onClick = onClick;

            Icon.sprite = skill.Sprite;
            DescriptionText.text = skill.Description;
            CostText.text = $"Cost: {skill.Cost}";
        }

        public void OnClick()
        {
            _onClick?.Invoke(_skill);
        }
    }
}