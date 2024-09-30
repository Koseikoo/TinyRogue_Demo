using Models;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace Views
{
    public class CoreSkillUI
    {
        [SerializeField] private Image SwipeSkillView;
        [SerializeField] private Image TapSkillView;
        [SerializeField] private Image PassiveSkillView;
        
        [SerializeField] private ProceduralImage SwipeSkillProgress;
        [SerializeField] private ProceduralImage TapSkillProgress;
        [SerializeField] private ProceduralImage PassiveSkillProgress;

        public void Initialize(Player player)
        {
            
        }
    }
}