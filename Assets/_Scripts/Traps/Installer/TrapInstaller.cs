using Container;
using Factories;
using UnityEngine;
using Views;
using Zenject;

namespace Installer
{
    [CreateAssetMenu(fileName = "TrapInstaller", menuName = "Installer/TrapInstaller")]
    public class TrapInstaller : ScriptableObjectInstaller<TrapInstaller>
    {
        [SerializeField] private TrapView _trapPrefab;
        [SerializeField] private TrapDefinition _trapDefinition;
        public override void InstallBindings()
        {
            Container.Bind<TrapFactory>().AsSingle();
            Container.Bind<TrapView>().FromInstance(_trapPrefab).AsSingle();
            Container.Bind<TrapDefinitionContainer>().FromInstance(new(_trapDefinition)).AsSingle();
        }
    }
}