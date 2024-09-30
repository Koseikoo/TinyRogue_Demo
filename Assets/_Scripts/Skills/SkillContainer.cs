using System.Collections.Generic;

namespace Container
{
    public class SkillContainer
    {
        private List<PlayerSkill> _skills;
        
        public SkillContainer(List<PlayerSkill> skills)
        {
            _skills = new(skills);
        }

        public PlayerSkill GetRandom()
        {
            return _skills.Random();
        }
    }
}