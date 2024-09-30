using UnityEngine;
using UnityEngine.UI;

namespace TinyRogue
{
    public class PlayerSkillView : MonoBehaviour
    {
        [SerializeField] private Image skillIcon;
        
        private PlayerSkill _skill;
        
        public void Initialize(PlayerSkill skill)
        {
            _skill = skill;

            skillIcon.sprite = skill.Sprite;
        }
    }
}