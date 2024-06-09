using System.Collections.Generic;
using Models;
using TinyRogue;
using UnityEngine;

namespace Views
{
    public class WeaponSkillTreeModalView : MonoBehaviour, IView<WeaponData>
    {
        [SerializeField]
        private Transform _skillParent;
        [SerializeField]
        private WeaponSkillButtonView _skillButtonPrefab;

        private List<WeaponSkillButtonView> _spawnedSkillButtons = new();

        private WeaponData _model;
        private WeaponType _weaponType;
        private WeaponSkill _initialSkill;
        
        public void Initialize(WeaponData model)
        {
            _model = model;
            _weaponType = model.GetWeaponType();
            _initialSkill = GameStateContainer.InitialSkillDict[_weaponType];
            List<WeaponSkill> current = new();
            List<WeaponSkill> next = new(_initialSkill.ConnectedSkills);
            
            int x = 0;
            int y = 0;
            
            // unlock initial skill
            SpawnSkill(_initialSkill, x, y);

            while (next.Count > 0)
            {
                current = new(next);
                next.Clear();
                y -= 300;
                x = 0;
                for (int i = 0; i < current.Count; i++)
                {
                    SpawnSkill(current[i], x, y);
                    next.AddRange(current[i].ConnectedSkills);
                    x += 300;
                }
            }
        }
        
        public void Destroy()
        {
            Destroy(gameObject);
        }

        private void UpdateAllBorders()
        {
            foreach (WeaponSkillButtonView view in _spawnedSkillButtons)
            {
                view.UpdateBorder();
            }
        }

        private void SpawnSkill(WeaponSkill skill, int x, int y)
        {
            WeaponSkillButtonView skillView = Instantiate(_skillButtonPrefab, _skillParent);
            _spawnedSkillButtons.Add(skillView);
            skillView.AddButtonEvent(UpdateAllBorders);
            skillView.Initialize(skill, _model);
            skillView.transform.localPosition = new Vector3(x, y, 0);
        }
        
    }
}