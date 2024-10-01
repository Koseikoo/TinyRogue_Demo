using System.Collections.Generic;
using Container;
using Factory;
using Zenject;

namespace TinyRogue
{
    public class SkillCraftingManager
    {
        [Inject] private ModalFactory _modalFactory;
        [Inject] private SkillContainer _skillContainer;

        public void OpenCraftingModal()
        {
            List<PlayerSkill> skills = GetCraftingSkills();
            _modalFactory.CreateSkillCraftingModal(skills);
        }

        private List<PlayerSkill> GetCraftingSkills()
        {
            List<PlayerSkill> skills = new();
            
            for (int i = 0; i < 3; i++)
            {
                skills.Add(_skillContainer.GetRandom());
            }
            return skills;
        }
        
        
    }
}