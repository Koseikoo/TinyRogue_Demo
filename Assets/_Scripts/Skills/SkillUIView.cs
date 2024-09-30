using Models;
using TinyRogue;
using UnityEngine;

namespace Views
{
    using UniRx;
    public class SkillUIView : MonoBehaviour
    {
        [SerializeField] private PlayerSkillView skillPrefab;
        [SerializeField] private Transform skillViewParent;
        private Player _player;
        
        public void Initialize(Player player)
        {
            _player = player;

            _player.UnlockedSkills.ObserveAdd()
                .Subscribe(skill => OnAddSkill(skill.Value)).AddTo(this);
        }

        private void OnAddSkill(PlayerSkill skill)
        {
            PlayerSkillView view = Instantiate(skillPrefab, skillViewParent);
            view.Initialize(skill);
        }
    }
}