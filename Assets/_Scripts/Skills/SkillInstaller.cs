using System.Collections.Generic;
using Container;
using TinyRogue;
using UnityEngine;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(fileName = "SkillInstaller", menuName = "Installer/SkillInstaller")]
    public class SkillInstaller : ScriptableObjectInstaller<SkillInstaller>
    {
        [SerializeField] private List<PlayerSkill> skills;
        public override void InstallBindings()
        {
            Container.Bind<SkillContainer>().FromInstance(new(skills)).AsSingle();
            Container.Bind<SkillCraftingManager>().AsSingle();
        }
    }
}