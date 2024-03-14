using Factories;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Views;

namespace Installer
{
    [CreateAssetMenu(fileName = "WeaponInstaller", menuName = "Installer/WeaponInstaller")]
    public class WeaponInstaller : ScriptableObjectInstaller<WeaponInstaller>
    {
        [SerializeField] private WeaponView weaponPrefab;
        [SerializeField] private SwordView swordPrefab;

        [SerializeField] private ActionIndicatorView actionIndicatorPrefab;
        public override void InstallBindings()
        {
            Container.Bind<WeaponView>().FromInstance(weaponPrefab).AsSingle();
            Container.Bind<SwordView>().FromInstance(swordPrefab).AsSingle();
            Container.Bind<ActionIndicatorView>().FromInstance(actionIndicatorPrefab).AsSingle();

            Container.Bind<WeaponFactory>().AsSingle();
            Container.Bind<ActionIndicatorFactory>().AsSingle();
            
        }
    }
}