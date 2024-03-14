using Container;
using Factory;
using UnityEngine;
using Views;
using Zenject;

namespace Modals.Installer
{
    [CreateAssetMenu(fileName = "ModalInstaller", menuName = "Installer/ModalInstaller")]
    public class ModalInstaller : ScriptableObjectInstaller<ModalInstaller>
    {
        [SerializeField] private EnemyInfoModalView _enemyInfoModalPrefab;
        [SerializeField] private DeathModalView _deathModalPrefab;
        [SerializeField] private CharacterCreationModalView _characterCreationModalPrefab;
        [SerializeField] private ChoiceModalView _choiceModalPrefab;
        [SerializeField] private CraftModalUIView craftModalUIPrefab;
        [SerializeField] private UnlockRecipeUIModal _unlockRecipeUIPrefab;

        [Header("Choice Modal")]
        [SerializeField] private ChoiceDefinition NextIslandChoice;
        [SerializeField] private ChoiceDefinition ToShipChoice;
        [SerializeField] private ChoiceDefinition IncreaseHealthChoice;
        [SerializeField] private ChoiceDefinition IncreaseDamageChoice;
        public override void InstallBindings()
        {
            Container.Bind<EnemyInfoModalView>().FromInstance(_enemyInfoModalPrefab).AsSingle();
            Container.Bind<DeathModalView>().FromInstance(_deathModalPrefab).AsSingle();
            Container.Bind<CharacterCreationModalView>().FromInstance(_characterCreationModalPrefab).AsSingle();
            Container.Bind<ChoiceModalView>().FromInstance(_choiceModalPrefab).AsSingle();
            Container.Bind<CraftModalUIView>().FromInstance(craftModalUIPrefab).AsSingle();
            Container.Bind<UnlockRecipeUIModal>().FromInstance(_unlockRecipeUIPrefab).AsSingle();

            Container.Bind<ChoiceContainer>().FromInstance(new(NextIslandChoice, ToShipChoice, IncreaseHealthChoice, IncreaseDamageChoice)).AsSingle();

            Container.Bind<ModalFactory>().AsSingle();
        }
    }
}