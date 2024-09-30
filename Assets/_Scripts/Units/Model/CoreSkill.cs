using System;

namespace Models
{
    public class CoreSkill
    {
        private SkillType _type;

        private Action OnTurnEnd;
        
        private void OnActivation()
        {
            switch (_type)
            {
                case SkillType.Attack:
                    break;
                
                case SkillType.Arrow:
                    break;
                
                case SkillType.ComboEnergy:
                    break;
            }
        }
    }
}