using Factories;
using TinyRogue;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;
using Views;

namespace Installer
{
    [CreateAssetMenu(fileName = "WeaponInstaller", menuName = "Installer/WeaponInstaller")]
    public class WeaponInstaller : ScriptableObjectInstaller<WeaponInstaller>
    {
        [SerializeField] private ActionIndicatorView actionIndicatorPrefab;
        [SerializeField] private WeaponView weaponViewPrefab;
        public override void InstallBindings()
        {
            Container.Bind<ActionIndicatorView>().FromInstance(actionIndicatorPrefab).AsSingle();
            Container.Bind<WeaponView>().FromInstance(weaponViewPrefab).AsSingle();
            
            Container.Bind<ActionIndicatorFactory>().AsSingle();
            Container.Bind<WeaponFactory>().AsSingle();
        }
    }
}