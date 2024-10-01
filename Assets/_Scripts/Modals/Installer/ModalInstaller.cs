using Container;
using Factory;
using TinyRogue;
using UnityEngine;
using Views;
using Zenject;

namespace Modals.Installer
{
    [CreateAssetMenu(fileName = "ModalInstaller", menuName = "Installer/ModalInstaller")]
    public class ModalInstaller : ScriptableObjectInstaller<ModalInstaller>
    {
        [SerializeField] private WeaponDataModalView _weaponDataModalPrefab;
        [SerializeField] private EnemyInfoModalView _enemyInfoModalPrefab;
        [SerializeField] private DeathModalView _deathModalPrefab;
        [SerializeField] private CharacterCreationModalView _characterCreationModalPrefab;
        [SerializeField] private ChoiceModalView _choiceModalPrefab;
        [SerializeField] private CraftModalUIView craftModalUIPrefab;
        [SerializeField] private UnlockRecipeUIModal _unlockRecipeUIPrefab;
        [SerializeField] private SkillCraftingModalView _skillCraftingModalPrefab;

        [Header("Choice Modal")]
        [SerializeField] private ChoiceDefinition[] choiceDefinitions;
        public override void InstallBindings()
        {
            Container.Bind<WeaponDataModalView>().FromInstance(_weaponDataModalPrefab).AsSingle();
            Container.Bind<EnemyInfoModalView>().FromInstance(_enemyInfoModalPrefab).AsSingle();
            Container.Bind<DeathModalView>().FromInstance(_deathModalPrefab).AsSingle();
            Container.Bind<CharacterCreationModalView>().FromInstance(_characterCreationModalPrefab).AsSingle();
            Container.Bind<ChoiceModalView>().FromInstance(_choiceModalPrefab).AsSingle();
            Container.Bind<CraftModalUIView>().FromInstance(craftModalUIPrefab).AsSingle();
            Container.Bind<UnlockRecipeUIModal>().FromInstance(_unlockRecipeUIPrefab).AsSingle();
            Container.Bind<SkillCraftingModalView>().FromInstance(_skillCraftingModalPrefab).AsSingle();

            Container.Bind<ChoiceContainer>().FromInstance(new(choiceDefinitions)).AsSingle();

            Container.Bind<ModalFactory>().AsSingle();
        }
    }
}